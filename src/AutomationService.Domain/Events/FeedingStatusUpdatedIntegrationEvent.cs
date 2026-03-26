namespace AutomationService.Domain.Events;

public sealed record FeedingStatusUpdatedIntegrationEvent(
    Guid AquariumId,
    DateTimeOffset LastFeedingAt,
    DateTimeOffset NextFeedingAt,
    long RemainingSeconds,
    bool IsOverdue,
    string? Message);
