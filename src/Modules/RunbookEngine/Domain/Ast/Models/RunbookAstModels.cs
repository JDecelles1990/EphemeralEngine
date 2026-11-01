namespace RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;

public record RunbookMetadata(
    string Id,
    string Title,
    string Slug,
    string Category,
    string Level,
    string Risk,
    string Author,
    string Version,
    string SchemaVersion,
    IReadOnlyList<string> Tags,
    string? LastReviewed = null,
    string? Audience = null,
    bool HasFrontmatter = true);

public record CommandNode(
    string Language,
    string Script,
    bool RequiresElevation,
    string? Timeout = null,
    string? Mode = null);

public record AssertionNode(
    string Target,
    string Comparator,
    string ExpectedValue,
    string? Message = null);

public record QuestionNode(
    string Prompt,
    IReadOnlyList<string> Options);

public record StepNode(
    string Id,
    string Title,
    string Risk,
    IReadOnlyList<CommandNode> Commands,
    IReadOnlyList<AssertionNode> Assertions,
    IReadOnlyList<QuestionNode> Questions,
    string? Description = null);

public record RunbookAstDocument(
    RunbookMetadata Metadata,
    IReadOnlyList<StepNode> Steps,
    IReadOnlyList<string> EscalationTriggers,
    string? RawMarkdown = null,
    IReadOnlyList<string>? Symptoms = null,
    IReadOnlyList<string>? RawSectionsFound = null)
{
    public IReadOnlyList<string> Symptoms { get; init; } = Symptoms ?? [];
    public IReadOnlyList<string> RawSectionsFound { get; init; } = RawSectionsFound ?? [];
}
