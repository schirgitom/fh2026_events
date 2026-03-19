using AutomationService.Api.Hubs;
using AutomationService.Application.Abstractions;
using AutomationService.Application.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;

namespace AutomationService.Api.Services;

public sealed class SignalRNotificationService(
    IHubContext<NotificationHub> hubContext,
    ILogger<SignalRNotificationService> logger) : INotificationService
{
    public Task NotifyFeedingAsync(FeedingNotificationDto notification, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            aquariumId = notification.AquariumId,
            nextFeedingAt = notification.NextFeedingAt,
            remainingSeconds = notification.RemainingSeconds,
            isOverdue = notification.IsOverdue,
            message = notification.Message
        };

        var aquariumGroup = $"aquarium:{notification.AquariumId}";
        var allGroup = NotificationHub.GetAllAquariumsGroup();

        logger.LogInformation(
            "Sending SignalR feeding update for aquarium {AquariumId} to groups {AquariumGroup} and {AllGroup}.",
            notification.AquariumId,
            aquariumGroup,
            allGroup);

        return hubContext.Clients.Groups(
                aquariumGroup,
                allGroup)
            .SendAsync("feeding:update", payload, cancellationToken);
    }

    public Task NotifyRuleTriggeredAsync(RuleTriggeredNotificationDto notification, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            aquariumId = notification.AquariumId,
            ruleName = notification.RuleName,
            description = notification.Description,
            occurredAt = notification.OccurredAt
        };

        var aquariumGroup = $"aquarium:{notification.AquariumId}";
        var allGroup = NotificationHub.GetAllAquariumsGroup();

        logger.LogInformation(
            "Sending SignalR rule trigger for aquarium {AquariumId} ({RuleName}) to groups {AquariumGroup} and {AllGroup}.",
            notification.AquariumId,
            notification.RuleName,
            aquariumGroup,
            allGroup);

        return hubContext.Clients.Groups(
                aquariumGroup,
                allGroup)
            .SendAsync("rule:triggered", payload, cancellationToken);
    }
}
