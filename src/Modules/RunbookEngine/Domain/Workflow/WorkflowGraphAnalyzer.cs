namespace RunbookForge.Modules.RunbookEngine.Domain.Workflow;

using RunbookForge.BuildingBlocks.Common.Results;

/// <summary>
/// Structural validation and ranking for workflow graphs. All failures are returned as
/// <see cref="Result"/> values; no method throws for malformed or cyclic graphs.
/// </summary>
public static class WorkflowGraphAnalyzer
{
    public const string CycleDetectedCode = "Workflow.CycleDetected";
    public const string DuplicateNodeCode = "Workflow.DuplicateNodeId";
    public const string InvalidEdgeCode = "Workflow.InvalidEdge";

    private enum VisitColor
    {
        White = 0,
        Gray = 1,
        Black = 2
    }

    /// <summary>
    /// Validates node-id uniqueness, edge endpoint integrity, and acyclicity.
    /// </summary>
    public static Result Validate(IReadOnlyList<WorkflowNode> nodes, IReadOnlyList<WorkflowEdge> edges)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(edges);

        var known = new HashSet<string>(StringComparer.Ordinal);
        foreach (var node in nodes)
        {
            if (!known.Add(node.Id))
            {
                return Result.Failure(Error.Validation(DuplicateNodeCode, $"Duplicate workflow node id '{node.Id}'."));
            }
        }

        foreach (var edge in edges)
        {
            if (!known.Contains(edge.Source) || !known.Contains(edge.Target))
            {
                return Result.Failure(Error.Validation(
                    InvalidEdgeCode,
                    $"Edge '{edge.Id}' references an unknown node ('{edge.Source}' -> '{edge.Target}')."));
            }
        }

        var cycleNode = FindCycleNode(nodes, edges);
        return cycleNode is null
            ? Result.Success()
            : Result.Failure(Error.Validation(CycleDetectedCode, $"Cycle detected involving node {cycleNode}"));
    }

    /// <summary>
    /// Iterative 3-colour (White/Gray/Black) depth-first search. Returns the id of a node that lies on a
    /// cycle, or <c>null</c> when the graph is acyclic. Iteration order is deterministic (node order, then edge order).
    /// </summary>
    public static string? FindCycleNode(IReadOnlyList<WorkflowNode> nodes, IReadOnlyList<WorkflowEdge> edges)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(edges);

        var adjacency = BuildAdjacency(nodes, edges);
        var color = new Dictionary<string, VisitColor>(StringComparer.Ordinal);

        foreach (var root in nodes)
        {
            if (color.GetValueOrDefault(root.Id) != VisitColor.White)
            {
                continue;
            }

            var stack = new Stack<(string Id, int NextIndex)>();
            color[root.Id] = VisitColor.Gray;
            stack.Push((root.Id, 0));

            while (stack.Count > 0)
            {
                var (id, nextIndex) = stack.Pop();
                var neighbours = adjacency[id];

                if (nextIndex >= neighbours.Count)
                {
                    color[id] = VisitColor.Black;
                    continue;
                }

                stack.Push((id, nextIndex + 1));
                var next = neighbours[nextIndex];

                switch (color.GetValueOrDefault(next))
                {
                    case VisitColor.Gray:
                        return next;
                    case VisitColor.White:
                        color[next] = VisitColor.Gray;
                        stack.Push((next, 0));
                        break;
                    case VisitColor.Black:
                        break;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the ids reachable from <paramref name="rootId"/> following edge direction (including the root).
    /// </summary>
    public static HashSet<string> ComputeReachable(
        string rootId,
        IReadOnlyList<WorkflowNode> nodes,
        IReadOnlyList<WorkflowEdge> edges)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(edges);

        var adjacency = BuildAdjacency(nodes, edges);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        if (!adjacency.ContainsKey(rootId))
        {
            return visited;
        }

        var queue = new Queue<string>();
        visited.Add(rootId);
        queue.Enqueue(rootId);

        while (queue.Count > 0)
        {
            foreach (var next in adjacency[queue.Dequeue()])
            {
                if (visited.Add(next))
                {
                    queue.Enqueue(next);
                }
            }
        }

        return visited;
    }

    /// <summary>
    /// Kahn's algorithm. Produces the longest-path rank (depth) of every node. If the graph still holds a cycle
    /// the result is a <c>Workflow.CycleDetected</c> validation failure.
    /// </summary>
    public static Result<IReadOnlyDictionary<string, int>> ComputeRanks(
        IReadOnlyList<WorkflowNode> nodes,
        IReadOnlyList<WorkflowEdge> edges)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(edges);

        var adjacency = BuildAdjacency(nodes, edges);
        var orderedIds = nodes.Select(n => n.Id).Distinct(StringComparer.Ordinal).ToList();
        var inDegree = orderedIds.ToDictionary(id => id, _ => 0, StringComparer.Ordinal);
        foreach (var targets in adjacency.Values)
        {
            foreach (var target in targets)
            {
                inDegree[target]++;
            }
        }

        var depth = orderedIds.ToDictionary(id => id, _ => 0, StringComparer.Ordinal);
        var queue = new Queue<string>(orderedIds.Where(id => inDegree[id] == 0));
        var processed = 0;

        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            processed++;

            foreach (var target in adjacency[id])
            {
                depth[target] = Math.Max(depth[target], depth[id] + 1);
                if (--inDegree[target] == 0)
                {
                    queue.Enqueue(target);
                }
            }
        }

        if (processed != orderedIds.Count)
        {
            var stuck = orderedIds.First(id => inDegree[id] > 0);
            return Result<IReadOnlyDictionary<string, int>>.Failure(
                Error.Validation(CycleDetectedCode, $"Cycle detected involving node {stuck}"));
        }

        return Result<IReadOnlyDictionary<string, int>>.Success(depth);
    }

    private static Dictionary<string, List<string>> BuildAdjacency(
        IReadOnlyList<WorkflowNode> nodes,
        IReadOnlyList<WorkflowEdge> edges)
    {
        var adjacency = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var node in nodes)
        {
            adjacency.TryAdd(node.Id, []);
        }

        foreach (var edge in edges)
        {
            // Edges with unknown endpoints are rejected by Validate; ignore them here so analysis never throws.
            if (adjacency.TryGetValue(edge.Source, out var targets) && adjacency.ContainsKey(edge.Target))
            {
                targets.Add(edge.Target);
            }
        }

        return adjacency;
    }
}
