namespace RunbookForge.Modules.RunbookEngine.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using RunbookForge.BuildingBlocks.Persistence.Constants;
using RunbookForge.Modules.RunbookEngine.Application.Contracts;
using RunbookForge.Modules.RunbookEngine.Domain.Entities;

public sealed class RunbookDbContext(DbContextOptions<RunbookDbContext> options)
    : DbContext(options), IRunbookDbContext
{
    public DbSet<RunbookDocument> Documents => Set<RunbookDocument>();
    public DbSet<RunbookVersion> Versions => Set<RunbookVersion>();
    public DbSet<RunbookLintResult> LintResults => Set<RunbookLintResult>();
    public DbSet<RunbookWorkflow> Workflows => Set<RunbookWorkflow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaConstants.Runbooks);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RunbookDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}

