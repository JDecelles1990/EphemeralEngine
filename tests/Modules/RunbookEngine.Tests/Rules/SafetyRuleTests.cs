namespace RunbookForge.Modules.RunbookEngine.Tests.Rules;

using FluentAssertions;
using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;
using RunbookForge.Modules.RunbookEngine.Domain.Rules.Safety;
using Xunit;

public class SafetyRuleTests
{
    private static RunbookAstDocument CreateAst(
        string risk = "low",
        IReadOnlyList<StepNode>? steps = null,
        IReadOnlyList<string>? rawSections = null,
        string? rawMarkdown = null) =>
        new(
            Metadata: new RunbookMetadata(
                Id: "NET-201",
                Title: "Core Network Switch Operation",
                Slug: "core-network-switch-operation",
                Category: "networking",
                Level: "L2",
                Risk: risk,
                Author: "NetSec Ops",
                Version: "1.0.0",
                SchemaVersion: "rf-v1",
                Tags: ["network", "switch"]),
            Steps: steps ?? [],
            EscalationTriggers: ["Escalate to Network NOC team if packet loss persists"],
            RawMarkdown: rawMarkdown ?? "# Core Network Operation\nSee https://wiki.internal/network",
            Symptoms: ["Packet loss"],
            RawSectionsFound: rawSections ?? ["Symptoms", "Scope", "Diagnostics", "Remediation", "Validation", "Escalation"]);

    [Fact]
    public async Task RF021_ShouldEmitWarning_WhenCommandHasEmptyOrUnrecognizedLanguage()
    {
        var rule = new RF021_UntypedCodeFenceRule();
        var step = new StepNode(
            "step-1",
            "Run Diagnostic Script",
            "low",
            [new CommandNode("unknown_lang", "run-script.xyz", false)],
            [],
            []);
        var ast = CreateAst(steps: [step]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF021");
        diagnostics[0].Severity.Should().Be(LintSeverity.Warning);
    }

    [Fact]
    public async Task RF021_ShouldPass_WhenCommandUsesRecognizedLanguage()
    {
        var rule = new RF021_UntypedCodeFenceRule();
        var step = new StepNode(
            "step-1",
            "Run Diagnostic Script",
            "low",
            [new CommandNode("powershell", "Get-Service wuauserv", false)],
            [],
            []);
        var ast = CreateAst(steps: [step]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task RF022_ShouldEmitError_WhenLowRiskRunbookContainsMutatingCommand()
    {
        var rule = new RF022_StateMutationInLowRiskRule();
        var step = new StepNode(
            "step-mut",
            "Restart Core Service",
            "low",
            [new CommandNode("powershell", "Restart-Service -Name Spooler -Force", false)],
            [],
            []);
        var ast = CreateAst(risk: "low", steps: [step]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF022");
        diagnostics[0].Severity.Should().Be(LintSeverity.Error);
        diagnostics[0].Message.Should().Contain("Restart-Service");
    }

    [Fact]
    public async Task RF022_ShouldPass_WhenLowRiskRunbookUsesReadOnlyCmdlets()
    {
        var rule = new RF022_StateMutationInLowRiskRule();
        var step = new StepNode(
            "step-ro",
            "Inspect Service State",
            "low",
            [new CommandNode("powershell", "Get-Service Spooler | Select-Object Status, Name | Format-Table -AutoSize", false)],
            [],
            []);
        var ast = CreateAst(risk: "low", steps: [step]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task RF022_ShouldPass_WhenMediumOrHighRiskRunbookContainsMutatingCommand()
    {
        var rule = new RF022_StateMutationInLowRiskRule();
        var step = new StepNode(
            "step-mut",
            "Clear Cache and Restart",
            "medium",
            [new CommandNode("powershell", "Clear-DnsClientCache; Restart-Service Dnscache", false)],
            [],
            []);
        var ast = CreateAst(risk: "medium", steps: [step]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task RF023_ShouldEmitWarning_WhenElevationKeywordUsedWithoutElevationFlag()
    {
        var rule = new RF023_UndeclaredElevationRule();
        var step = new StepNode(
            "step-elev",
            "Elevated Process Launch",
            "low",
            [new CommandNode("powershell", "Start-Process powershell -Verb RunAs", RequiresElevation: false)],
            [],
            []);
        var ast = CreateAst(steps: [step]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF023");
        diagnostics[0].Severity.Should().Be(LintSeverity.Warning);
    }

    [Fact]
    public async Task RF023_ShouldPass_WhenElevationDeclaredProperly()
    {
        var rule = new RF023_UndeclaredElevationRule();
        var step = new StepNode(
            "step-elev",
            "Elevated Process Launch",
            "high",
            [new CommandNode("powershell", "Start-Process powershell -Verb RunAs", RequiresElevation: true)],
            [],
            []);
        var ast = CreateAst(steps: [step]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task RF024_ShouldEmitWarning_WhenDiagnosticStepHasCommandsWithoutAssertions()
    {
        var rule = new RF024_MissingCommandAssertionRule();
        var step = new StepNode(
            "diag-step-1",
            "Diagnostic Network Test",
            "low",
            [new CommandNode("bash", "netstat -ano", false)],
            [], // zero assertions
            []);
        var ast = CreateAst(steps: [step]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF024");
        diagnostics[0].Severity.Should().Be(LintSeverity.Warning);
    }

    [Fact]
    public async Task RF024_ShouldPass_WhenDiagnosticStepHasAssertions()
    {
        var rule = new RF024_MissingCommandAssertionRule();
        var step = new StepNode(
            "diag-step-1",
            "Diagnostic Network Test",
            "low",
            [new CommandNode("bash", "netstat -ano", false)],
            [new AssertionNode("stdout", "contains", "LISTENING")],
            []);
        var ast = CreateAst(steps: [step]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task RF025_ShouldEmitInfo_WhenCommandContainsUnboundedPolling()
    {
        var rule = new RF025_UnboundedExecutionRule();
        var step = new StepNode(
            "poll-step",
            "Continuous Ping",
            "low",
            [new CommandNode("cmd", "ping 192.168.1.1 -t", false)],
            [],
            []);
        var ast = CreateAst(steps: [step]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF025");
        diagnostics[0].Severity.Should().Be(LintSeverity.Info);
        diagnostics[0].Message.Should().Contain("unbounded polling");
    }

    [Fact]
    public async Task RF025_ShouldPass_WhenCommandsAreBounded()
    {
        var rule = new RF025_UnboundedExecutionRule();
        var step = new StepNode(
            "poll-step",
            "Bounded Ping",
            "low",
            [new CommandNode("cmd", "ping 192.168.1.1 -n 4", false)],
            [],
            []);
        var ast = CreateAst(steps: [step]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task RF026_ShouldEmitWarning_WhenRemediationHasNoSubsequentValidation()
    {
        var rule = new RF026_MissingRemediationValidationRule();
        var remStep = new StepNode(
            "rem-01",
            "Remediation Action",
            "medium",
            [new CommandNode("powershell", "Restart-Service Spooler", false)],
            [], // no assertions
            []);
        var ast = CreateAst(
            rawSections: ["Symptoms", "Diagnostics", "Remediation"], // No Validation section
            steps: [remStep]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF026");
        diagnostics[0].Severity.Should().Be(LintSeverity.Warning);
    }

    [Fact]
    public async Task RF026_ShouldPass_WhenValidationSectionOrStepExists()
    {
        var rule = new RF026_MissingRemediationValidationRule();
        var remStep = new StepNode(
            "rem-01",
            "Remediation Action",
            "medium",
            [new CommandNode("powershell", "Restart-Service Spooler", false)],
            [],
            []);
        var ast = CreateAst(
            rawSections: ["Symptoms", "Diagnostics", "Remediation", "Validation"],
            steps: [remStep]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task RF027_ShouldEmitError_WhenDecisionBranchHasFewerThanTwoDistinctOptions()
    {
        var rule = new RF027_IncompleteDecisionBranchRule();
        var step = new StepNode(
            "dec-01",
            "Decision Step",
            "low",
            [],
            [],
            [new QuestionNode("Is the port link LED green?", ["Yes"])]); // Only 1 option
        var ast = CreateAst(steps: [step]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF027");
        diagnostics[0].Severity.Should().Be(LintSeverity.Error);
    }

    [Fact]
    public async Task RF027_ShouldPass_WhenDecisionBranchHasMultipleDistinctOptions()
    {
        var rule = new RF027_IncompleteDecisionBranchRule();
        var step = new StepNode(
            "dec-01",
            "Decision Step",
            "low",
            [],
            [],
            [new QuestionNode("Is the port link LED green?", ["Yes", "No", "Amber"])]);
        var ast = CreateAst(steps: [step]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task RF028_ShouldEmitError_WhenHardcodedCredentialsDetected()
    {
        var rule = new RF028_HardcodedSecretDetectedRule();
        var step = new StepNode(
            "step-sec",
            "Authenticate API",
            "medium",
            [new CommandNode("powershell", "$password = 'SuperSecretP@ss123!'", false)],
            [],
            []);
        var ast = CreateAst(steps: [step]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF028");
        diagnostics[0].Severity.Should().Be(LintSeverity.Error);
        diagnostics[0].Message.Should().Contain("SuperSecretP@ss123!");
    }

    [Fact]
    public async Task RF028_ShouldPass_WhenCredentialsAreParameterized()
    {
        var rule = new RF028_HardcodedSecretDetectedRule();
        var step = new StepNode(
            "step-sec",
            "Authenticate API",
            "medium",
            [new CommandNode("powershell", "$cred = Get-Credential; Connect-AzAccount -Credential $cred", false)],
            [],
            []);
        var ast = CreateAst(steps: [step]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task RF029_ShouldEmitWarning_WhenHighRiskRunbookLacksRollbackProcedure()
    {
        var rule = new RF029_MissingRollbackProcedureRule();
        var ast = CreateAst(
            risk: "high",
            rawSections: ["Symptoms", "Scope", "Diagnostics", "Remediation", "Validation", "Escalation"],
            rawMarkdown: "# High Risk Operation\nNo rollback defined.");

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF029");
        diagnostics[0].Severity.Should().Be(LintSeverity.Warning);
    }

    [Fact]
    public async Task RF029_ShouldPass_WhenHighRiskRunbookDefinesRollback()
    {
        var rule = new RF029_MissingRollbackProcedureRule();
        var ast = CreateAst(
            risk: "high",
            rawSections: ["Symptoms", "Scope", "Diagnostics", "Remediation", "Validation", "Rollback", "Escalation"],
            rawMarkdown: "# High Risk Operation\n## Rollback\nRestore database from backup.");

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task RF030_ShouldEmitWarning_WhenPackageInstallLacksVersionPin()
    {
        var rule = new RF030_UnpinnedSoftwareInstallRule();
        var step = new StepNode(
            "pkg-step",
            "Install Tooling",
            "medium",
            [new CommandNode("powershell", "winget install Git.Git", false)],
            [],
            []);
        var ast = CreateAst(steps: [step]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF030");
        diagnostics[0].Severity.Should().Be(LintSeverity.Warning);
        diagnostics[0].Message.Should().Contain("unpinned");
    }

    [Fact]
    public async Task RF030_ShouldPass_WhenPackageInstallHasVersionPin()
    {
        var rule = new RF030_UnpinnedSoftwareInstallRule();
        var step = new StepNode(
            "pkg-step",
            "Install Tooling",
            "medium",
            [new CommandNode("powershell", "winget install Git.Git --version 2.45.1", false)],
            [],
            []);
        var ast = CreateAst(steps: [step]);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }
}

