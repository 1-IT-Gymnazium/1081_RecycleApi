namespace Recycle.Api.Settings;

public class SmtpSettings
{
    public required string Host { get; set; }
    public required int Port { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public string Sender { get; set; } = null!;
}
