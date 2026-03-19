using AutomationService.Domain.Models;

namespace AutomationService.Application.Abstractions;

public interface IStateStore
{
    Task<DateTimeOffset?> GetLastFeedingAsync(Guid aquariumId, CancellationToken cancellationToken = default);
    Task SetLastFeedingAsync(Guid aquariumId, DateTimeOffset timestamp, CancellationToken cancellationToken = default);
    Task<int?> GetFeedingIntervalMinutesAsync(Guid aquariumId, CancellationToken cancellationToken = default);
    Task SetFeedingIntervalMinutesAsync(Guid aquariumId, int intervalMinutes, CancellationToken cancellationToken = default);
    Task<bool> TrySetFlagAsync(string key, TimeSpan ttl, CancellationToken cancellationToken = default);
    Task RemoveKeyAsync(string key, CancellationToken cancellationToken = default);
    Task SetLastSensorStateAsync(Guid aquariumId, AquariumData data, CancellationToken cancellationToken = default);
}
