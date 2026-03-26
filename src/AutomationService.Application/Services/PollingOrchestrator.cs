using AutomationService.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace AutomationService.Application.Services;

public sealed class PollingOrchestrator(
    IAquariumDataClient aquariumDataClient,
    IAquariumRegistryClient aquariumRegistryClient,
    IStateStore stateStore,
    IEventDetectionService eventDetectionService,
    IFeedingService feedingService,
    IClock clock,
    ILogger<PollingOrchestrator> logger) : IPollingOrchestrator
{
    public async Task PollAllAsync(CancellationToken cancellationToken = default)
    {
        var aquariumIds = await aquariumRegistryClient.GetAquariumIdsAsync(cancellationToken);
        foreach (var aquariumId in aquariumIds)
        {
            var data = await aquariumDataClient.GetAquariumDataAsync(aquariumId, cancellationToken);
            await stateStore.SetLastSensorStateAsync(aquariumId, data, cancellationToken);
            await eventDetectionService.DetectAndPublishAsync(data, cancellationToken);
            var feedingStatus = await feedingService.EvaluateAsync(aquariumId, clock.UtcNow, cancellationToken);

            logger.LogInformation(
                "Created feeding status for aquarium {AquariumId}: NextFeedingAt={NextFeedingAt}, RemainingSeconds={RemainingSeconds}, IsOverdue={IsOverdue}",
                feedingStatus.AquariumId,
                feedingStatus.NextFeedingAt,
                feedingStatus.RemainingSeconds,
                feedingStatus.IsOverdue);
        }
    }
}
