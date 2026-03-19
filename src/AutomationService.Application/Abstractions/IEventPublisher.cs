namespace AutomationService.Application.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default);
}
