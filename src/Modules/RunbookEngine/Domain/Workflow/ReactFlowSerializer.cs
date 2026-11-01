namespace RunbookForge.Modules.RunbookEngine.Domain.Workflow;

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Serialises a <see cref="WorkflowGraph"/> into the standard React Flow JSON contract:
/// nodes as <c>{ id, type, position: { x, y }, data: { label, ... } }</c> and
/// edges as <c>{ id, source, target, label?, animated }</c>.
/// </summary>
public static class ReactFlowSerializer
{
    // Relaxed escaping keeps ids such as "step-1->step-2" readable in jsonb; the payload is never embedded in HTML.
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

    public static string SerializeNodes(IReadOnlyList<WorkflowNode> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);

        var payload = nodes.Select(node =>
        {
            var data = new Dictionary<string, object?>(StringComparer.Ordinal) { ["label"] = node.Label };
            foreach (var (key, value) in node.Data)
            {
                data[key] = value;
            }

            return new ReactFlowNode(node.Id, ToReactFlowType(node.Type), new ReactFlowPosition(node.Position.X, node.Position.Y), data);
        });

        return JsonSerializer.Serialize(payload, Options);
    }

    public static string SerializeEdges(IReadOnlyList<WorkflowEdge> edges)
    {
        ArgumentNullException.ThrowIfNull(edges);

        var payload = edges.Select(edge => new ReactFlowEdge(edge.Id, edge.Source, edge.Target, edge.Label, edge.Animated));
        return JsonSerializer.Serialize(payload, Options);
    }

    public static string ToReactFlowType(WorkflowNodeType type) => type switch
    {
        WorkflowNodeType.Start => "start",
        WorkflowNodeType.Step => "step",
        WorkflowNodeType.Decision => "decision",
        WorkflowNodeType.Escalation => "escalation",
        WorkflowNodeType.Terminal => "terminal",
        _ => "step"
    };

    private sealed record ReactFlowPosition(int X, int Y);

    private sealed record ReactFlowNode(
        string Id,
        string Type,
        ReactFlowPosition Position,
        IReadOnlyDictionary<string, object?> Data);

    private sealed record ReactFlowEdge(string Id, string Source, string Target, string? Label, bool Animated);
}
