namespace RunbookForge.Modules.RunbookEngine.Domain.Workflow;

public enum WorkflowNodeType
{
    Start = 0,
    Step = 1,
    Decision = 2,
    Escalation = 3,
    Terminal = 4
}

public sealed record NodePosition(int X, int Y);

/// <summary>
/// A vertex of the executable runbook DAG. <see cref="Data"/> carries node-specific payload
/// (scripts, requires_elevation, risk, prompt, options, ...) using snake_case keys.
/// </summary>
public sealed record WorkflowNode(
    string Id,
    WorkflowNodeType Type,
    string Label,
    NodePosition Position,
    IReadOnlyDictionary<string, object?> Data);

public sealed record WorkflowEdge(
    string Id,
    string Source,
    string Target,
    string? Label,
    bool Animated);

public sealed record WorkflowGraph(
    IReadOnlyList<WorkflowNode> Nodes,
    IReadOnlyList<WorkflowEdge> Edges,
    bool IsAcyclic);

