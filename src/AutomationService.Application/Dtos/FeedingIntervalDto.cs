namespace AutomationService.Application.Dtos;

public sealed class FeedingIntervalDto
{
    public Guid AquariumId { get; init; }
    public int IntervalMinutes { get; init; }
}
