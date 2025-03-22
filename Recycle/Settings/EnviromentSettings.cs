namespace Recycle.Api.Settings;

/// <summary>
/// Configuration for backend and frontend URLs, email sender info, and password reset paths.
/// </summary>
public class EnviromentSettings 
{
    public required string BackendHostUrl { get; set; }
    public required string FrontendHostUrl { get; set; }
    public required string FrontendConfirmUrl { get; set; }
    public required string SenderEmail { get; set; }
    public required string SenderName { get; set; }
    public string FrontendResetPasswordUrl { get; set; } = string.Empty;

}
