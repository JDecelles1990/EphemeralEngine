namespace RunbookForge.Modules.RunbookEngine.Tests.Workflow;

using FluentAssertions;
using RunbookForge.BuildingBlocks.Common.Results;
using RunbookForge.Modules.RunbookEngine.Domain.Workflow;
using Xunit;

public class WorkflowGraphAnalyzerTests
{
    private static WorkflowNode Node(string id) =>
        new(id, WorkflowNodeType.Step, id, new NodePosition(0, 0), new Dictionary<string, object?>());

    private static WorkflowEdge Edge(string source, string target) =>
        new($"{source}->{target}", source, target, null, false);

    private static List<WorkflowNode> Nodes(params string[] ids) => ids.Select(Node).ToList();

    [Fact]
    public void Validate_ReturnsSuccess_ForDiamondDag()
    {
        var nodes = Nodes("a", "b", "c", "d");
        var edges = new[] { Edge("a", "b"), Edge("a", "c"), Edge("b", "d"), Edge("c", "d") };

        WorkflowGraphAnalyzer.Validate(nodes, edges).IsSuccess.Should().BeTrue();
        WorkflowGraphAnalyzer.FindCycleNode(nodes, edges).Should().BeNull();
    }

    [Fact]
    public void Validate_ReportsCycleDetected_ForArtificialCircularEdge()
    {
        var nodes = Nodes("a", "b", "c");
        var edges = new[] { Edge("a", "b"), Edge("b", "c"), Edge("c", "a") };

        var result = WorkflowGraphAnalyzer.Validate(nodes, edges);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Workflow.CycleDetected");
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Message.Should().MatchRegex("^Cycle detected involving node [abc]$");
    }

    [Fact]
    public void FindCycleNode_ReportsNodeOnCycle_NotMerelyDownstreamOfIt()
    {
        // a -> b -> c -> b ; d hangs off the cycle but is not part of it.
        var nodes = Nodes("a", "b", "c", "d");
        var edges = new[] { Edge("a", "b"), Edge("b", "c"), Edge("c", "b"), Edge("c", "d") };

        WorkflowGraphAnalyzer.FindCycleNode(nodes, edges).Should().BeOneOf("b", "c");
    }

    [Fact]
    public void FindCycleNode_DetectsSelfLoop()
    {
        WorkflowGraphAnalyzer.FindCycleNode(Nodes("a"), [Edge("a", "a")]).Should().Be("a");
    }

    [Fact]
    public void Validate_RejectsEdgesWithUnknownEndpoints_WithoutThrowing()
    {
        var result = WorkflowGraphAnalyzer.Validate(Nodes("a"), [Edge("a", "ghost")]);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(WorkflowGraphAnalyzer.InvalidEdgeCode);
    }

    [Fact]
    public void Validate_RejectsDuplicateNodeIds_WithoutThrowing()
    {
        var result = WorkflowGraphAnalyzer.Validate(Nodes("a", "a"), []);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(WorkflowGraphAnalyzer.DuplicateNodeCode);
    }

    [Fact]
    public void ComputeRanks_UsesLongestPathDepth()
    {
        var nodes = Nodes("a", "b", "c", "d");
        var edges = new[] { Edge("a", "b"), Edge("b", "c"), Edge("a", "c"), Edge("c", "d") };

        var ranks = WorkflowGraphAnalyzer.ComputeRanks(nodes, edges).Value;

        ranks["a"].Should().Be(0);
        ranks["b"].Should().Be(1);
        ranks["c"].Should().Be(2);
        ranks["d"].Should().Be(3);
    }

    [Fact]
    public void ComputeRanks_FailsWithCycleDetected_WhenGraphIsCyclic()
    {
        var result = WorkflowGraphAnalyzer.ComputeRanks(Nodes("a", "b"), [Edge("a", "b"), Edge("b", "a")]);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Workflow.CycleDetected");
    }

    [Fact]
    public void ComputeReachable_ExcludesNodesNotConnectedToRoot()
    {
        var nodes = Nodes("a", "b", "orphan");

        var reachable = WorkflowGraphAnalyzer.ComputeReachable("a", nodes, [Edge("a", "b")]);

        reachable.Should().BeEquivalentTo(["a", "b"]);
    }
}
