
using Recycle.Api.Services;

namespace Recycle.Api.BackgroundServices;

/// <summary>
/// Background service that processes and sends queued emails on a timed interval.
/// </summary>
public class EmailSenderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailSenderBackgroundService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The application's service provider for creating scopes.</param>
    public EmailSenderBackgroundService(
        IServiceProvider serviceProvider
        )
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Executes the background task when the host starts.
    /// </summary>
    /// <param name="stoppingToken">Token to monitor for shutdown requests.</param>
    /// <returns>A Task representing the background operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await SendEmailsAsync(stoppingToken);
    }

    /// <summary>
    /// Continuously processes and sends emails every minute until cancellation is requested.
    /// </summary>
    /// <param name="stoppingToken">Token to monitor for shutdown requests.</param>
    /// <returns>A Task that completes when the operation is cancelled.</returns>
    private async Task SendEmailsAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<EmailSenderService>();
            await service!.SendEmailsAsync();

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
