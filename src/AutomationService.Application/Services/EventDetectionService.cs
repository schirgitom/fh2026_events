using AutomationService.Application.Abstractions;
using AutomationService.Domain.Events;
using AutomationService.Domain.Models;

namespace AutomationService.Application.Services;

public sealed class EventDetectionService(
    IEnumerable<IRule<AquariumData>> rules,
    IEventPublisher eventPublisher,
    IStateStore stateStore,
    IClock clock) : IEventDetectionService
{
    private static readonly TimeSpan RuleIdempotencyTtl = TimeSpan.FromMinutes(5);

    public async Task DetectAndPublishAsync(AquariumData data, CancellationToken cancellationToken = default)
    {
        foreach (var rule in rules)
        {
            if (!rule.IsSatisfied(data))
            {
                continue;
            }

            var key = $"rule:{data.AquariumId}:{rule.Name}:sent";
            var shouldPublish = await stateStore.TrySetFlagAsync(key, RuleIdempotencyTtl, cancellationToken);
            if (!shouldPublish)
            {
                continue;
            }

            var @event = new RuleTriggeredIntegrationEvent(
                data.AquariumId,
                rule.Name,
                rule.Description,
                clock.UtcNow);

            await eventPublisher.PublishAsync(@event, cancellationToken);
        }
    }
}
