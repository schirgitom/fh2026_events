namespace AutomationService.Application.Dtos;

public sealed class FeedingStatusDto
{
    public Guid AquariumId { get; init; }
    public DateTimeOffset NextFeedingAt { get; init; }
    public long RemainingSeconds { get; init; }
    public bool IsOverdue { get; init; }
}
