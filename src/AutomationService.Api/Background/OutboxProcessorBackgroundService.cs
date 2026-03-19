using AutomationService.Application.Abstractions;
using AutomationService.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AutomationService.Api.Background;

public sealed class OutboxProcessorBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxOptions> options,
    ILogger<OutboxProcessorBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(1, options.Value.PollIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
                await processor.ProcessAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox processor loop failed.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}
