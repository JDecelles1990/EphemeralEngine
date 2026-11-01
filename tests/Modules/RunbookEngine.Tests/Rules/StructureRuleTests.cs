namespace RunbookForge.Modules.RunbookEngine.Tests.Rules;

using FluentAssertions;
using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;
using RunbookForge.Modules.RunbookEngine.Domain.Rules.Structure;
using Xunit;

public class StructureRuleTests
{
    private static RunbookAstDocument CreateAst(
        IReadOnlyList<string>? rawSections = null,
        IReadOnlyList<string>? symptoms = null,
        IReadOnlyList<StepNode>? steps = null,
        IReadOnlyList<string>? escalationTriggers = null) =>
        new(
            Metadata: new RunbookMetadata(
                Id: "NET-101",
                Title: "Core Switch Troubleshooting",
                Slug: "core-switch-troubleshooting",
                Category: "networking",
                Level: "L2",
                Risk: "medium",
                Author: "Network Ops",
                Version: "1.0.0",
                SchemaVersion: "rf-v1",
                Tags: ["cisco"]),
            Steps: steps ?? [new StepNode("step-1", "Diagnose Port Status", "low", [new CommandNode("bash", "show interfaces", false, Mode: "readonly")], [], [])],
            EscalationTriggers: escalationTriggers ?? ["Escalate to L3 if down > 15m"],
            RawMarkdown: "# Test",
            Symptoms: symptoms ?? ["Interface down"],
            RawSectionsFound: rawSections ?? ["Symptoms", "Diagnostics", "Remediation", "Validation", "Escalation"]);

    [Fact]
    public async Task RF011_ShouldEmitError_WhenSymptomsSectionIsMissing()
    {
        var rule = new RF011_MissingSymptomsSectionRule();
        var ast = CreateAst(rawSections: ["Diagnostics", "Remediation", "Validation", "Escalation"], symptoms: ["Interface down"]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF011");
        diagnostics[0].Severity.Should().Be(LintSeverity.Error);
        diagnostics[0].Message.Should().Contain("# Symptoms");
    }

    [Fact]
    public async Task RF011_ShouldEmitError_WhenSymptomsCollectionIsEmpty()
    {
        var rule = new RF011_MissingSymptomsSectionRule();
        var ast = CreateAst(rawSections: ["Symptoms", "Diagnostics", "Remediation", "Validation", "Escalation"], symptoms: []);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF011");
        diagnostics[0].Severity.Should().Be(LintSeverity.Error);
    }

    [Fact]
    public async Task RF011_ShouldPass_WhenSymptomsHeadingAndItemsArePresent()
    {
        var rule = new RF011_MissingSymptomsSectionRule();
        var ast = CreateAst(rawSections: ["Symptoms"], symptoms: ["High latency", "Packet drops"]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task RF012_ShouldEmitError_WhenDiagnosticsSectionAndStepsAreAbsent()
    {
        var rule = new RF012_MissingDiagnosticsSectionRule();
        var nonDiagStep = new StepNode("step-1", "Apply Configuration", "medium", [new CommandNode("bash", "configure terminal", false, Mode: "write")], [], []);
        var ast = CreateAst(rawSections: ["Symptoms", "Remediation", "Validation", "Escalation"], steps: [nonDiagStep]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF012");
        diagnostics[0].Severity.Should().Be(LintSeverity.Error);
    }

    [Fact]
    public async Task RF012_ShouldPass_WhenDiagnosticsSectionOrStepIsPresent()
    {
        var rule = new RF012_MissingDiagnosticsSectionRule();

        var astWithHeading = CreateAst(rawSections: ["Diagnostics"]);
        (await rule.EvaluateAsync(astWithHeading)).Should().BeEmpty();

        var diagStep = new StepNode("diag-1", "Inspect Counters", "low", [], [], []);
        var astWithDiagStep = CreateAst(rawSections: ["Symptoms"], steps: [diagStep]);
        (await rule.EvaluateAsync(astWithDiagStep)).Should().BeEmpty();
    }

    [Fact]
    public async Task RF013_ShouldEmitError_WhenRemediationSectionIsAbsent()
    {
        var rule = new RF013_MissingRemediationSectionRule();
        var ast = CreateAst(rawSections: ["Symptoms", "Diagnostics", "Validation", "Escalation"]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF013");
        diagnostics[0].Severity.Should().Be(LintSeverity.Error);
        diagnostics[0].Message.Should().Contain("# Remediation");
    }

    [Fact]
    public async Task RF013_ShouldPass_WhenRemediationSectionIsPresent()
    {
        var rule = new RF013_MissingRemediationSectionRule();
        var ast = CreateAst(rawSections: ["Remediation Procedure"]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task RF014_ShouldEmitError_WhenValidationSectionIsAbsent()
    {
        var rule = new RF014_MissingValidationSectionRule();
        var ast = CreateAst(rawSections: ["Symptoms", "Diagnostics", "Remediation", "Escalation"]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF014");
        diagnostics[0].Severity.Should().Be(LintSeverity.Error);
        diagnostics[0].Message.Should().Contain("# Validation");
    }

    [Fact]
    public async Task RF014_ShouldPass_WhenValidationSectionIsPresent()
    {
        var rule = new RF014_MissingValidationSectionRule();
        var ast = CreateAst(rawSections: ["Validation & Verification"]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task RF015_ShouldEmitError_WhenEscalationSectionOrTriggersAbsent()
    {
        var rule = new RF015_MissingEscalationSectionRule();

        var astNoHeading = CreateAst(rawSections: ["Symptoms", "Diagnostics"], escalationTriggers: ["Trigger 1"]);
        (await rule.EvaluateAsync(astNoHeading)).Should().ContainSingle();

        var astNoTriggers = CreateAst(rawSections: ["Escalation Triggers"], escalationTriggers: []);
        (await rule.EvaluateAsync(astNoTriggers)).Should().ContainSingle();
    }

    [Fact]
    public async Task RF015_ShouldPass_WhenEscalationHeadingAndTriggersPresent()
    {
        var rule = new RF015_MissingEscalationSectionRule();
        var ast = CreateAst(rawSections: ["Escalation Matrix"], escalationTriggers: ["MTTR > 15m"]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }
}
