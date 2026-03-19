namespace AutomationService.Domain.Events;

public sealed record RuleTriggeredIntegrationEvent(
    Guid AquariumId,
    string RuleName,
    string Description,
    DateTimeOffset OccurredAt);
