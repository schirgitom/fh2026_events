using AutomationService.Application.Abstractions;
using AutomationService.Application.Configuration;
using AutomationService.Application.Dtos;
using AutomationService.Domain.Events;
using Microsoft.Extensions.Options;

namespace AutomationService.Application.Services;

public sealed class FeedingService(
    IStateStore stateStore,
    IEventPublisher eventPublisher,
    IClock clock,
    IOptions<FeedingOptions> feedingOptions) : IFeedingService
{
    private const int FallbackDefaultIntervalMinutes = 480;
    private readonly int defaultIntervalMinutes = feedingOptions.Value.DefaultIntervalMinutes > 0
        ? feedingOptions.Value.DefaultIntervalMinutes
        : FallbackDefaultIntervalMinutes;

    public async Task<FeedingStatusDto> FeedNowAsync(Guid aquariumId, CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;
        await stateStore.SetLastFeedingAsync(aquariumId, now, cancellationToken);
        await stateStore.RemoveKeyAsync(GetOverdueFlagKey(aquariumId), cancellationToken);

        var status = await BuildStatusAsync(aquariumId, now, cancellationToken);
        await PublishStatusAsync(status, null, cancellationToken);
        return status;
    }

    public async Task<FeedingStatusDto> SetIntervalAsync(Guid aquariumId, int intervalMinutes, CancellationToken cancellationToken = default)
    {
        if (intervalMinutes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(intervalMinutes), "Interval must be greater than zero.");
        }

        await stateStore.SetFeedingIntervalMinutesAsync(aquariumId, intervalMinutes, cancellationToken);
        var status = await BuildStatusAsync(aquariumId, clock.UtcNow, cancellationToken);
        await PublishStatusAsync(status, null, cancellationToken);
        return status;
    }

    public async Task<FeedingStatusDto> EvaluateAsync(Guid aquariumId, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        var status = await BuildStatusAsync(aquariumId, now, cancellationToken);

        if (!status.IsOverdue)
        {
            await stateStore.RemoveKeyAsync(GetOverdueFlagKey(aquariumId), cancellationToken);
            await PublishStatusAsync(status, null, cancellationToken);
            return status;
        }

        var sent = await stateStore.TrySetFlagAsync(GetOverdueFlagKey(aquariumId), TimeSpan.FromDays(1), cancellationToken);
        if (!sent)
        {
            return status;
        }

        await PublishStatusAsync(status, "Feeding overdue!", cancellationToken);
        return status;
    }

    private async Task<FeedingStatusDto> BuildStatusAsync(Guid aquariumId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var intervalMinutes = await stateStore.GetFeedingIntervalMinutesAsync(aquariumId, cancellationToken) ?? defaultIntervalMinutes;
        var lastFeeding = await stateStore.GetLastFeedingAsync(aquariumId, cancellationToken);
        if (lastFeeding is null)
        {
            lastFeeding = now;
            await stateStore.SetLastFeedingAsync(aquariumId, lastFeeding.Value, cancellationToken);
        }

        var nextFeeding = lastFeeding.Value.AddMinutes(intervalMinutes);
        var remainingSeconds = (long)Math.Floor((nextFeeding - now).TotalSeconds);

        return new FeedingStatusDto
        {
            AquariumId = aquariumId,
            NextFeedingAt = nextFeeding,
            RemainingSeconds = remainingSeconds,
            IsOverdue = now > nextFeeding
        };
    }

    private async Task PublishStatusAsync(FeedingStatusDto status, string? message, CancellationToken cancellationToken)
    {
        var @event = new FeedingStatusUpdatedIntegrationEvent(
            status.AquariumId,
            status.NextFeedingAt,
            status.RemainingSeconds,
            status.IsOverdue,
            message);

        await eventPublisher.PublishAsync(@event, cancellationToken);
    }

    private static string GetOverdueFlagKey(Guid aquariumId) => $"feeding:{aquariumId}:overdue:sent";
}
