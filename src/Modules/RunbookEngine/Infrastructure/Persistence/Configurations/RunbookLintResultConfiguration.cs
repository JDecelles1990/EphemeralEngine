namespace RunbookForge.Modules.RunbookEngine.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RunbookForge.BuildingBlocks.Persistence.Constants;
using RunbookForge.Modules.RunbookEngine.Domain.Entities;

public sealed class RunbookLintResultConfiguration : IEntityTypeConfiguration<RunbookLintResult>
{
    public void Configure(EntityTypeBuilder<RunbookLintResult> builder)
    {
        builder.ToTable("lint_results", SchemaConstants.Runbooks);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.DocumentId).IsRequired();
        builder.Property(x => x.VersionNumber).IsRequired();

        builder.Property(x => x.RuleId)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Severity)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.LineNumber).IsRequired();

        builder.Property(x => x.Message)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.IsValid).IsRequired();
        builder.Property(x => x.ErrorCount).IsRequired();
        builder.Property(x => x.WarningCount).IsRequired();

        builder.Property(x => x.Diagnostics)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.EvaluatedAt).IsRequired();

        builder.HasIndex(x => x.DocumentId)
            .HasDatabaseName("ix_lint_results_document_id");

        builder.HasIndex(x => new { x.DocumentId, x.Severity })
            .HasDatabaseName("ix_lint_results_document_severity");
    }
}

