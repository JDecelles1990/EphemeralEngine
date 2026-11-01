namespace RunbookForge.Modules.TicketIngestion.Infrastructure.Persistence.Entities;

/// <summary>Row of legacy <c>f7_incidents</c> (the F7Hub "ticket"). Read-only projection source.</summary>
internal sealed class F7LegacyIncident
{
    public string IncidentId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ResolutionSummary { get; init; }
    public string? Severity { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
}

/// <summary>Row of legacy <c>f7_logs</c> (technical work notes attached to an incident).</summary>
internal sealed class F7LegacyLog
{
    public long LogId { get; init; }
    public string IncidentId { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

/// <summary>Row of legacy <c>f7_kb_articles</c> (resolution archives).</summary>
internal sealed class F7LegacyArticle
{
    public string ArticleId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? BodyMarkdown { get; init; }
    public string? Author { get; init; }
}

