using AutomationService.Domain.Models;

namespace AutomationService.Application.Abstractions;

public interface IEventDetectionService
{
    Task DetectAndPublishAsync(AquariumData data, CancellationToken cancellationToken = default);
}
