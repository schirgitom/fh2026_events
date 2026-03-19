using AutomationService.Application.Abstractions;
using AutomationService.Infrastructure.Configuration;
using AutomationService.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

namespace AutomationService.Infrastructure.Messaging;

public sealed class OutboxProcessor(
    IOutboxMessageRepository outboxMessageRepository,
    IUnitOfWork unitOfWork,
    IOutboxMessageDispatcher dispatcher,
    IOptions<OutboxOptions> options) : IOutboxProcessor
{
    public async Task<int> ProcessAsync(CancellationToken cancellationToken = default)
    {
        var messages = await outboxMessageRepository.GetUnprocessedBatchAsync(
            options.Value.BatchSize,
            cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await dispatcher.DispatchAsync(message.Type, message.Payload, cancellationToken);
                message.ProcessedAtUtc = DateTimeOffset.UtcNow;
                message.Error = null;
            }
            catch (Exception ex)
            {
                message.Error = ex.Message;
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return messages.Count;
    }
}
