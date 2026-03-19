using Microsoft.EntityFrameworkCore;

namespace AutomationService.Infrastructure.Persistence;

public sealed class EfOutboxMessageRepository(AutomationDbContext dbContext) : IOutboxMessageRepository
{
    public void Add(OutboxMessage message) => dbContext.OutboxMessages.Add(message);

    public async Task<IReadOnlyList<OutboxMessage>> GetUnprocessedBatchAsync(
        int batchSize,
        CancellationToken cancellationToken = default) =>
        await dbContext.OutboxMessages
            .Where(x => x.ProcessedAtUtc == null)
            .OrderBy(x => x.OccurredAtUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
}
