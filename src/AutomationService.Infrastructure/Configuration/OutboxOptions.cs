namespace AutomationService.Infrastructure.Configuration;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";
    public int BatchSize { get; init; } = 100;
    public int PollIntervalSeconds { get; init; } = 5;
}
