namespace AutomationService.Api.Contracts;

public sealed class SetFeedingIntervalRequest
{
    public int IntervalMinutes { get; init; }
}
