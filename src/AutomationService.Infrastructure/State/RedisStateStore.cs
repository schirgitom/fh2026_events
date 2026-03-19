using System.Text.Json;
using AutomationService.Application.Abstractions;
using AutomationService.Domain.Models;
using StackExchange.Redis;

namespace AutomationService.Infrastructure.State;

public sealed class RedisStateStore(IConnectionMultiplexer connectionMultiplexer) : IStateStore
{
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<DateTimeOffset?> GetLastFeedingAsync(Guid aquariumId, CancellationToken cancellationToken = default)
    {
        var value = await _database.StringGetAsync(GetLastFeedingKey(aquariumId));
        if (!value.HasValue || !DateTimeOffset.TryParse(value.ToString(), out var timestamp))
        {
            return null;
        }

        return timestamp;
    }

    public Task SetLastFeedingAsync(Guid aquariumId, DateTimeOffset timestamp, CancellationToken cancellationToken = default) =>
        _database.StringSetAsync(GetLastFeedingKey(aquariumId), timestamp.ToString("O"));

    public async Task<int?> GetFeedingIntervalMinutesAsync(Guid aquariumId, CancellationToken cancellationToken = default)
    {
        var value = await _database.StringGetAsync(GetIntervalKey(aquariumId));
        if (!value.HasValue || !int.TryParse(value.ToString(), out var interval))
        {
            return null;
        }

        return interval;
    }

    public Task SetFeedingIntervalMinutesAsync(Guid aquariumId, int intervalMinutes, CancellationToken cancellationToken = default) =>
        _database.StringSetAsync(GetIntervalKey(aquariumId), intervalMinutes);

    public Task<bool> TrySetFlagAsync(string key, TimeSpan ttl, CancellationToken cancellationToken = default) =>
        _database.StringSetAsync(key, "1", ttl, when: When.NotExists);

    public Task RemoveKeyAsync(string key, CancellationToken cancellationToken = default) =>
        _database.KeyDeleteAsync(key);

    public Task SetLastSensorStateAsync(Guid aquariumId, AquariumData data, CancellationToken cancellationToken = default) =>
        _database.StringSetAsync(GetSensorStateKey(aquariumId), JsonSerializer.Serialize(data, JsonOptions));

    private static string GetLastFeedingKey(Guid aquariumId) => $"feeding:{aquariumId}:last";
    private static string GetIntervalKey(Guid aquariumId) => $"feeding:{aquariumId}:interval";
    private static string GetSensorStateKey(Guid aquariumId) => $"sensor:{aquariumId}:last";
}
