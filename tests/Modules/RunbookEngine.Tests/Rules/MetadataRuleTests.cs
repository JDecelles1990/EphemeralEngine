namespace RunbookForge.Modules.RunbookEngine.Tests.Rules;

using FluentAssertions;
using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;
using RunbookForge.Modules.RunbookEngine.Domain.Rules.Metadata;
using Xunit;

public class MetadataRuleTests
{
    private static RunbookMetadata CreateValidMetadata(
        string id = "NET-101",
        string title = "Core Switch Troubleshooting",
        string category = "networking",
        string risk = "medium",
        string level = "L2",
        IReadOnlyList<string>? tags = null,
        string author = "Network Ops",
        string? lastReviewed = "2026-11-01",
        string? audience = "Tier 2 Engineers",
        bool hasFrontmatter = true) =>
        new(
            Id: id,
            Title: title,
            Slug: "core-switch-troubleshooting",
            Category: category,
            Level: level,
            Risk: risk,
            Author: author,
            Version: "1.0.0",
            SchemaVersion: "rf-v1",
            Tags: tags ?? ["cisco", "switch", "networking"],
            LastReviewed: lastReviewed,
            Audience: audience,
            HasFrontmatter: hasFrontmatter);

    private static RunbookAstDocument CreateAstWithMetadata(RunbookMetadata metadata) =>
        new(
            Metadata: metadata,
            Steps: [new StepNode("step-1", "Diagnostics Step", "low", [], [], [])],
            EscalationTriggers: ["Issue unresolved after 15m"],
            RawMarkdown: "# Test",
            Symptoms: ["Packet loss"],
            RawSectionsFound: ["Symptoms", "Diagnostics", "Remediation", "Validation", "Escalation"]);

    [Fact]
    public async Task RF001_ShouldEmitError_WhenFrontmatterIsMissingOrEmpty()
    {
        var rule = new RF001_MissingFrontmatterRule();
        var metadata = CreateValidMetadata(hasFrontmatter: false);
        var ast = CreateAstWithMetadata(metadata);

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF001");
        diagnostics[0].Severity.Should().Be(LintSeverity.Error);
        diagnostics[0].Message.Should().Contain("missing required YAML frontmatter");
    }

    [Fact]
    public async Task RF001_ShouldPass_WhenFrontmatterIsPresent()
    {
        var rule = new RF001_MissingFrontmatterRule();
        var ast = CreateAstWithMetadata(CreateValidMetadata());

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData("invalid_id")]
    [InlineData("123-abc")]
    [InlineData("N-101")]
    [InlineData("TOOLONGNAME-101")]
    [InlineData("NET-12")]
    [InlineData("NET-123456")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RF002_ShouldEmitError_WhenDocumentIdDoesNotMatchPattern(string invalidId)
    {
        var rule = new RF002_InvalidDocumentIdRule();
        var ast = CreateAstWithMetadata(CreateValidMetadata(id: invalidId));

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF002");
        diagnostics[0].Severity.Should().Be(LintSeverity.Error);
        diagnostics[0].Message.Should().Contain("invalid");
    }

    [Theory]
    [InlineData("NET-101")]
    [InlineData("SEC-50001")]
    [InlineData("INFRA-9999")]
    [InlineData("M-101", false)]
    public async Task RF002_ShouldPass_WhenDocumentIdMatchesPattern(string validId, bool shouldPass = true)
    {
        var rule = new RF002_InvalidDocumentIdRule();
        var ast = CreateAstWithMetadata(CreateValidMetadata(id: validId));

        var diagnostics = await rule.EvaluateAsync(ast);

        if (shouldPass)
        {
            diagnostics.Should().BeEmpty();
        }
        else
        {
            diagnostics.Should().NotBeEmpty();
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Fix")]
    [InlineData("1234")]
    public async Task RF003_ShouldEmitError_WhenTitleIsTooShortOrEmpty(string invalidTitle)
    {
        var rule = new RF003_MissingTitleRule();
        var ast = CreateAstWithMetadata(CreateValidMetadata(title: invalidTitle));

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF003");
        diagnostics[0].Severity.Should().Be(LintSeverity.Error);
    }

    [Fact]
    public async Task RF003_ShouldPass_WhenTitleIsAtLeastFiveChars()
    {
        var rule = new RF003_MissingTitleRule();
        var ast = CreateAstWithMetadata(CreateValidMetadata(title: "Reset Router"));

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData("gaming")]
    [InlineData("hardware")]
    [InlineData("random")]
    [InlineData("")]
    public async Task RF004_ShouldEmitWarning_WhenCategoryIsInvalid(string invalidCategory)
    {
        var rule = new RF004_InvalidCategoryRule();
        var ast = CreateAstWithMetadata(CreateValidMetadata(category: invalidCategory));

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF004");
        diagnostics[0].Severity.Should().Be(LintSeverity.Warning);
    }

    [Theory]
    [InlineData("networking")]
    [InlineData("m365")]
    [InlineData("identity")]
    [InlineData("endpoint")]
    [InlineData("infrastructure")]
    [InlineData("security")]
    [InlineData("NETWORKING")]
    public async Task RF004_ShouldPass_WhenCategoryIsValid(string validCategory)
    {
        var rule = new RF004_InvalidCategoryRule();
        var ast = CreateAstWithMetadata(CreateValidMetadata(category: validCategory));

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData("extreme")]
    [InlineData("severe")]
    [InlineData("none")]
    [InlineData("")]
    public async Task RF005_ShouldEmitError_WhenRiskLevelIsInvalid(string invalidRisk)
    {
        var rule = new RF005_InvalidRiskLevelRule();
        var ast = CreateAstWithMetadata(CreateValidMetadata(risk: invalidRisk));

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF005");
        diagnostics[0].Severity.Should().Be(LintSeverity.Error);
    }

    [Theory]
    [InlineData("low")]
    [InlineData("medium")]
    [InlineData("high")]
    [InlineData("critical")]
    [InlineData("CRITICAL")]
    public async Task RF005_ShouldPass_WhenRiskLevelIsValid(string validRisk)
    {
        var rule = new RF005_InvalidRiskLevelRule();
        var ast = CreateAstWithMetadata(CreateValidMetadata(risk: validRisk));

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData("L0")]
    [InlineData("L4")]
    [InlineData("Tier1")]
    [InlineData("")]
    public async Task RF006_ShouldEmitWarning_WhenSupportTierIsInvalid(string invalidLevel)
    {
        var rule = new RF006_MissingSupportTierRule();
        var ast = CreateAstWithMetadata(CreateValidMetadata(level: invalidLevel));

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF006");
        diagnostics[0].Severity.Should().Be(LintSeverity.Warning);
    }

    [Theory]
    [InlineData("L1")]
    [InlineData("L2")]
    [InlineData("L3")]
    [InlineData("l1")]
    public async Task RF006_ShouldPass_WhenSupportTierIsValid(string validLevel)
    {
        var rule = new RF006_MissingSupportTierRule();
        var ast = CreateAstWithMetadata(CreateValidMetadata(level: validLevel));

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task RF007_ShouldEmitInfo_WhenTitleExceedsEightyCharacters()
    {
        var rule = new RF007_VerboseTitleRule();
        var longTitle = new string('A', 81);
        var ast = CreateAstWithMetadata(CreateValidMetadata(title: longTitle));

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF007");
        diagnostics[0].Severity.Should().Be(LintSeverity.Info);
        diagnostics[0].Message.Should().Contain("exceeds 80 characters");
    }

    [Fact]
    public async Task RF007_ShouldPass_WhenTitleIsEightyCharactersOrFewer()
    {
        var rule = new RF007_VerboseTitleRule();
        var title = new string('A', 80);
        var ast = CreateAstWithMetadata(CreateValidMetadata(title: title));

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task RF008_ShouldEmitInfo_WhenTagsCollectionIsEmpty()
    {
        var rule = new RF008_MissingTagsRule();
        var ast = CreateAstWithMetadata(CreateValidMetadata(tags: []));

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF008");
        diagnostics[0].Severity.Should().Be(LintSeverity.Info);
    }

    [Fact]
    public async Task RF008_ShouldPass_WhenTagsArePresent()
    {
        var rule = new RF008_MissingTagsRule();
        var ast = CreateAstWithMetadata(CreateValidMetadata(tags: ["network", "switch"]));

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task RF009_ShouldEmitWarning_WhenNeitherLevelNorAudienceSpecified()
    {
        var rule = new RF009_MissingTargetAudienceRule();
        var ast = CreateAstWithMetadata(CreateValidMetadata(level: "", audience: null));

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF009");
        diagnostics[0].Severity.Should().Be(LintSeverity.Warning);
    }

    [Fact]
    public async Task RF009_ShouldPass_WhenLevelOrAudienceSpecified()
    {
        var rule = new RF009_MissingTargetAudienceRule();

        var astWithLevel = CreateAstWithMetadata(CreateValidMetadata(level: "L2", audience: null));
        (await rule.EvaluateAsync(astWithLevel)).Should().BeEmpty();

        var astWithAudience = CreateAstWithMetadata(CreateValidMetadata(level: "", audience: "DevOps Team"));
        (await rule.EvaluateAsync(astWithAudience)).Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "2026-11-01")]
    [InlineData("Unknown", "2026-11-01")]
    [InlineData("Jane Doe", null)]
    [InlineData("Jane Doe", "")]
    public async Task RF010_ShouldEmitInfo_WhenAuthorOrReviewerIsMissing(string author, string? lastReviewed)
    {
        var rule = new RF010_MissingAuthorOrReviewerRule();
        var ast = CreateAstWithMetadata(CreateValidMetadata(author: author, lastReviewed: lastReviewed));

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().ContainSingle();
        diagnostics[0].RuleId.Should().Be("RF010");
        diagnostics[0].Severity.Should().Be(LintSeverity.Info);
    }

    [Fact]
    public async Task RF010_ShouldPass_WhenBothAuthorAndReviewerArePresent()
    {
        var rule = new RF010_MissingAuthorOrReviewerRule();
        var ast = CreateAstWithMetadata(CreateValidMetadata(author: "Jane Doe", lastReviewed: "2026-11-01"));

        var diagnostics = await rule.EvaluateAsync(ast);

        diagnostics.Should().BeEmpty();
    }
}
