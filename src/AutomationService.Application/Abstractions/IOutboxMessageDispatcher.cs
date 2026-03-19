namespace AutomationService.Application.Abstractions;

public interface IOutboxMessageDispatcher
{
    Task DispatchAsync(string eventType, string payload, CancellationToken cancellationToken = default);
}
