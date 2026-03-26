namespace AutomationService.Infrastructure.Persistence;

public interface IOutboxMessageRepository
{
    void Add(OutboxMessage message);
    Task<IReadOnlyList<OutboxMessage>> GetUnprocessedBatchAsync(int batchSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutboxMessage>> GetHistoryAsync(
        Guid aquariumId,
        int take,
        CancellationToken cancellationToken = default);
}
