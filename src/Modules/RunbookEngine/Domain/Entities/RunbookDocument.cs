namespace RunbookForge.Modules.RunbookEngine.Domain.Entities;

using RunbookForge.BuildingBlocks.Common.Results;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;

public sealed class RunbookDocument
{
    private readonly List<RunbookVersion> _versions = [];
    private readonly List<RunbookLintResult> _lintResults = [];

    private RunbookDocument()
    {
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string RawMarkdown { get; private set; } = string.Empty;
    public string AstJson { get; private set; } = "{}";
    public string Category { get; private set; } = "General";
    public RunbookStatus Status { get; private set; } = RunbookStatus.Draft;
    public RunbookRiskLevel RiskLevel { get; private set; } = RunbookRiskLevel.Low;
    public string[] Tags { get; private set; } = [];
    public int CurrentVersionNumber { get; private set; } = 1;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public uint VersionToken { get; private set; }

    public IReadOnlyCollection<RunbookVersion> Versions => _versions.AsReadOnly();
    public IReadOnlyCollection<RunbookLintResult> LintResults => _lintResults.AsReadOnly();
    public RunbookWorkflow? Workflow { get; private set; }

    public static Result<RunbookDocument> Create(
        string title,
        string slug,
        string description,
        string rawMarkdown,
        string astJson = "{}",
        string category = "General",
        RunbookRiskLevel riskLevel = RunbookRiskLevel.Low,
        string[]? tags = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result<RunbookDocument>.Failure(Error.Validation("Runbook.TitleEmpty", "Title cannot be empty."));

        if (string.IsNullOrWhiteSpace(slug))
            return Result<RunbookDocument>.Failure(Error.Validation("Runbook.SlugEmpty", "Slug cannot be empty."));

        var document = new RunbookDocument
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            Description = description?.Trim() ?? string.Empty,
            RawMarkdown = rawMarkdown ?? string.Empty,
            AstJson = string.IsNullOrWhiteSpace(astJson) ? "{}" : astJson,
            Category = string.IsNullOrWhiteSpace(category) ? "General" : category.Trim(),
            Status = RunbookStatus.Draft,
            RiskLevel = riskLevel,
            Tags = tags ?? [],
            CurrentVersionNumber = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        return Result<RunbookDocument>.Success(document);
    }

    public void UpdateContent(string rawMarkdown, string astJson, string? description = null)
    {
        RawMarkdown = rawMarkdown ?? string.Empty;
        AstJson = string.IsNullOrWhiteSpace(astJson) ? "{}" : astJson;
        if (description is not null) Description = description.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetStatus(RunbookStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void IncrementVersion()
    {
        CurrentVersionNumber++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AddVersion(RunbookVersion version)
    {
        ArgumentNullException.ThrowIfNull(version);
        _versions.Add(version);
    }

    public void AddLintResult(RunbookLintResult lintResult)
    {
        ArgumentNullException.ThrowIfNull(lintResult);
        _lintResults.Add(lintResult);
    }

    public void SetWorkflow(RunbookWorkflow workflow)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        Workflow = workflow;
    }
}

