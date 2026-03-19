namespace AutomationService.Application.Abstractions;

public interface IAquariumRegistryClient
{
    Task<IReadOnlyCollection<Guid>> GetAquariumIdsAsync(CancellationToken cancellationToken = default);
}
