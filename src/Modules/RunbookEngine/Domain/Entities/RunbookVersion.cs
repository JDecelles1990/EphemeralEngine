namespace RunbookForge.Modules.RunbookEngine.Domain.Entities;

using RunbookForge.BuildingBlocks.Common.Results;

public sealed class RunbookVersion
{
    private RunbookVersion()
    {
    }

    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    public int VersionNumber { get; private set; }
    public string RawMarkdown { get; private set; } = string.Empty;
    public string ParsedAst { get; private set; } = "{}";
    public string ChangeSummary { get; private set; } = string.Empty;
    public string PublishedByUserId { get; private set; } = string.Empty;
    public DateTimeOffset PublishedAt { get; private set; } = DateTimeOffset.UtcNow;

    public RunbookDocument? Document { get; private set; }

    public static Result<RunbookVersion> Create(
        Guid documentId,
        int versionNumber,
        string rawMarkdown,
        string parsedAst,
        string changeSummary,
        string publishedByUserId)
    {
        if (documentId == Guid.Empty)
            return Result<RunbookVersion>.Failure(Error.Validation("Version.InvalidDocumentId", "DocumentId cannot be empty."));

        if (versionNumber <= 0)
            return Result<RunbookVersion>.Failure(Error.Validation("Version.InvalidNumber", "Version number must be positive."));

        var version = new RunbookVersion
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            VersionNumber = versionNumber,
            RawMarkdown = rawMarkdown ?? string.Empty,
            ParsedAst = string.IsNullOrWhiteSpace(parsedAst) ? "{}" : parsedAst,
            ChangeSummary = changeSummary?.Trim() ?? string.Empty,
            PublishedByUserId = string.IsNullOrWhiteSpace(publishedByUserId) ? "system" : publishedByUserId.Trim(),
            PublishedAt = DateTimeOffset.UtcNow
        };

        return Result<RunbookVersion>.Success(version);
    }
}

