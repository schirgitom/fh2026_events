using AutomationService.Application.Dtos;

namespace AutomationService.Application.Abstractions;

public interface IFeedingService
{
    Task<FeedingStatusDto> FeedNowAsync(Guid aquariumId, CancellationToken cancellationToken = default);
    Task<FeedingStatusDto> SetIntervalAsync(Guid aquariumId, int intervalMinutes, CancellationToken cancellationToken = default);
    Task<FeedingStatusDto> EvaluateAsync(Guid aquariumId, DateTimeOffset now, CancellationToken cancellationToken = default);
}
