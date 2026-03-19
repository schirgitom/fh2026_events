namespace AutomationService.Application.Abstractions;

public interface IOutboxProcessor
{
    Task<int> ProcessAsync(CancellationToken cancellationToken = default);
}
