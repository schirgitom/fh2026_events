namespace AutomationService.Infrastructure.Persistence;

public sealed class EfUnitOfWork(AutomationDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
