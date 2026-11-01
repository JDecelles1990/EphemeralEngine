namespace RunbookForge.Modules.RunbookEngine.Domain.Workflow;

using RunbookForge.BuildingBlocks.Common.Results;
using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;

/// <summary>
/// Compiles a parsed <see cref="RunbookAstDocument"/> into an executable, laid-out DAG.
/// <para>
/// Flow rules:
/// <list type="bullet">
/// <item>Start → Step₁ → … → Stepₙ → Terminal (sequential edges).</item>
/// <item>A step with <c>:::question</c> blocks is followed by one Decision node per question. Each option becomes an
/// outgoing edge. An option written <c>Label-&gt;step-id</c> jumps to that step; a plain option falls through to the
/// next decision / next step.</item>
/// <item>Each escalation trigger becomes an Escalation sink node attached to the last step.</item>
/// </list>
/// Malformed input never throws; it yields <c>Result.Failure(Error.Validation(...))</c>.
/// </para>
/// </summary>
public static class WorkflowDagCompiler
{
    public const string StartNodeId = "__start";
    public const string TerminalNodeId = "__terminal";

    public const string NoStepsCode = "Workflow.NoSteps";
    public const string DuplicateStepCode = "Workflow.DuplicateStepId";
    public const string InvalidBranchOptionCode = "Workflow.InvalidBranchOption";
    public const string InvalidBranchTargetCode = "Workflow.InvalidBranchTarget";
    public const string QuestionWithoutOptionsCode = "Workflow.QuestionWithoutOptions";
    public const string UnreachableStepCode = "Workflow.UnreachableStep";

    private const string BranchSeparator = "->";
    private const int CenterX = 250;
    private const int LaneSpacing = 280;
    private const int RankSpacing = 150;

    public static Result<WorkflowGraph> Compile(RunbookAstDocument ast)
    {
        ArgumentNullException.ThrowIfNull(ast);

        if (ast.Steps.Count == 0)
        {
            return Fail(Error.Validation(NoStepsCode, "A runbook must contain at least one step to compile a workflow."));
        }

        var stepIds = new List<string>(ast.Steps.Count);
        var knownStepIds = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < ast.Steps.Count; i++)
        {
            var id = string.IsNullOrWhiteSpace(ast.Steps[i].Id) ? $"step-{i + 1}" : ast.Steps[i].Id.Trim();
            if (!knownStepIds.Add(id))
            {
                return Fail(Error.Validation(DuplicateStepCode, $"Duplicate step id '{id}'."));
            }

            stepIds.Add(id);
        }

        var nodes = new List<WorkflowNode>();
        var edges = new EdgeCollector();

        var topologyError = BuildTopology(ast, stepIds, knownStepIds, nodes, edges);
        if (topologyError is { } buildError)
        {
            return Fail(buildError);
        }

        var edgeList = edges.Edges;

        var validation = WorkflowGraphAnalyzer.Validate(nodes, edgeList);
        if (validation.IsFailure)
        {
            return Fail(validation.Error);
        }

        var reachable = WorkflowGraphAnalyzer.ComputeReachable(StartNodeId, nodes, edgeList);
        var unreachable = stepIds.Where(id => !reachable.Contains(id)).ToList();
        if (unreachable.Count > 0)
        {
            return Fail(Error.Validation(
                UnreachableStepCode,
                $"Step(s) not reachable from the start node: {string.Join(", ", unreachable)}."));
        }

        var ranks = WorkflowGraphAnalyzer.ComputeRanks(nodes, edgeList);
        if (ranks.IsFailure)
        {
            return Fail(ranks.Error);
        }

        var positioned = ApplyLayout(nodes, ranks.Value);
        return Result<WorkflowGraph>.Success(new WorkflowGraph(positioned, edgeList, IsAcyclic: true));
    }

    private static Error? BuildTopology(
        RunbookAstDocument ast,
        List<string> stepIds,
        IReadOnlySet<string> knownStepIds,
        List<WorkflowNode> nodes,
        EdgeCollector edges)
    {
        nodes.Add(CreateNode(StartNodeId, WorkflowNodeType.Start, ast.Metadata.Title, new Dictionary<string, object?>
        {
            ["title"] = ast.Metadata.Title,
            ["slug"] = ast.Metadata.Slug,
            ["category"] = ast.Metadata.Category,
            ["level"] = ast.Metadata.Level,
            ["risk"] = ast.Metadata.Risk,
            ["tags"] = ast.Metadata.Tags.ToList()
        }));
        edges.Add(StartNodeId, stepIds[0], label: null, animated: false);

        for (var i = 0; i < ast.Steps.Count; i++)
        {
            var step = ast.Steps[i];
            var stepId = stepIds[i];
            var nextTarget = i + 1 < ast.Steps.Count ? stepIds[i + 1] : TerminalNodeId;

            nodes.Add(CreateNode(
                stepId,
                WorkflowNodeType.Step,
                string.IsNullOrWhiteSpace(step.Title) ? stepId : step.Title,
                BuildStepData(step)));

            if (step.Questions.Count == 0)
            {
                edges.Add(stepId, nextTarget, label: null, animated: false);
                continue;
            }

            var decisionIds = Enumerable.Range(1, step.Questions.Count)
                .Select(k => $"{stepId}__decision_{k}")
                .ToList();

            edges.Add(stepId, decisionIds[0], label: null, animated: false);

            for (var k = 0; k < step.Questions.Count; k++)
            {
                var question = step.Questions[k];
                if (question.Options.Count == 0)
                {
                    return Error.Validation(
                        QuestionWithoutOptionsCode,
                        $"Question '{question.Prompt}' in step '{stepId}' has no options.");
                }

                var fallThrough = k + 1 < step.Questions.Count ? decisionIds[k + 1] : nextTarget;
                var labels = new List<string>(question.Options.Count);
                var branches = new List<(string Label, string Target)>(question.Options.Count);

                foreach (var rawOption in question.Options)
                {
                    var parsed = ParseOption(rawOption, stepId, knownStepIds, fallThrough);
                    if (parsed.Error is { } optionError)
                    {
                        return optionError;
                    }

                    labels.Add(parsed.Label);
                    branches.Add((parsed.Label, parsed.Target));
                }

                nodes.Add(CreateNode(decisionIds[k], WorkflowNodeType.Decision, question.Prompt, new Dictionary<string, object?>
                {
                    ["prompt"] = question.Prompt,
                    ["options"] = labels,
                    ["question_index"] = k + 1
                }));

                foreach (var (label, target) in branches)
                {
                    edges.Add(decisionIds[k], target, label, animated: false);
                }
            }
        }

        nodes.Add(CreateNode(TerminalNodeId, WorkflowNodeType.Terminal, "Resolved", new Dictionary<string, object?>()));

        var escalationNumber = 0;
        foreach (var trigger in ast.EscalationTriggers.Where(t => !string.IsNullOrWhiteSpace(t)))
        {
            escalationNumber++;
            var escalationId = $"__escalation_{escalationNumber}";
            nodes.Add(CreateNode(escalationId, WorkflowNodeType.Escalation, trigger, new Dictionary<string, object?>
            {
                ["trigger"] = trigger
            }));
            edges.Add(stepIds[^1], escalationId, label: "Escalate", animated: true);
        }

        return null;
    }

    private static (string Label, string Target, Error? Error) ParseOption(
        string rawOption,
        string stepId,
        IReadOnlySet<string> knownStepIds,
        string fallThrough)
    {
        var separatorIndex = rawOption.IndexOf(BranchSeparator, StringComparison.Ordinal);
        var label = (separatorIndex < 0 ? rawOption : rawOption[..separatorIndex]).Trim();

        if (label.Length == 0)
        {
            return (string.Empty, string.Empty, Error.Validation(
                InvalidBranchOptionCode,
                $"A question option in step '{stepId}' has an empty label ('{rawOption}')."));
        }

        if (separatorIndex < 0)
        {
            return (label, fallThrough, null);
        }

        var target = rawOption[(separatorIndex + BranchSeparator.Length)..].Trim();
        if (target.Length == 0 || !knownStepIds.Contains(target))
        {
            return (label, string.Empty, Error.Validation(
                InvalidBranchTargetCode,
                $"Option '{label}' in step '{stepId}' references unknown step '{target}'."));
        }

        return (label, target, null);
    }

    private static Dictionary<string, object?> BuildStepData(StepNode step)
    {
        var data = new Dictionary<string, object?>
        {
            ["risk"] = step.Risk,
            ["scripts"] = step.Commands.Select(c => c.Script).ToList(),
            ["requires_elevation"] = step.Commands.Any(c => c.RequiresElevation),
            ["commands"] = step.Commands.Select(c => (object)new Dictionary<string, object?>
            {
                ["language"] = c.Language,
                ["script"] = c.Script,
                ["requires_elevation"] = c.RequiresElevation,
                ["timeout"] = c.Timeout,
                ["mode"] = c.Mode
            }).ToList(),
            ["assertions"] = step.Assertions.Select(a => (object)new Dictionary<string, object?>
            {
                ["target"] = a.Target,
                ["comparator"] = a.Comparator,
                ["expected_value"] = a.ExpectedValue,
                ["message"] = a.Message
            }).ToList()
        };

        if (!string.IsNullOrWhiteSpace(step.Description))
        {
            data["description"] = step.Description;
        }

        return data;
    }

    private static List<WorkflowNode> ApplyLayout(
        IReadOnlyList<WorkflowNode> nodes,
        IReadOnlyDictionary<string, int> ranks)
    {
        var lanesByRank = nodes
            .GroupBy(n => ranks[n.Id])
            .ToDictionary(g => g.Key, g => g.Select(n => n.Id).ToList());

        return nodes.Select(node =>
        {
            var rank = ranks[node.Id];
            var lanes = lanesByRank[rank];
            var lane = lanes.IndexOf(node.Id);

            // X = 250 + (lane - center) * 280, with center = (count - 1) / 2, kept in integer arithmetic.
            var x = CenterX + ((2 * lane) - (lanes.Count - 1)) * (LaneSpacing / 2);
            return node with { Position = new NodePosition(x, rank * RankSpacing) };
        }).ToList();
    }

    private static WorkflowNode CreateNode(
        string id,
        WorkflowNodeType type,
        string label,
        IReadOnlyDictionary<string, object?> data) =>
        new(id, type, label, new NodePosition(0, 0), data);

    private static Result<WorkflowGraph> Fail(Error error) => Result<WorkflowGraph>.Failure(error);

    private sealed class EdgeCollector
    {
        private readonly List<WorkflowEdge> _edges = [];
        private readonly Dictionary<string, int> _idCounts = new(StringComparer.Ordinal);

        public IReadOnlyList<WorkflowEdge> Edges => _edges;

        public void Add(string source, string target, string? label, bool animated)
        {
            var baseId = $"{source}->{target}";
            var count = _idCounts.GetValueOrDefault(baseId) + 1;
            _idCounts[baseId] = count;

            var id = count == 1 ? baseId : $"{baseId}#{count}";
            _edges.Add(new WorkflowEdge(id, source, target, label, animated));
        }
    }
}
