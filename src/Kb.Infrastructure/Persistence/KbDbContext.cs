using Kb.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Kb.Infrastructure.Persistence;

public sealed class KbDbContext(DbContextOptions<KbDbContext> options) : DbContext(options)
{
  public DbSet<Document> Documents => Set<Document>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(KbDbContext).Assembly);
  }
}
