using Microsoft.EntityFrameworkCore;
using AutomationService.Domain.Events;

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

    public async Task<IReadOnlyList<OutboxMessage>> GetHistoryAsync(
        Guid aquariumId,
        int take,
        CancellationToken cancellationToken = default)
    {
        var alertType = typeof(RuleTriggeredIntegrationEvent).FullName ?? typeof(RuleTriggeredIntegrationEvent).Name;
        var query = dbContext.OutboxMessages.Where(x => x.Type == alertType && x.AquariumId == aquariumId);

        return await query
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
