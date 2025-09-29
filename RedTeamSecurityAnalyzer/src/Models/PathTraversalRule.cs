namespace RedTeamSecurityAnalyzer.Models;

public enum PathTraversalRule
{
    Basic = 100,
    URLEncoded = 101,
    Unicode = 102,
    HTMLEntities = 103,
    NullBytes = 104,
    Advanced = 105,
    ColdFusion = 106,
    FileUpload = 107,

    RateLimit = 109,
    FormData = 110,
    MultipartForms = 111,
    JSONPayload = 112,
    CFFormFields = 113,
    PostSizeLimit = 114
}