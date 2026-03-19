using System.Text.Json;
using AutomationService.Application.Abstractions;
using AutomationService.Infrastructure.Persistence;

namespace AutomationService.Infrastructure.Messaging;

public sealed class OutboxEventPublisher(
    IOutboxMessageRepository outboxMessageRepository,
    IUnitOfWork unitOfWork) : IEventPublisher
{
    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
    {
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(T).FullName ?? typeof(T).Name,
            Payload = JsonSerializer.Serialize(@event),
            OccurredAtUtc = DateTimeOffset.UtcNow
        };

        outboxMessageRepository.Add(message);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
