namespace AutomationService.Application.Abstractions;

public interface IPollingOrchestrator
{
    Task PollAllAsync(CancellationToken cancellationToken = default);
}
