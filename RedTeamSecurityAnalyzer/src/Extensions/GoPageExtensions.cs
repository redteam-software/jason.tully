using Spectre.Console;
using Spectre.Console.Rendering;
using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using System.Threading.Channels;

namespace RedTeamSecurityAnalyzer.Extensions;

public record FrameUrl(string Name, string Source, string Url);
public record NavigationLink(string Id, string Name, string Url, IElementHandle ElementHandle);

public static class GoPageExtensions
{
    public static IRenderable CookieParamsToTable(this IEnumerable<CookieParam> cookieParams, string title, bool border = false)
    {
        var cookieTable = new Table().AddColumns("Name", "Value").Border(border ? TableBorder.Rounded : TableBorder.None).BorderColor(Color.Grey);

        var titleRule = new Rule(title);
        titleRule.Style = new Style(foreground: Color.White, background: Color.NavyBlue);

        foreach (var cookie in cookieParams)
        {
            cookieTable.AddRow(cookie.Name.TextValue(true), cookie.Value.TextValue(true));
        }

        return new Rows(titleRule, cookieTable).Expand();
    }

    public static async Task<List<string>> DebugFormInputs(this IPage page)
    {
        var fields = await page.EvaluateFunctionAsync<List<FormInputData>>(@"
            () => {
                const elements = Array.from(document.querySelectorAll('input, textarea, select'));
                return elements.map(el => ({
                    tag: el.tagName.toLowerCase(),
                    type: el.type || null,
                    name: el.name || null,
                    id: el.id || null,
                    value: el.value || null,
                    placeholder: el.placeholder || null
                }));
            }
        ");


        return fields.Select(x => $"{x.name ?? x.id} = \"{x.value}\"").ToList();




    }
    public class FormInputData
    {
        public string? tag { get; set; }
        public string? type { get; set; }
        public string? name { get; set; }
        public string? id { get; set; }
        public string? value { get; set; }



    }

    public static async Task DisplayPageFrameDetails(this IPage page)
    {
        var headerChannel = Channel.CreateBounded<Dictionary<string, string>>(1);

        if (page != null)
        {
            page.Response += SetHeaders;

            //  $"{new Uri(page.Url).PathAndQuery} Has {page.Frames.Count()} frames".WriteLine();

            foreach (var frame in page.Frames)
            {
                var cookieTable = new Table().AddColumns("Name", "Value").Title("Cookies").Border(TableBorder.Rounded).BorderColor(Color.Grey);

                var cookies = await frame.Page.GetCookiesAsync();
                foreach (var cookie in cookies)
                {
                    cookieTable.AddRow(cookie.Name.TextValue(true), cookie.Value.TextValue(true));
                }

                var headerTable = new Table().AddColumns("Name", "Value").Title("Headers").Border(TableBorder.Rounded).BorderColor(Color.Grey);

                await foreach (var item in headerChannel.Reader.ReadAllAsync())
                {
                    foreach (var (key, value) in item)
                    {
                        headerTable.AddRow(key.TextValue(true), value.TextValue(true));
                    }
                }

                var panelContent = new Table().HideHeaders().AddColumn("").Expand().Border(TableBorder.None);
                panelContent.AddRow(headerTable);
                panelContent.AddRow(cookieTable);
                var panel = new Panel(panelContent);
                panel.Header = new PanelHeader($"Frame {frame.Url}").Centered();
                AnsiConsole.Write(panel);

                page.Response -= SetHeaders;
            }

            void SetHeaders(object? sender, ResponseCreatedEventArgs e)
            {
                var response = e.Response;
                var headers = response.Headers;

                headerChannel.Writer.TryWrite(headers);
                headerChannel.Writer.TryComplete();
            }
            ;
        }
    }

    public static async Task<IElementHandle?> FindAnyElementHandle(this IPage page, string selector)
    {
        var element = await page.QuerySelectorAsync(selector);
        if (element != null)
        {
            return element;
        }


        var frameStack = new Stack<IFrame>();
        foreach (var item in page.Frames)
        {
            frameStack.Push(item);
        }

        var processedStack = new HashSet<string>();

        IElementHandle? elementHandle = null;

        while (frameStack.Count > 0)
        {
            if (frameStack.TryPeek(out var parent))
            {

                elementHandle = await parent.QuerySelectorAsync(selector);
                if (elementHandle != null)
                {
                    return elementHandle;
                }


                if (parent.ChildFrames.Any() && !processedStack.Contains(parent.Id))
                {
                    foreach (var item in parent.ChildFrames)
                    {
                        frameStack.Push(item);
                    }

                    processedStack.Add(parent.Id);
                }
                else
                {
                    var item = frameStack.Pop();
                    elementHandle = await item.QuerySelectorAsync(selector);
                    if (elementHandle != null)
                    {
                        return elementHandle;
                    }
                    processedStack.Add(item.Id);

                }
            }
        }

        return elementHandle;
    }

    public static async Task<IEnumerable<CookieParam>> GetAllFrameCookies(this IPage page)
    {
        var tasks = page.Frames.Select(async frame => await frame.Page.GetCookieParams());
        var outcome = await Task.WhenAll(tasks);

        return outcome.SelectMany(x => x);
    }

    public static async Task<IEnumerable<KeyValuePair<string, string>>> GetAllFrameHeaders(this IPage page)
    {
        var tasks = page.Frames.Select(async frame => await frame.Page.GetResponseHeaders());
        var outcome = await Task.WhenAll(tasks);

        return outcome.SelectMany(x => x);
    }

    public static async Task<IEnumerable<FrameUrl>> GetAllFrameUrls(this IPage page)
    {
        var bag = new ConcurrentBag<FrameUrl>();

        var anchors = () => page.QuerySelectorAllAsync(".sidenavlinks");
        var frames = page.Frames.Select(async x => await x.Page.QuerySelectorAllAsync("a"));

        var tasks = new List<Task<IElementHandle[]?>>();
        tasks.Add(anchors());
        foreach (var item in page.Frames)
        {
            // tasks.Add(item.QuerySelectorAllAsync("a"));
        }
        var allAnchors = await Task.WhenAll(tasks);
        //foreach (var frame in page.Frames)
        //{
        //    var links = await frame.EvaluateFunctionAsync<string[]>(@"() => {
        //    return Array.from(document.querySelectorAll('a')).map(a => a.href);
        //}");
        //    foreach (var link in links)
        //    {
        //        bag.Add(new FrameUrl(frame.Url, link));
        //    }
        //}
        return bag;
    }

    public static async Task<IReadOnlyList<CookieParam>> GetCookieParams(this IPage page)
    {
        var cookies = await page.GetCookiesAsync();
        return cookies.Select(x => new CookieParam()
        {
            Name = x.Name,
            Value = x.Value,
            Domain = new Uri(page.Url).Host,
            HttpOnly = x.HttpOnly,
            Secure = x.Secure,
            Expires = x.Expires
        }).ToList().AsReadOnly();
    }

    public static async Task<IReadOnlyList<NavigationLink>> GetMenuLinksAsync(this IPage page)
    {
        var menuLinks = await page.QuerySelectorAllAsync(".sidenavlinks");
        var asyncEnumerable = menuLinks.ToAsyncEnumerable();
        var bag = new ConcurrentBag<NavigationLink>();
        await foreach (var link in asyncEnumerable)
        {
            var id = await link.EvaluateFunctionAsync<string>("el => el.getAttribute('data-id')");
            var title = await link.EvaluateFunctionAsync<string>("el => el.getAttribute('data-title')");
            var href = await link.EvaluateFunctionAsync<string>("el => el.getAttribute('href')");
            if (!string.IsNullOrWhiteSpace(href))
            {
                bag.Add(new NavigationLink(id ?? string.Empty, title ?? string.Empty, href, link));
            }
        }
        return bag.ToList().AsReadOnly();
    }
    public static async Task<IReadOnlyDictionary<string, string>> GetResponseHeaders(this IPage page)
    {
        var headerChannel = Channel.CreateBounded<Dictionary<string, string>>(1);
        page.Response += SetHeaders;

        var concurrentDictionary = new ConcurrentDictionary<string, string>();
        await foreach (var item in headerChannel.Reader.ReadAllAsync())
        {
            foreach (var (key, value) in item)
            {
                concurrentDictionary.AddOrUpdate(key, value, (k, v) => value);
            }
        }
        page.Response -= SetHeaders;

        return concurrentDictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        void SetHeaders(object? sender, ResponseCreatedEventArgs e)
        {
            var response = e.Response;
            var headers = response.Headers;

            headerChannel.Writer.TryWrite(headers);
            headerChannel.Writer.TryComplete();
        }
    }
    public static IRenderable HeadersToTable(this IReadOnlyDictionary<string, string> headers, string title, bool border = false)
    {
        var headerTable = new Table().AddColumns("Name", "Value").Border(border ? TableBorder.Rounded : TableBorder.None).BorderColor(Color.Grey);

        var titleRule = new Rule(title);
        titleRule.Style = new Style(foreground: Color.White, background: Color.NavyBlue);
        foreach (var (key, value) in headers)
        {
            try
            {
                headerTable.AddRow(key.TextValue(true), value.TextValue(true));
            }
            catch (Exception ex)
            {
                headerTable.AddRow(new Text(value), new Text(value));
            }
        }

        return new Rows(titleRule, headerTable).Expand();
    }
    public static async Task<IResponse?> PostFormAsync(this IPage page, TestCaseProperties formDetails, IProgressTaskNotificationService progressTaskNotificationService)
    {


        int responseCode = 200;

        progressTaskNotificationService.Information($"Submitting Form at {page.Url}");
        var logBuffer = new ConcurrentQueue<string>();

        if (formDetails.FormKeys.Any())
        {
            foreach (var item in formDetails.FormKeys)
            {
                try
                {
                    // progressTaskNotificationService.Information($"Setting form field {item.Key} to {item.Value} on {page.Url}");
                    var field = await page.QuerySelectorAsync(item.Key);

                    if (field != null)
                    {
                        await field.TypeAsync(item.Value);
                    }
                    else
                    {
                        progressTaskNotificationService.Information($"Form field {item.Key} not found.");
                    }
                }
                catch (Exception ex)
                {
                    await progressTaskNotificationService.NotifyAsync($"Error setting form field {item.Key} to {item.Value}.  {ex.Message.Error()}");
                }
            }

            var form = await page.DebugFormInputs();
            await progressTaskNotificationService.NotifyAsync($"Submitting form {formDetails.FormSubmitButton} to {page.Url}");
            try
            {

                foreach (var item in form)
                {
                    progressTaskNotificationService.Information(item);
                }
                //await page.SetRequestInterceptionAsync(true);
                page.Console += Page_Console;
                page.Response += Page_Response;
                // var result = await page.EvaluateExpressionAsync("document.getElementById(\"rfpadd\").submit();");
                page.PageError += Page_PageError;
                page.Popup += Page_Popup;
                //    page.RequestFailed += Page_RequestFailed;
                await page.ClickAsync(formDetails.FormSubmitButton);
                page.PageError -= Page_PageError;


                // Wait for navigation or response
                var response = await page.WaitForNavigationAsync(new NavigationOptions { Timeout = 0 });

                foreach (var log in logBuffer)
                {
                    progressTaskNotificationService.Information(log);
                }

                page.Console -= Page_Console;
                page.Response -= Page_Response;
                page.PageError -= Page_PageError;
                page.Popup -= Page_Popup;
                //     page.RequestFailed -= Page_RequestFailed;


                progressTaskNotificationService.Information($"Form Response: {(response != null ? response.Status.ToString() : "No Response")} on {page.Url}");
                return response ?? new PrivateResponse(responseCode);
            }
            catch (Exception ex)
            {
                await progressTaskNotificationService.NotifyAsync($"Error submitting form {formDetails.FormSubmitButton} to {page.Url}.  {ex.Message.Error()}");
            }
        }
        return default;

        void Page_PageError(object? sender, PageErrorEventArgs e)
        {
            logBuffer.Enqueue("PageError: " + e.Message);

        }
        void Page_Popup(object? sender, PopupEventArgs e)
        {
            logBuffer.Enqueue("Popup: " + e.PopupPage.Url);
        }
        void Page_RequestFailed(object? sender, RequestEventArgs e)
        {
            if (e.Request.Url.Contains("redteam"))
            {
                logBuffer.Enqueue($"RequestFailed: {e.Request.Url}  {e.Request.FailureText}");
            }
        }
        void Page_Console(object? sender, ConsoleEventArgs e)
        {
            logBuffer.Enqueue("Console: " + e.Message.Text);

        }
        void Page_Response(object? sender, ResponseCreatedEventArgs e)
        {
            if (e.Response.Url == page.Url)
            {
                Interlocked.Exchange(ref responseCode, (int)e.Response.Status);
                logBuffer.Enqueue($"Response: {e.Response.Url}  {e.Response.Status}");
            }
        }


    }
    internal class PrivateResponse : IResponse
    {
        public PrivateResponse(int status)
        {
            Status = (HttpStatusCode)status;

        }
        public IFrame Frame => throw new NotImplementedException();
        public bool FromCache => throw new NotImplementedException();
        public bool FromServiceWorker => throw new NotImplementedException();
        public Dictionary<string, string> Headers => throw new NotImplementedException();
        public bool Ok => Status == HttpStatusCode.OK;
        public RemoteAddress RemoteAddress => throw new NotImplementedException();
        public IRequest Request => throw new NotImplementedException();
        public SecurityDetails SecurityDetails => throw new NotImplementedException();
        public HttpStatusCode Status { get; init; }
        public string StatusText => throw new NotImplementedException();
        public string Url => throw new NotImplementedException();
        public ValueTask<byte[]> BufferAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JsonDocument> JsonAsync(JsonDocumentOptions options = default)
        {
            throw new NotImplementedException();
        }

        public Task<T> JsonAsync<T>(JsonSerializerOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> TextAsync()
        {
            return Task.FromResult(string.Empty);
        }
    }
}






//public static async Task<IResponse?> GoLoginAsync(this IPage page, string username, string password)
//{
//    await page.GoToAsync("https://go.redteam.com/manager/login.cfm");
//    await page.WaitForSelectorAsync("input[name='username']", new WaitForSelectorOptions() { Timeout = 0 });
//    var usernameField = await page.QuerySelectorAsync("input[name='username']");
//    if (usernameField != null) await usernameField.TypeAsync(username);

//    var passwordField = await page.QuerySelectorAsync("input[type='password']");
//    if (passwordField != null) await passwordField.TypeAsync(password);

//    await page.ClickAsync("input[value='login']");

//    // Wait for navigation or response
//    var response = await page.WaitForNavigationAsync();

//    if (!response.Ok)
//    {
//        return default;
//    }
//    return await page.GoToAsync("https://go.redteam.com/manager/product/dev_logins.cfm");
//}

///// <summary>
///// retrieves all GoLoginButton elements from the provided page.
///// </summary>
///// <param name="page"></param>
///// <returns></returns>
//public static async Task<IEnumerable<GoLoginButton>> GetLoginButtons(this IPage page)
//{
//    var buttons = await page.QuerySelectorAllAsync(".lobtn");
//    var asyncEnumerable = buttons.ToAsyncEnumerable();

//    var bag = new ConcurrentBag<GoLoginButton>();
//    await foreach (var button in asyncEnumerable)
//    {
//        var dataAjax = await button.EvaluateFunctionAsync<string>("el => el.getAttribute('data-ajax')");
//        var contactId = await button.EvaluateFunctionAsync<string>("el => el.getAttribute('data-rel')");
//        var companyId = await button.EvaluateFunctionAsync<string>("el => el.getAttribute('data-id')");
//        var goLoginButton = new GoLoginButton(dataAjax, contactId, contactId, companyId, button);
//        bag.Add(goLoginButton);
//    }
//    return bag;
//}
//}