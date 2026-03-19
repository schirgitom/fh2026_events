using AutomationService.Domain.Models;

namespace AutomationService.Application.Abstractions;

public interface IAquariumDataClient
{
    Task<AquariumData> GetAquariumDataAsync(Guid aquariumId, CancellationToken cancellationToken = default);
}
