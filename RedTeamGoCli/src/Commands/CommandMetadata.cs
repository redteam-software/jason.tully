namespace RedTeamGoCli.Commands;


public static class CommandMetadata
{
    public static class SubCommandUser
    {
        public const string SubCommandName = "user";
        public const string SubCommandDescription = "A collection of user related commands";
        public static string Format()
        {
            return $"{SubCommandName} - {SubCommandDescription}";
        }

        public static class CommandUnifiedLoginSignup
        {
            public const string CommandName = "unified-login-signup";
            public const string CommandDescription = "Sends an invitation to unified login for an email.";
            public static string Format()
            {
                return $"{CommandName} - {CommandDescription}";
            }
        }

    }

    public static class SubCommandConfig
    {
        public const string SubCommandName = "config";
        public const string SubCommandDescription = "A collection of config related commands";
        public static string Format()
        {
            return $"{SubCommandName} - {SubCommandDescription}";
        }
        public static class CommandSshConfigsList
        {
            public const string CommandName = "ssh-config-list";
            public const string CommandDescription = "lists all configured hosts in .ssh/config";
            public static string Format()
            {
                return $"{CommandName} - {CommandDescription}";
            }
        }
        public static class CommandSshConfigsAdd
        {
            public const string CommandName = "ssh-config-list";
            public const string CommandDescription = "lists all configured hosts in .ssh/config";
            public static string Format()
            {
                return $"{CommandName} - {CommandDescription}";
            }
        }
    }
    public static class SubCommandLogs
    {
        public const string SubCommandName = "logs";
        public const string SubCommandDescription = "A collection of logs related commands";
        public static string Format()
        {
            return $"{SubCommandName} - {SubCommandDescription}";
        }

        public static class CommandGetRemoteLogs
        {
            public const string CommandName = "remote";
            public const string CommandDescription = "Gets remote logs for laravel go apps.  You must have an ssh host configured in your ./ssh/config file for this to work.";

            public static string Format()
            {
                return $"{CommandName} - {CommandDescription}";
            }
        }
        public static class CommandWriteLokiLog
        {
            public const string CommandName = "loki";
            public const string CommandDescription = "Pushes a message to loki in Grafana cloud.";

            public static string Format()
            {
                return $"{CommandName} - {CommandDescription}";
            }
        }

    }
    public static class SubCommandGit
    {
        public const string SubCommandName = "git";
        public const string SubCommandDescription = "A collection of git related commands.  --help for a list of all sub-commands.";
        public static string Format()
        {
            return $"{SubCommandName} - {SubCommandDescription}";
        }
        public static class CommandCherryPick
        {
            public const string CommandName = "cherry-pick";
            public const string CommandDescription = "This command will attempt to cherry-pick each commit individually, excluding any merge commits.  The end result will be a complete application of all changes made by the specified author between the given commit range.";

            public static string Format()
            {
                return $"{CommandName} - {CommandDescription}";
            }
        }
        public static class CommandListChanges
        {
            public const string CommandName = "list-changes";
            public const string CommandDescription = "Generates a list of changes filtered by commit ranges and or author.  This will only yield the names of the files changed not the actual diff.";

            public static string Format()
            {
                return $"{CommandName} - {CommandDescription}";
            }
        }

        public static class CommandListPullRequests
        {
            public const string CommandName = "list-prs";
            public const string CommandDescription = "Lists pull requests created by the specified author across all Go projects.";
            public static string Format()
            {
                return $"{CommandName} - {CommandDescription}";
            }
        }

        public static class CommandAutomatedPullRequest
        {
            public const string CommandName = "auto-pr";
            public const string CommandDescription = "Using the current project directory,  creates a pull request, auto merges it and displays the progress of the associated git hub acction.";

            public static string Format()
            {
                return $"{CommandName} - {CommandDescription}";
            }
        }
    }

    public static class SubCommandDeploy
    {
        public const string SubCommandName = "deploy";
        public const string SubCommandDescription = "A collection of deployment related commands. Use --help to list all subcommands";
        public static string Format()
        {
            return $"{SubCommandName} - {SubCommandDescription}";
        }
        public static class CommandGoSync
        {
            public const string CommandName = "go-sync";
            public const string CommandDescription = "Using the current project directory, starts a local file system monitor to synchronize changes between the local  Go project and the remotely deployed UAT instance.";

            public static string Format()
            {
                return $"{CommandName} - {CommandDescription}";
            }
        }
    }
}
public static class CommandParameterMetdata
{
    public static class Common
    {
        public const string Author = "author";
        public const string AuthorDescription = "The author name or email to filter commits by.";

        public const string HashStart = "start";
        public const string HashStartDescription = "The starting hash commit.";

        public const string HashEnd = "end";
        public const string HashEndDescription = "The ending hash commit.  Can be omitted.";

        public const string Since = "since";
        public const string SinceDescription = "Commits since date.  Can use short hand like \"1 week ago\"";

        public const string Until = "until";
        public const string UntilDescription = "Commits until  date.  Can use short hand like \"1 week ago\"";

        public const string Path = "path";
        public const string PathDescription = "The file system path to the git repository.";

        public const string AllBranches = "all";
        public const string AllBranchesDescription = "Search all branches for commits by the specified author.";

        public const string Interactive = "interactive";
        public const string InteractiveDescription = "Whether to run in interactive mode.";
    }



}
