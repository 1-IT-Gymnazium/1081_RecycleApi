using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using NodaTime;
using Recycle.Api.Settings;
using Recycle.Data;
using Recycle.Data.Entities;
using Recycle.Data.Interfaces;
using System.Net.Mail;

namespace Recycle.Api.Services;

public class EmailSenderService
{
    private readonly AppDbContext _dbContext;
    private readonly SmtpSettings _smtpSettings;
    private readonly EnviromentSettings _enviromentSettings;
    public EmailSenderService(
        AppDbContext appDbContext,
        IOptions<SmtpSettings> options,
        IOptions<EnviromentSettings> enviromentSettings)
    {
        _dbContext = appDbContext;
        _smtpSettings = options.Value;
        _enviromentSettings = enviromentSettings.Value;
    }
    public async Task SendEmailsAsync()
    {
        var unsentMails = await _dbContext.Emails.Where(x => !x.Sent).ToListAsync();
        foreach (var unsent in unsentMails)
        {
            using var mail = new MailMessage
            {
                Subject = unsent.Subject,
                Body = unsent.Body,
                IsBodyHtml = false,
                From = new MailAddress(unsent.FromEmail, unsent.FromName),
            };
            mail.To.Add(new MailAddress(unsent.RecipientEmail, unsent.RecipientName));

            try
            {
                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                await smtp.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port);
                await smtp.AuthenticateAsync(_smtpSettings.UserName, _smtpSettings.Password);
                await smtp.SendAsync((MimeMessage)mail);

                unsent.Sent = true;
            }
            catch (Exception ex)
            {
            }
        }
    }
}
