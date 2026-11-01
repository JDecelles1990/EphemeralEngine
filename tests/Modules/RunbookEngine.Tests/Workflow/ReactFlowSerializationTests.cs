namespace RunbookForge.Modules.RunbookEngine.Tests.Workflow;

using System.Text.Json;
using FluentAssertions;
using RunbookForge.Modules.RunbookEngine.Domain.Workflow;
using Xunit;
using static RunbookForge.Modules.RunbookEngine.Tests.Workflow.WorkflowTestAst;

public class ReactFlowSerializationTests
{
    private static readonly JsonSerializerOptions WebOptions = new(JsonSerializerDefaults.Web);

    private sealed record FlowPosition(int X, int Y);

    private sealed record FlowNode(string Id, string Type, FlowPosition Position, Dictionary<string, JsonElement> Data);

    private sealed record FlowEdge(string Id, string Source, string Target, string? Label, bool Animated);

    private static WorkflowGraph CompileSample() =>
        WorkflowDagCompiler.Compile(Build(
            [
                Step("step-1", Question("Is the service healthy?", "Yes->step-2", "No")),
                Step("step-2")
            ],
            ["Escalate to DBA"])).Value;

    [Fact]
    public void ReactFlowJson_MatchesExpectedSchema()
    {
        var graph = CompileSample();

        var nodesJson = ReactFlowSerializer.SerializeNodes(graph.Nodes);
        var edgesJson = ReactFlowSerializer.SerializeEdges(graph.Edges);

        var nodes = JsonSerializer.Deserialize<List<FlowNode>>(nodesJson, WebOptions)!;
        var edges = JsonSerializer.Deserialize<List<FlowEdge>>(edgesJson, WebOptions)!;

        nodes.Should().HaveCount(graph.Nodes.Count);
        edges.Should().HaveCount(graph.Edges.Count);

        nodes.Select(n => n.Type).Distinct().Should().BeEquivalentTo(
            ["start", "step", "decision", "terminal", "escalation"]);
        nodes.Should().OnlyContain(n => !string.IsNullOrEmpty(n.Id) && n.Position != null && n.Data.ContainsKey("label"));

        var start = nodes.Single(n => n.Type == "start");
        start.Id.Should().Be(WorkflowDagCompiler.StartNodeId);
        start.Position.Should().Be(new FlowPosition(250, 0));
        start.Data["label"].GetString().Should().Be("Test Runbook");

        var step = nodes.Single(n => n.Id == "step-1");
        step.Data["label"].GetString().Should().Be("Title step-1");
        step.Data["risk"].GetString().Should().Be("low");
        step.Data["requires_elevation"].ValueKind.Should().Be(JsonValueKind.False);
        step.Data["scripts"].EnumerateArray().Select(e => e.GetString()).Should().Equal("echo step-1");
        step.Data["commands"][0].GetProperty("language").GetString().Should().Be("bash");
        step.Data["assertions"][0].GetProperty("expected_value").GetString().Should().Be("step-1");

        var decision = nodes.Single(n => n.Type == "decision");
        decision.Data["prompt"].GetString().Should().Be("Is the service healthy?");
        decision.Data["options"].EnumerateArray().Select(e => e.GetString()).Should().Equal("Yes", "No");

        edges.Should().OnlyContain(e => !string.IsNullOrEmpty(e.Id) && !string.IsNullOrEmpty(e.Source) && !string.IsNullOrEmpty(e.Target));
        edges.Should().ContainSingle(e => e.Label == "Yes" && e.Target == "step-2" && !e.Animated);
        edges.Should().ContainSingle(e => e.Label == "Escalate" && e.Animated);

        var nodeIds = nodes.Select(n => n.Id).ToHashSet();
        edges.Should().OnlyContain(e => nodeIds.Contains(e.Source) && nodeIds.Contains(e.Target));
    }

    [Fact]
    public void ReactFlowJson_UsesLowerCamelCaseContract_AndOmitsNullEdgeLabels()
    {
        var graph = CompileSample();

        using var nodes = JsonDocument.Parse(ReactFlowSerializer.SerializeNodes(graph.Nodes));
        using var edges = JsonDocument.Parse(ReactFlowSerializer.SerializeEdges(graph.Edges));

        nodes.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        var firstNode = nodes.RootElement[0];
        firstNode.EnumerateObject().Select(p => p.Name).Should().Equal("id", "type", "position", "data");
        firstNode.GetProperty("position").EnumerateObject().Select(p => p.Name).Should().Equal("x", "y");
        firstNode.GetProperty("position").GetProperty("x").ValueKind.Should().Be(JsonValueKind.Number);

        edges.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        var unlabeled = edges.RootElement.EnumerateArray().First(e => e.GetProperty("source").GetString() == WorkflowDagCompiler.StartNodeId);
        unlabeled.TryGetProperty("label", out _).Should().BeFalse();
        unlabeled.GetProperty("animated").ValueKind.Should().Be(JsonValueKind.False);
    }

    [Fact]
    public void Serialize_EmptyCollections_YieldEmptyJsonArrays()
    {
        ReactFlowSerializer.SerializeNodes([]).Should().Be("[]");
        ReactFlowSerializer.SerializeEdges([]).Should().Be("[]");
    }
}
