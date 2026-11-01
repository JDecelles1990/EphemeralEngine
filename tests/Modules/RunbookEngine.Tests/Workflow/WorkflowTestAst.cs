namespace RunbookForge.Modules.RunbookEngine.Tests.Workflow;

using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;

internal static class WorkflowTestAst
{
    public static RunbookAstDocument Build(
        IReadOnlyList<StepNode> steps,
        IReadOnlyList<string>? escalationTriggers = null) =>
        new(
            new RunbookMetadata(
                Id: "rb-1",
                Title: "Test Runbook",
                Slug: "test-runbook",
                Category: "General",
                Level: "L1",
                Risk: "low",
                Author: "tester",
                Version: "1.0.0",
                SchemaVersion: "rf-v1",
                Tags: ["test"]),
            steps,
            escalationTriggers ?? [],
            RawMarkdown: null);

    public static StepNode Step(string id, params QuestionNode[] questions) =>
        new(
            Id: id,
            Title: $"Title {id}",
            Risk: "low",
            Commands: [new CommandNode("bash", $"echo {id}", RequiresElevation: false, Timeout: "15s", Mode: "readonly")],
            Assertions: [new AssertionNode("stdout", "contains", id, "Output should contain the step id.")],
            Questions: questions);

    public static QuestionNode Question(string prompt, params string[] options) => new(prompt, options);

    public static IReadOnlyList<StepNode> LinearSteps(int count) =>
        Enumerable.Range(1, count).Select(i => Step($"step-{i}")).ToList();
}

