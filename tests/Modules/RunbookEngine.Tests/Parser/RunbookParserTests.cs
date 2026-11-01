namespace RunbookForge.Modules.RunbookEngine.Tests.Parser;

using FluentAssertions;
using RunbookForge.BuildingBlocks.Common.Results;
using RunbookForge.Modules.RunbookEngine.Application.Features.ParseRunbook;
using Xunit;

public class RunbookParserTests
{
    private readonly ParseRunbookMarkdownHandler _handler = new();

    [Fact]
    public async Task Handle_ShouldExtractFrontmatterMetadata_WhenYamlPresent()
    {
        // Arrange
        var markdown = """
        ---
        title: "Database Connection Pool Recovery"
        slug: "db-connection-pool-recovery"
        category: "Database"
        risk: "high"
        level: "L2"
        author: "DevOps Core"
        version: "1.2.0"
        schema_version: "rf-v1"
        tags: ["postgres", "p1-incident", "infrastructure"]
        ---

        # Overview
        Remediation procedure when connection pool reaches 100% saturation.
        """;

        var query = new ParseRunbookMarkdownQuery(markdown);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Error.Message.Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        var doc = result.Value;
        doc.Metadata.Title.Should().Be("Database Connection Pool Recovery");
        doc.Metadata.Slug.Should().Be("db-connection-pool-recovery");
        doc.Metadata.Category.Should().Be("Database");
        doc.Metadata.Risk.Should().Be("high");
        doc.Metadata.Level.Should().Be("L2");
        doc.Metadata.Author.Should().Be("DevOps Core");
        doc.Metadata.Version.Should().Be("1.2.0");
        doc.Metadata.SchemaVersion.Should().Be("rf-v1");
        doc.Metadata.Tags.Should().Contain("postgres").And.Contain("p1-incident");
    }

    [Fact]
    public async Task Handle_ShouldParseStepWithFencedCodeBlock_WhenValid()
    {
        // Arrange
        var markdown = """
        :::step id="step-1" title="Inspect Active Connections" risk="low"
        Check the current connection count against max pool limits.

        ```bash {lang="bash" timeout="15s" mode="readonly"}
        psql -h $PGHOST -U $PGUSER -c "SELECT count(*) FROM pg_stat_activity;"
        ```
        :::
        """;

        var query = new ParseRunbookMarkdownQuery(markdown);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Error.Message.Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        result.Value.Steps.Should().HaveCount(1);

        var step = result.Value.Steps[0];
        step.Id.Should().Be("step-1");
        step.Title.Should().Be("Inspect Active Connections");
        step.Risk.Should().Be("low");
        step.Description.Should().Contain("Check the current connection count");

        step.Commands.Should().HaveCount(1);
        var cmd = step.Commands[0];
        cmd.Language.Should().Be("bash");
        cmd.Timeout.Should().Be("15s");
        cmd.Mode.Should().Be("readonly");
        cmd.Script.Should().Contain("SELECT count(*)");
    }

    [Fact]
    public async Task Handle_ShouldParseAssertionsAndQuestions_WhenNestedInStep()
    {
        // Arrange
        var markdown = """
        :::step id="step-2" title="Verify Service Health"
        Run health check command to verify database reachability.

        ```powershell
        Test-NetConnection -ComputerName "db.internal" -Port 5432
        ```

        :::assert target="stdout" condition="matches" value="TcpTestSucceeded : True"
        Ensure database port is reachable.
        :::

        :::question prompt="Is the database responsive?" options="Yes,No,Degraded"
        :::
        :::
        """;

        var query = new ParseRunbookMarkdownQuery(markdown);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Error.Message.Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        result.Value.Steps.Should().HaveCount(1);

        var step = result.Value.Steps[0];
        step.Assertions.Should().HaveCount(1);
        var assert = step.Assertions[0];
        assert.Target.Should().Be("stdout");
        assert.Comparator.Should().Be("matches");
        assert.ExpectedValue.Should().Be("TcpTestSucceeded : True");
        assert.Message.Should().Contain("Ensure database port is reachable.");

        step.Questions.Should().HaveCount(1);
        var question = step.Questions[0];
        question.Prompt.Should().Be("Is the database responsive?");
        question.Options.Should().ContainInOrder("Yes", "No", "Degraded");
    }

    [Fact]
    public async Task Handle_ShouldExtractEscalationTriggers_WhenEscalationSectionPresent()
    {
        // Arrange
        var markdown = """
        # Remediation
        Standard recovery steps.

        # Escalation
        - MTTR exceeds 15 minutes without recovery
        - Data corruption suspected
        - Core database replica is partitioned
        """;

        var query = new ParseRunbookMarkdownQuery(markdown);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Error.Message.Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        result.Value.EscalationTriggers.Should().HaveCount(3);
        result.Value.EscalationTriggers[0].Should().Contain("MTTR exceeds 15 minutes");
        result.Value.EscalationTriggers[1].Should().Contain("Data corruption suspected");
    }

    [Fact]
    public async Task Handle_ShouldReturnDefaultMetadata_WhenFrontmatterOmitted()
    {
        // Arrange
        var markdown = """
        :::step id="S1" title="Fallback Step"
        Plain step without frontmatter.
        :::
        """;

        var query = new ParseRunbookMarkdownQuery(markdown);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Error.Message.Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        result.Value.Metadata.Title.Should().Be("Untitled Runbook");
        result.Value.Metadata.Slug.Should().Be("untitled-runbook");
        result.Value.Steps.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenContentIsEmpty()
    {
        // Arrange
        var query = new ParseRunbookMarkdownQuery("   ");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Code.Should().Be("ParseRunbook.EmptyContent");
    }

    [Fact]
    public async Task Handle_ShouldParseMultipleStepsInSequence_WhenMultipleStepsDefined()
    {
        // Arrange
        var markdown = """
        :::step id="S1" title="Step One" risk="low"
        First step description.
        :::

        :::step id="S2" title="Step Two" risk="high"
        Second step description.
        :::
        """;

        var query = new ParseRunbookMarkdownQuery(markdown);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Error.Message.Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        result.Value.Steps.Should().HaveCount(2);
        result.Value.Steps[0].Id.Should().Be("S1");
        result.Value.Steps[0].Risk.Should().Be("low");
        result.Value.Steps[1].Id.Should().Be("S2");
        result.Value.Steps[1].Risk.Should().Be("high");
    }
}
