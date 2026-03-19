using System.Text.Json;
using AutomationService.Application.Abstractions;
using AutomationService.Application.Dtos;
using AutomationService.Domain.Events;
using Microsoft.Extensions.Logging;

namespace AutomationService.Infrastructure.Messaging;

public sealed class OutboxMessageDispatcher(
    INotificationService notificationService,
    ILogger<OutboxMessageDispatcher> logger) : IOutboxMessageDispatcher
{
    public async Task DispatchAsync(string eventType, string payload, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Dispatching outbox event type {EventType}.", eventType);

        if (eventType == typeof(FeedingStatusUpdatedIntegrationEvent).FullName)
        {
            var feeding = JsonSerializer.Deserialize<FeedingStatusUpdatedIntegrationEvent>(payload)
                          ?? throw new InvalidOperationException("Failed to deserialize feeding status event.");

            await notificationService.NotifyFeedingAsync(new FeedingNotificationDto
            {
                AquariumId = feeding.AquariumId,
                NextFeedingAt = feeding.NextFeedingAt,
                RemainingSeconds = feeding.RemainingSeconds,
                IsOverdue = feeding.IsOverdue,
                Message = feeding.Message
            }, cancellationToken);

            return;
        }

        if (eventType == typeof(RuleTriggeredIntegrationEvent).FullName)
        {
            var rule = JsonSerializer.Deserialize<RuleTriggeredIntegrationEvent>(payload)
                       ?? throw new InvalidOperationException("Failed to deserialize rule event.");

            await notificationService.NotifyRuleTriggeredAsync(new RuleTriggeredNotificationDto
            {
                AquariumId = rule.AquariumId,
                RuleName = rule.RuleName,
                Description = rule.Description,
                OccurredAt = rule.OccurredAt
            }, cancellationToken);

            return;
        }

        throw new InvalidOperationException($"Unsupported outbox event type '{eventType}'.");
    }
}
