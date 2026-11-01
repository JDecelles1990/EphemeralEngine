namespace RunbookForge.Modules.RunbookEngine.Tests.Workflow;

using Microsoft.EntityFrameworkCore;
using RunbookForge.Modules.RunbookEngine.Application.Contracts;
using RunbookForge.Modules.RunbookEngine.Domain.Entities;

/// <summary>
/// Minimal relational model used only to exercise <see cref="IRunbookDbContext"/> consumers against SQLite.
/// The production Npgsql model (schemas, xmin, jsonb/GIN) is covered by the EF migration, not by unit tests.
/// </summary>
internal sealed class TestRunbookDbContext(DbContextOptions<TestRunbookDbContext> options)
    : DbContext(options), IRunbookDbContext
{
    public DbSet<RunbookDocument> Documents => Set<RunbookDocument>();
    public DbSet<RunbookVersion> Versions => Set<RunbookVersion>();
    public DbSet<RunbookLintResult> LintResults => Set<RunbookLintResult>();
    public DbSet<RunbookWorkflow> Workflows => Set<RunbookWorkflow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Ignore<RunbookVersion>();
        modelBuilder.Ignore<RunbookLintResult>();

        modelBuilder.Entity<RunbookDocument>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Ignore(x => x.Versions);
            builder.Ignore(x => x.LintResults);
            builder.Ignore(x => x.Tags);
            builder.Ignore(x => x.VersionToken);
            builder.HasOne(x => x.Workflow)
                .WithOne(x => x.Document)
                .HasForeignKey<RunbookWorkflow>(x => x.DocumentId);
        });

        modelBuilder.Entity<RunbookWorkflow>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
        });
    }
}

