namespace RunbookForge.Modules.RunbookEngine.Tests.Rules;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RunbookForge.Modules.RunbookEngine.Application.Features.LintRunbook;
using RunbookForge.Modules.RunbookEngine.Domain.Ast;
using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;
using RunbookForge.Modules.RunbookEngine.Domain.Rules;
using RunbookForge.Modules.RunbookEngine.Infrastructure.Extensions;
using Xunit;

public class RunbookLinterEngineTests
{
    private static RunbookLinterEngine CreateEngine()
    {
        var services = new ServiceCollection();
        services.AddRunbookLinter();
        var sp = services.BuildServiceProvider();
        return sp.GetRequiredService<RunbookLinterEngine>();
    }

    private static RunbookAstDocument CreateCompliantAst() =>
        new(
            Metadata: new RunbookMetadata(
                Id: "NET-1001",
                Title: "Core Switch Port Saturation Remediation",
                Slug: "core-switch-port-saturation",
                Category: "networking",
                Level: "L2",
                Risk: "medium",
                Author: "DevOps Core",
                Version: "1.0.0",
                SchemaVersion: "rf-v1",
                Tags: ["cisco", "switch", "p1-incident"],
                LastReviewed: "2026-11-01",
                Audience: "Network Operations",
                HasFrontmatter: true),
            Steps:
            [
                new StepNode("diag-1", "Inspect Port Buffer Counters", "low",
                    [new CommandNode("bash", "show interface counters", false, Timeout: "15s", Mode: "readonly")],
                    [new AssertionNode("stdout", "contains", "GigabitEthernet0/1")],
                    [])
            ],
            EscalationTriggers: ["Escalate to Network NOC team if MTTR exceeds 15 minutes"],
            RawMarkdown: "# Test\nSee https://wiki.internal/network-troubleshooting for references.",
            Symptoms: ["Packet drop alarms", "High CPU on switch ASIC"],
            RawSectionsFound: ["Symptoms", "Scope", "Diagnostics", "Remediation", "Validation", "Escalation", "References"]);

    [Fact]
    public async Task LintAsync_ShouldReturnScore100AndNoDiagnostics_WhenAstIsFullyCompliant()
    {
        var engine = CreateEngine();
        var ast = CreateCompliantAst();

        var report = await engine.LintAsync(ast);

        report.Score.Should().Be(100);
        report.Diagnostics.Should().BeEmpty();
        report.HasErrors.Should().BeFalse();
        report.HasWarnings.Should().BeFalse();
    }

    [Theory]
    [InlineData(0, 0, 0, 100)]
    [InlineData(1, 0, 0, 85)]
    [InlineData(0, 1, 0, 95)]
    [InlineData(0, 0, 1, 99)]
    [InlineData(1, 1, 1, 79)]
    [InlineData(2, 2, 2, 58)]
    [InlineData(5, 0, 0, 25)]
    [InlineData(7, 0, 0, 0)]
    [InlineData(10, 5, 3, 0)]
    public void LinterReport_ShouldCalculateScoreDeductionsAccurately(int errors, int warnings, int infos, int expectedScore)
    {
        var diagnostics = new List<LintDiagnostic>();
        for (var i = 0; i < errors; i++) diagnostics.Add(new LintDiagnostic($"E{i}", LintSeverity.Error, "Error"));
        for (var i = 0; i < warnings; i++) diagnostics.Add(new LintDiagnostic($"W{i}", LintSeverity.Warning, "Warning"));
        for (var i = 0; i < infos; i++) diagnostics.Add(new LintDiagnostic($"I{i}", LintSeverity.Info, "Info"));

        var report = LinterReport.Create(diagnostics);

        report.Score.Should().Be(expectedScore);
        report.HasErrors.Should().Be(errors > 0);
        report.HasWarnings.Should().Be(warnings > 0);
        report.Diagnostics.Should().HaveCount(errors + warnings + infos);
    }

    [Fact]
    public async Task Handler_ShouldReturnSuccessWithReport_WhenValidAstProvided()
    {
        var engine = CreateEngine();
        var handler = new LintRunbookHandler(engine);
        var ast = CreateCompliantAst();

        var result = await handler.Handle(new LintRunbookQuery(ast), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Score.Should().Be(100);
        result.Value.Diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task Handler_ShouldReturnFailure_WhenNullAstProvided()
    {
        var engine = CreateEngine();
        var handler = new LintRunbookHandler(engine);

        var result = await handler.Handle(new LintRunbookQuery(null!), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("LintRunbook.NullAst");
    }

    [Fact]
    public async Task EndToEnd_MarkdownWithViolations_ShouldDetectExpectedDiagnostics()
    {
        var engine = CreateEngine();
        var rawMarkdown = """
        ---
        id: "bad-id"
        title: "Fix"
        category: "unknown"
        risk: "extreme"
        level: "L9"
        tags: []
        ---

        # Overview
        Just an overview, no required sections.
        """;

        var markdownDoc = Markdig.Markdown.Parse(rawMarkdown, RunbookMarkdownPipeline.Instance);
        var ast = RunbookAstVisitor.Visit(markdownDoc, rawMarkdown);

        var report = await engine.LintAsync(ast);

        report.HasErrors.Should().BeTrue();
        report.HasWarnings.Should().BeTrue();
        report.Diagnostics.Should().Contain(d => d.RuleId == "RF002"); // bad id
        report.Diagnostics.Should().Contain(d => d.RuleId == "RF003"); // title < 5
        report.Diagnostics.Should().Contain(d => d.RuleId == "RF004"); // category
        report.Diagnostics.Should().Contain(d => d.RuleId == "RF005"); // risk
        report.Diagnostics.Should().Contain(d => d.RuleId == "RF006"); // level
        report.Diagnostics.Should().Contain(d => d.RuleId == "RF008"); // empty tags
        report.Diagnostics.Should().Contain(d => d.RuleId == "RF011"); // missing symptoms
        report.Diagnostics.Should().Contain(d => d.RuleId == "RF012"); // missing diagnostics
        report.Diagnostics.Should().Contain(d => d.RuleId == "RF013"); // missing remediation
        report.Diagnostics.Should().Contain(d => d.RuleId == "RF014"); // missing validation
        report.Diagnostics.Should().Contain(d => d.RuleId == "RF015"); // missing escalation
    }
}

