namespace RunbookForge.Modules.RunbookEngine.Domain.Entities;

using RunbookForge.BuildingBlocks.Common.Results;

public sealed class RunbookWorkflow
{
    private RunbookWorkflow()
    {
    }

    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    public int VersionNumber { get; private set; }
    public string NodesJson { get; private set; } = "[]";
    public string EdgesJson { get; private set; } = "[]";
    public DateTimeOffset CompiledAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public RunbookDocument? Document { get; private set; }

    public static Result<RunbookWorkflow> Create(
        Guid documentId,
        int versionNumber,
        string nodesJson = "[]",
        string edgesJson = "[]")
    {
        if (documentId == Guid.Empty)
            return Result<RunbookWorkflow>.Failure(Error.Validation("Workflow.InvalidDocumentId", "DocumentId cannot be empty."));

        var workflow = new RunbookWorkflow
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            VersionNumber = versionNumber,
            NodesJson = string.IsNullOrWhiteSpace(nodesJson) ? "[]" : nodesJson,
            EdgesJson = string.IsNullOrWhiteSpace(edgesJson) ? "[]" : edgesJson,
            CompiledAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        return Result<RunbookWorkflow>.Success(workflow);
    }

    public void UpdateTopology(string nodesJson, string edgesJson)
    {
        NodesJson = string.IsNullOrWhiteSpace(nodesJson) ? "[]" : nodesJson;
        EdgesJson = string.IsNullOrWhiteSpace(edgesJson) ? "[]" : edgesJson;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Recompile(int versionNumber, string nodesJson, string edgesJson)
    {
        VersionNumber = versionNumber;
        UpdateTopology(nodesJson, edgesJson);
        CompiledAt = UpdatedAt;
    }
}

