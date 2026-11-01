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
        IReadOnlyList<string>? escalationTriggers = null,
        string? rawMarkdown = null,
        IReadOnlyList<string>? emptySections = null,
        string risk = "medium") =>
        new(
            Metadata: new RunbookMetadata(
                Id: "NET-101",
                Title: "Core Switch Troubleshooting",
                Slug: "core-switch-troubleshooting",
                Category: "networking",
                Level: "L2",
                Risk: risk,
                Author: "Network Ops",
                Version: "1.0.0",
                SchemaVersion: "rf-v1",
                Tags: ["cisco"]),
            Steps: steps ?? [new StepNode("step-1", "Diagnose Port Status", "low", [new CommandNode("bash", "show interfaces", false, Mode: "readonly")], [], [])],
            EscalationTriggers: escalationTriggers ?? ["Escalate to Network NOC team if down > 15m"],
            RawMarkdown: rawMarkdown ?? "# Test\nReference: https://wiki.internal/net",
            Symptoms: symptoms ?? ["Interface down"],
            RawSectionsFound: rawSections ?? ["Symptoms", "Scope", "Diagnostics", "Remediation", "Validation", "Escalation"],
            EmptySections: emptySections);

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

    [Fact]
    public async Task RF016_ShouldEmitWarning_WhenHeadingsDeviateFromCanonicalOrder()
    {
        var rule = new RF016_SectionOrderingDeviationRule();
        var ast = CreateAst(rawSections: ["Diagnostics", "Scope", "Remediation", "Validation", "Escalation"]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF016");
        diagnostics[0].Severity.Should().Be(LintSeverity.Warning);
        diagnostics[0].Message.Should().Contain("canonical order");
    }

    [Fact]
    public async Task RF016_ShouldPass_WhenHeadingsFollowCanonicalOrder()
    {
        var rule = new RF016_SectionOrderingDeviationRule();
        var ast = CreateAst(rawSections: ["Symptoms", "Scope", "Diagnostics", "Remediation", "Validation", "Escalation"]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task RF017_ShouldEmitWarning_WhenEscalationSectionIsNotActionable()
    {
        var rule = new RF017_UnactionableEscalationRule();
        var ast = CreateAst(
            rawSections: ["Escalation"],
            escalationTriggers: ["Escalate if unresolved after 10 minutes"],
            rawMarkdown: "# Escalation\nEscalate if unresolved after 10 minutes.");

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF017");
        diagnostics[0].Severity.Should().Be(LintSeverity.Warning);
    }

    [Fact]
    public async Task RF017_ShouldPass_WhenEscalationContainsTeamOrQueueReference()
    {
        var rule = new RF017_UnactionableEscalationRule();
        var ast = CreateAst(
            rawSections: ["Escalation"],
            escalationTriggers: ["Escalate to Network NOC team via Jira ticket"]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task RF018_ShouldEmitWarning_WhenMediumOrHigherRiskLacksPreconditionsOrScope()
    {
        var rule = new RF018_MissingPreconditionsRule();
        var ast = CreateAst(
            risk: "high",
            rawSections: ["Symptoms", "Diagnostics", "Remediation", "Validation", "Escalation"]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF018");
        diagnostics[0].Severity.Should().Be(LintSeverity.Warning);
    }

    [Fact]
    public async Task RF018_ShouldPass_WhenScopeOrPreconditionsPresentOrLowRisk()
    {
        var rule = new RF018_MissingPreconditionsRule();
        var astHighWithScope = CreateAst(risk: "high", rawSections: ["Symptoms", "Scope", "Diagnostics"]);
        (await rule.EvaluateAsync(astHighWithScope)).Should().BeEmpty();

        var astLowNoScope = CreateAst(risk: "low", rawSections: ["Symptoms", "Diagnostics"]);
        (await rule.EvaluateAsync(astLowNoScope)).Should().BeEmpty();
    }

    [Fact]
    public async Task RF019_ShouldEmitInfo_WhenReferencesAndLinksAreAbsent()
    {
        var rule = new RF019_MissingReferencesRule();
        var ast = CreateAst(
            rawSections: ["Symptoms", "Diagnostics"],
            rawMarkdown: "# Switch Runbook\nPlain text with no external URLs.");

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF019");
        diagnostics[0].Severity.Should().Be(LintSeverity.Info);
    }

    [Fact]
    public async Task RF019_ShouldPass_WhenReferencesSectionOrExternalLinkPresent()
    {
        var rule = new RF019_MissingReferencesRule();
        var astWithLink = CreateAst(rawMarkdown: "Reference: https://cisco.com/troubleshoot");
        (await rule.EvaluateAsync(astWithLink)).Should().BeEmpty();

        var astWithHeading = CreateAst(rawSections: ["References"], rawMarkdown: "Internal docs");
        (await rule.EvaluateAsync(astWithHeading)).Should().BeEmpty();
    }

    [Fact]
    public async Task RF020_ShouldEmitWarning_WhenSectionBodyIsEmpty()
    {
        var rule = new RF020_EmptySectionBodyRule();
        var ast = CreateAst(emptySections: ["Remediation", "Validation"]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().HaveCount(2);
        diagnostics[0].RuleId.Should().Be("RF020");
        diagnostics[0].Severity.Should().Be(LintSeverity.Warning);
        diagnostics[0].Message.Should().Contain("Remediation");
        diagnostics[1].Message.Should().Contain("Validation");
    }

    [Fact]
    public async Task RF020_ShouldPass_WhenNoSectionsAreEmpty()
    {
        var rule = new RF020_EmptySectionBodyRule();
        var ast = CreateAst(emptySections: []);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }
}

