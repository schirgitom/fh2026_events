namespace AutomationService.Application.Configuration;

public sealed class FeedingOptions
{
    public const string SectionName = "Feeding";
    public int DefaultIntervalMinutes { get; init; } = 480;
}
