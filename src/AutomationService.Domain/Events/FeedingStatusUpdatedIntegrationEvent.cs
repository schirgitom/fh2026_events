namespace AutomationService.Domain.Events;

public sealed record FeedingStatusUpdatedIntegrationEvent(
    Guid AquariumId,
    DateTimeOffset NextFeedingAt,
    long RemainingSeconds,
    bool IsOverdue,
    string? Message);
