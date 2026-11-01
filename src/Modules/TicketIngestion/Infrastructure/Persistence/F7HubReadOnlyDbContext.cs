namespace RunbookForge.Modules.TicketIngestion.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using RunbookForge.Modules.TicketIngestion.Infrastructure.Persistence.Entities;

/// <summary>
/// Read-only gateway to the legacy F7Hub catalog (<c>f7hub.db</c>). Invariant INV-001:
/// <list type="number">
/// <item>Connection is <c>Mode=ReadOnly;Cache=Shared</c> — verified at construction, fail closed.</item>
/// <item>Tracking is <see cref="QueryTrackingBehavior.NoTracking"/>, auto change detection is off.</item>
/// <item>Every <c>SaveChanges*</c> overload throws <see cref="InvalidOperationException"/>.</item>
/// <item>Raw SQL mutations are blocked by <c>F7HubReadOnlyCommandInterceptor</c>.</item>
/// </list>
/// </summary>
internal sealed class F7HubReadOnlyDbContext : DbContext
{
    public const string ReadOnlyMessage = "F7Hub database is strictly read-only. Mutation operations are prohibited.";

    public F7HubReadOnlyDbContext(DbContextOptions<F7HubReadOnlyDbContext> options)
        : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        ChangeTracker.AutoDetectChangesEnabled = false;

        if (!F7HubConnectionPolicy.IsEnforced(Database.GetConnectionString()))
        {
            throw new InvalidOperationException(
                "F7Hub must be opened with 'Mode=ReadOnly;Cache=Shared'. Register it through AddF7HubReadOnlyPersistence.");
        }
    }

    public DbSet<F7LegacyIncident> Incidents => Set<F7LegacyIncident>();
    public DbSet<F7LegacyLog> Logs => Set<F7LegacyLog>();
    public DbSet<F7LegacyArticle> Articles => Set<F7LegacyArticle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<F7LegacyIncident>(b =>
        {
            b.ToTable("f7_incidents");
            b.HasKey(x => x.IncidentId);
            b.Property(x => x.IncidentId).HasColumnName("incident_id");
            b.Property(x => x.Title).HasColumnName("title");
            b.Property(x => x.Description).HasColumnName("description");
            b.Property(x => x.ResolutionSummary).HasColumnName("resolution_summary");
            b.Property(x => x.Severity).HasColumnName("severity");
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
            b.Property(x => x.ClosedAt).HasColumnName("closed_at");
        });

        modelBuilder.Entity<F7LegacyLog>(b =>
        {
            b.ToTable("f7_logs");
            b.HasKey(x => x.LogId);
            b.Property(x => x.LogId).HasColumnName("log_id");
            b.Property(x => x.IncidentId).HasColumnName("incident_id");
            b.Property(x => x.Message).HasColumnName("message");
            b.Property(x => x.Timestamp).HasColumnName("timestamp");
        });

        modelBuilder.Entity<F7LegacyArticle>(b =>
        {
            b.ToTable("f7_kb_articles");
            b.HasKey(x => x.ArticleId);
            b.Property(x => x.ArticleId).HasColumnName("article_id");
            b.Property(x => x.Title).HasColumnName("title");
            b.Property(x => x.BodyMarkdown).HasColumnName("body_markdown");
            b.Property(x => x.Author).HasColumnName("author");
        });
    }

    // All four overloads are blocked: the parameterless ones delegate to the bool overloads in the base class,
    // so overriding only those would leave SaveChanges(true) / SaveChangesAsync(true) as a bypass.
    public override int SaveChanges() => throw new InvalidOperationException(ReadOnlyMessage);

    public override int SaveChanges(bool acceptAllChangesOnSuccess) =>
        throw new InvalidOperationException(ReadOnlyMessage);

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(ReadOnlyMessage);

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(ReadOnlyMessage);
}

