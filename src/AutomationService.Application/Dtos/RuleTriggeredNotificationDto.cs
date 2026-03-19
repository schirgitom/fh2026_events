namespace AutomationService.Application.Dtos;

public sealed class RuleTriggeredNotificationDto
{
    public Guid AquariumId { get; init; }
    public string RuleName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }
}
