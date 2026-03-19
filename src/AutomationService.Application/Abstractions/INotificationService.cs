using AutomationService.Application.Dtos;

namespace AutomationService.Application.Abstractions;

public interface INotificationService
{
    Task NotifyFeedingAsync(FeedingNotificationDto notification, CancellationToken cancellationToken = default);
    Task NotifyRuleTriggeredAsync(RuleTriggeredNotificationDto notification, CancellationToken cancellationToken = default);
}
