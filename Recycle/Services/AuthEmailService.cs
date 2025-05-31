using Microsoft.Extensions.Options;
using Recycle.Api.Settings;
using Recycle.Data.Entities.Identity;

namespace Recycle.Api.Services;

public class AuthEmailService : IAuthEmailService
{
    private readonly EmailSenderService _emailSender;
    private readonly EnviromentSettings _enviromentSettings;

    public AuthEmailService (
        EmailSenderService emailSender,
        IOptions<EnviromentSettings> envOptions
        )
    {
        _emailSender = emailSender;
        _enviromentSettings = envOptions.Value;
    }

    public async Task SendRegistrationConfirmationEmailAsync(ApplicationUser user, string token)
    {
        var confirmationUrl = $"{_enviromentSettings.FrontendHostUrl}/{_enviromentSettings.FrontendConfirmUrl}?token={Uri.EscapeDataString(token)}&email={user.Email}";

        var htmlBody = $@"
        <html>
        <head>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    background-color: #f4f4f4;
                    padding: 20px;
                }}
                .container {{
                    max-width: 600px;
                    margin: 0 auto;
                    background: #ffffff;
                    padding: 20px;
                    border-radius: 8px;
                    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                    text-align: center;
                }}
                .button {{
                    display: inline-block;
                    padding: 10px 20px;
                    font-size: 16px;
                    color: #fff;
                    background-color: #28a745;
                    text-decoration: none;
                    border-radius: 5px;
                    margin-top: 20px;
                }}
                .footer {{
                    margin-top: 20px;
                    font-size: 12px;
                    color: #777;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <h2>Confirm of registration</h2>
                <p>Click on the button to verify the email address:</p>
                <a href='{confirmationUrl}' class='button'>Verify e-mail</a>
                <p class='footer'>If you did not register on Recycle!, please ignore this email.</p>
            </div>
        </body>
        </html>";

        await _emailSender.AddEmailToSendAsync(user.Email, "Confirmation of registration", htmlBody);
    }
    public async Task SendPasswordResetEmailAsync(string email, string token)
    {
        var resetUrl = $"{_enviromentSettings.FrontendHostUrl}/{_enviromentSettings.FrontendResetPasswordUrl}?token={Uri.EscapeDataString(token)}&email={email}";

        var htmlBody = $@"
        <html>
        <head>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    background-color: #f4f4f4;
                    padding: 20px;
                }}
                .container {{
                    max-width: 600px;
                    margin: 0 auto;
                    background: #ffffff;
                    padding: 20px;
                    border-radius: 8px;
                    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                    text-align: center;
                }}
                .button {{
                    display: inline-block;
                    padding: 10px 20px;
                    font-size: 16px;
                    color: #fff;
                    background-color: #dc3545;
                    text-decoration: none;
                    border-radius: 5px;
                    margin-top: 20px;
                }}
                .footer {{
                    margin-top: 20px;
                    font-size: 12px;
                    color: #777;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <h2>Password Reset Request</h2>
                <p>If you requested a password reset, click the button below:</p>
                <a href='{resetUrl}' class='button'>Reset Password</a>
                <p class='footer'>If you did not request a password reset, please ignore this email.</p>
            </div>
        </body>
        </html>";
        await _emailSender.AddEmailToSendAsync(email, "Password Reset Request", htmlBody);
    }
}

