using AutomationService.Application.Abstractions;

namespace AutomationService.Application.Services;

public sealed class PollingOrchestrator(
    IAquariumDataClient aquariumDataClient,
    IAquariumRegistryClient aquariumRegistryClient,
    IStateStore stateStore,
    IEventDetectionService eventDetectionService,
    IFeedingService feedingService,
    IClock clock) : IPollingOrchestrator
{
    public async Task PollAllAsync(CancellationToken cancellationToken = default)
    {
        var aquariumIds = await aquariumRegistryClient.GetAquariumIdsAsync(cancellationToken);
        foreach (var aquariumId in aquariumIds)
        {
            var data = await aquariumDataClient.GetAquariumDataAsync(aquariumId, cancellationToken);
            await stateStore.SetLastSensorStateAsync(aquariumId, data, cancellationToken);
            await eventDetectionService.DetectAndPublishAsync(data, cancellationToken);
            await feedingService.EvaluateAsync(aquariumId, clock.UtcNow, cancellationToken);
        }
    }
}
