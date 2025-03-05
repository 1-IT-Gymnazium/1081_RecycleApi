namespace Recycle.Api.Settings;

public class EnviromentSettings
{
    //PRAVDEDPODOBNE TO PRIJDE I SEM adresa serveru
    public required string FrontendHostUrl { get; set; }
    public required string FrontendConfirmUrl { get; set; }
    public required string SenderEmail { get; set; }
    public required string SenderName { get; set; }
    public string FrontendResetPasswordUrl { get; set; } = string.Empty;

}
