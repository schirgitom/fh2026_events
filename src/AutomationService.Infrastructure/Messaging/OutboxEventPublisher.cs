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
            AquariumId = ExtractAquariumId(@event),
            Type = typeof(T).FullName ?? typeof(T).Name,
            Payload = JsonSerializer.Serialize(@event),
            OccurredAtUtc = DateTimeOffset.UtcNow
        };

        outboxMessageRepository.Add(message);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static Guid? ExtractAquariumId<T>(T @event)
    {
        if (@event is null)
        {
            return null;
        }

        var property = @event.GetType().GetProperty("AquariumId");
        if (property is null)
        {
            return null;
        }

        var value = property.GetValue(@event);
        return value switch
        {
            Guid guid => guid,
            _ => null
        };
    }
}
