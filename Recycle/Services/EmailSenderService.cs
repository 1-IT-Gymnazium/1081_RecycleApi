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
using System.Threading;

namespace Recycle.Api.Services;

/// <summary>
/// Handles queuing and sending of emails using configured SMTP settings.
/// </summary>
public class EmailSenderService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly AppDbContext _dbContext;
    private readonly IClock _clock;

    public EmailSenderService(
        IClock clock,
        AppDbContext dbContext,
        IOptions<SmtpSettings> smtpSettings
        )
    {
        _clock = clock;
        _dbContext = dbContext;
        _smtpSettings = smtpSettings.Value;
    }

    /// <summary>
    /// Queues an email to be sent later by saving it to the database.
    /// </summary>
    public async Task AddEmailToSendAsync(
        string receiver,
        string subject,
        string body
        )
    {
        var now = _clock.GetCurrentInstant();

        var newMail = new EmailMessage
        {
            Body = body,
            Subject = subject,
            Receiver = receiver,
            Sender = _smtpSettings.Sender,
            ScheduledAt = now,
        }.SetCreateBySystem(now);

        _dbContext.Emails.Add(newMail);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Sends all unsent emails from the database using the configured SMTP server.
    /// </summary>
    public async Task SendEmailsAsync()
    {
        var mails = await _dbContext.Set<EmailMessage>().Where(x => x.SentAt == null).ToListAsync();
        foreach (var mail in mails)
        {
            using var notif = new MailMessage
            {
                Subject = mail.Subject,
                Body = mail.Body,
                IsBodyHtml = true,
                From = new MailAddress(mail.Sender),
            };
            notif.To.Add(new MailAddress(mail.Receiver));

            try
            {
                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                await smtp.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port);
                await smtp.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
                await smtp.SendAsync((MimeMessage)notif);

                mail.SentAt = _clock.GetCurrentInstant();
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // log error, notify someone
            }
            finally
            {
                // nothing to do right now
            }
        }
    }
}
