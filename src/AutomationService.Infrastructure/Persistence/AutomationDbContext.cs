using Microsoft.EntityFrameworkCore;

namespace AutomationService.Infrastructure.Persistence;

public sealed class AutomationDbContext(DbContextOptions<AutomationDbContext> options) : DbContext(options)
{
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AquariumId);
            entity.Property(x => x.Type).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Payload).IsRequired();
            entity.Property(x => x.OccurredAtUtc).IsRequired();
            entity.HasIndex(x => x.ProcessedAtUtc);
            entity.HasIndex(x => new { x.AquariumId, x.OccurredAtUtc });
        });
    }
}
