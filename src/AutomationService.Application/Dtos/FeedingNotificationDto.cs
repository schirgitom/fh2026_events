namespace AutomationService.Application.Dtos;

public sealed class FeedingNotificationDto
{
    public Guid AquariumId { get; init; }
    public DateTimeOffset LastFeedingAt { get; init; }
    public DateTimeOffset NextFeedingAt { get; init; }
    public long RemainingSeconds { get; init; }
    public bool IsOverdue { get; init; }
    public string? Message { get; init; }
}
