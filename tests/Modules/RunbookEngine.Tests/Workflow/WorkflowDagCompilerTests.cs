namespace RunbookForge.Modules.RunbookEngine.Tests.Workflow;

using FluentAssertions;
using RunbookForge.BuildingBlocks.Common.Results;
using RunbookForge.Modules.RunbookEngine.Domain.Workflow;
using Xunit;
using static RunbookForge.Modules.RunbookEngine.Tests.Workflow.WorkflowTestAst;

public class WorkflowDagCompilerTests
{
    [Fact]
    public void LinearRunbook_CompilesToLinearDAG_WithSequentialEdges()
    {
        var result = WorkflowDagCompiler.Compile(Build(LinearSteps(3)));

        result.IsSuccess.Should().BeTrue(result.IsFailure ? result.Error.Message : string.Empty);
        var graph = result.Value;

        graph.IsAcyclic.Should().BeTrue();
        graph.Nodes.Count(n => n.Type == WorkflowNodeType.Start).Should().Be(1);
        graph.Nodes.Count(n => n.Type == WorkflowNodeType.Step).Should().Be(3);
        graph.Nodes.Count(n => n.Type == WorkflowNodeType.Terminal).Should().Be(1);
        graph.Nodes.Should().HaveCount(5);

        graph.Edges.Select(e => (e.Source, e.Target)).Should().Equal(
            (WorkflowDagCompiler.StartNodeId, "step-1"),
            ("step-1", "step-2"),
            ("step-2", "step-3"),
            ("step-3", WorkflowDagCompiler.TerminalNodeId));
        graph.Edges.Should().OnlyContain(e => e.Label == null && !e.Animated);
        graph.Edges.Select(e => e.Id).Should().OnlyHaveUniqueItems();

        graph.Nodes.Select(n => n.Position).Should().Equal(
            new NodePosition(250, 0),
            new NodePosition(250, 150),
            new NodePosition(250, 300),
            new NodePosition(250, 450),
            new NodePosition(250, 600));
    }

    [Fact]
    public void QuestionBlock_GeneratesDecisionNode_WithMultipleBranchingEdges()
    {
        var steps = new[]
        {
            Step("step-1", Question("Is the service healthy?", "Yes->step-3", "No")),
            Step("step-2"),
            Step("step-3")
        };

        var result = WorkflowDagCompiler.Compile(Build(steps));

        result.IsSuccess.Should().BeTrue(result.IsFailure ? result.Error.Message : string.Empty);
        var graph = result.Value;

        var decision = graph.Nodes.Should().ContainSingle(n => n.Type == WorkflowNodeType.Decision).Subject;
        decision.Id.Should().Be("step-1__decision_1");
        decision.Label.Should().Be("Is the service healthy?");
        decision.Data["prompt"].Should().Be("Is the service healthy?");
        ((IEnumerable<string>)decision.Data["options"]!).Should().Equal("Yes", "No");

        graph.Edges.Should().ContainSingle(e => e.Source == "step-1" && e.Target == decision.Id);

        var branches = graph.Edges.Where(e => e.Source == decision.Id).ToList();
        branches.Should().HaveCount(2);
        branches.Should().ContainSingle(e => e.Label == "Yes" && e.Target == "step-3");
        branches.Should().ContainSingle(e => e.Label == "No" && e.Target == "step-2");
    }

    [Fact]
    public void QuestionBlock_WithPlainOptions_FallsThroughToNextStep_WithDistinctEdgeIds()
    {
        var steps = new[]
        {
            Step("step-1", Question("Ready?", "Yes", "No")),
            Step("step-2")
        };

        var graph = WorkflowDagCompiler.Compile(Build(steps)).Value;

        var branches = graph.Edges.Where(e => e.Source == "step-1__decision_1").ToList();
        branches.Select(e => e.Target).Should().OnlyContain(t => t == "step-2");
        branches.Select(e => e.Label).Should().Equal("Yes", "No");
        graph.Edges.Select(e => e.Id).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void MultipleQuestions_AreChainedAsSequentialDecisions()
    {
        var steps = new[]
        {
            Step("step-1", Question("First?", "A", "B"), Question("Second?", "C", "D")),
            Step("step-2")
        };

        var graph = WorkflowDagCompiler.Compile(Build(steps)).Value;

        graph.Nodes.Where(n => n.Type == WorkflowNodeType.Decision).Select(n => n.Id)
            .Should().Equal("step-1__decision_1", "step-1__decision_2");
        graph.Edges.Where(e => e.Source == "step-1__decision_1").Select(e => e.Target)
            .Should().OnlyContain(t => t == "step-1__decision_2");
        graph.Edges.Where(e => e.Source == "step-1__decision_2").Select(e => e.Target)
            .Should().OnlyContain(t => t == "step-2");
    }

    [Fact]
    public void CyclicWorkflow_FailsCompilation_WithCycleDetectedError()
    {
        var steps = new[]
        {
            Step("step-1"),
            Step("step-2", Question("Fixed?", "Yes", "Retry->step-1"))
        };

        var result = WorkflowDagCompiler.Compile(Build(steps));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Workflow.CycleDetected");
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Message.Should().StartWith("Cycle detected involving node ");
    }

    [Fact]
    public void SelfReferencingBranch_FailsCompilation_WithCycleDetectedError()
    {
        var steps = new[] { Step("step-1", Question("Again?", "Loop->step-1", "Done")) };

        var result = WorkflowDagCompiler.Compile(Build(steps));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Workflow.CycleDetected");
    }

    [Fact]
    public void DisconnectedSteps_AreFlaggedOrRoutedToTerminal()
    {
        // Every branch of step-1 jumps over step-2, so nothing can ever reach it.
        var steps = new[]
        {
            Step("step-1", Question("Skip?", "A->step-3", "B->step-3")),
            Step("step-2"),
            Step("step-3")
        };

        var result = WorkflowDagCompiler.Compile(Build(steps));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(WorkflowDagCompiler.UnreachableStepCode);
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Message.Should().Contain("step-2").And.NotContain("step-3");
    }

    [Fact]
    public void BranchToUnknownStep_FailsWithInvalidBranchTarget()
    {
        var steps = new[] { Step("step-1", Question("Next?", "Go->step-99", "Stay")) };

        var result = WorkflowDagCompiler.Compile(Build(steps));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(WorkflowDagCompiler.InvalidBranchTargetCode);
        result.Error.Message.Should().Contain("step-99");
    }

    [Theory]
    [InlineData("->step-1")]
    [InlineData("   ")]
    public void BranchOptionWithEmptyLabel_FailsWithInvalidBranchOption(string option)
    {
        var steps = new[] { Step("step-1", Question("Next?", option)) };

        var result = WorkflowDagCompiler.Compile(Build(steps));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(WorkflowDagCompiler.InvalidBranchOptionCode);
    }

    [Fact]
    public void QuestionWithoutOptions_FailsValidation()
    {
        var steps = new[] { Step("step-1", Question("Anything?")) };

        var result = WorkflowDagCompiler.Compile(Build(steps));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(WorkflowDagCompiler.QuestionWithoutOptionsCode);
    }

    [Fact]
    public void DuplicateStepIds_FailValidation()
    {
        var result = WorkflowDagCompiler.Compile(Build([Step("step-1"), Step("step-1")]));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(WorkflowDagCompiler.DuplicateStepCode);
    }

    [Fact]
    public void EmptyRunbook_FailsValidation()
    {
        var result = WorkflowDagCompiler.Compile(Build([]));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(WorkflowDagCompiler.NoStepsCode);
    }

    [Fact]
    public void StepsWithoutIds_ReceiveGeneratedSequentialIds()
    {
        var steps = new[] { Step(string.Empty), Step(" ") };

        var graph = WorkflowDagCompiler.Compile(Build(steps)).Value;

        graph.Nodes.Where(n => n.Type == WorkflowNodeType.Step).Select(n => n.Id)
            .Should().Equal("step-1", "step-2");
    }

    [Fact]
    public void EscalationTriggers_BecomeAnimatedEscalationSinksAttachedToLastStep()
    {
        var graph = WorkflowDagCompiler.Compile(
            Build(LinearSteps(2), ["MTTR exceeds 15 minutes", "Data corruption suspected", "  "])).Value;

        var escalations = graph.Nodes.Where(n => n.Type == WorkflowNodeType.Escalation).ToList();
        escalations.Select(n => n.Label).Should().Equal("MTTR exceeds 15 minutes", "Data corruption suspected");

        var escalationEdges = graph.Edges.Where(e => e.Target.StartsWith("__escalation_", StringComparison.Ordinal)).ToList();
        escalationEdges.Should().HaveCount(2);
        escalationEdges.Should().OnlyContain(e => e.Source == "step-2" && e.Animated && e.Label == "Escalate");
        graph.Edges.Should().NotContain(e => escalations.Any(n => n.Id == e.Source));
    }

    [Fact]
    public void Layout_SpreadsNodesSharingARank_AcrossHorizontalLanes()
    {
        var graph = WorkflowDagCompiler.Compile(Build(LinearSteps(1), ["Escalate me"])).Value;

        // Terminal and the escalation sink both sit one rank below step-1 → two lanes centred on X=250.
        var terminal = graph.Nodes.Single(n => n.Type == WorkflowNodeType.Terminal);
        var escalation = graph.Nodes.Single(n => n.Type == WorkflowNodeType.Escalation);

        terminal.Position.Y.Should().Be(300);
        escalation.Position.Y.Should().Be(300);
        terminal.Position.X.Should().Be(110);
        escalation.Position.X.Should().Be(390);
        graph.Nodes.Single(n => n.Type == WorkflowNodeType.Start).Position.Should().Be(new NodePosition(250, 0));
    }

    [Fact]
    public void StepNode_CarriesScriptsRiskAndElevationInData()
    {
        var graph = WorkflowDagCompiler.Compile(Build(LinearSteps(1))).Value;

        var step = graph.Nodes.Single(n => n.Type == WorkflowNodeType.Step);
        step.Label.Should().Be("Title step-1");
        step.Data["risk"].Should().Be("low");
        step.Data["requires_elevation"].Should().Be(false);
        ((IEnumerable<string>)step.Data["scripts"]!).Should().Equal("echo step-1");
    }

    [Fact]
    public void Compile_IsDeterministic_ForIdenticalInput()
    {
        var steps = new[]
        {
            Step("step-1", Question("Q?", "Yes->step-3", "No")),
            Step("step-2"),
            Step("step-3")
        };

        var first = WorkflowDagCompiler.Compile(Build(steps, ["Escalate"])).Value;
        var second = WorkflowDagCompiler.Compile(Build(steps, ["Escalate"])).Value;

        ReactFlowSerializer.SerializeNodes(first.Nodes).Should().Be(ReactFlowSerializer.SerializeNodes(second.Nodes));
        ReactFlowSerializer.SerializeEdges(first.Edges).Should().Be(ReactFlowSerializer.SerializeEdges(second.Edges));
    }

    [Fact]
    public void LongLinearRunbook_Compiles_WithoutStackOverflow()
    {
        var result = WorkflowDagCompiler.Compile(Build(LinearSteps(3000)));

        result.IsSuccess.Should().BeTrue();
        result.Value.Nodes.Should().HaveCount(3002);
        result.Value.Nodes[^1].Position.Y.Should().Be(3001 * 150);
    }
}
