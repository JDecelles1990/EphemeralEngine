namespace RunbookForge.Modules.RunbookEngine.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RunbookForge.BuildingBlocks.Persistence.Constants;
using RunbookForge.Modules.RunbookEngine.Domain.Entities;

public sealed class RunbookDocumentConfiguration : IEntityTypeConfiguration<RunbookDocument>
{
    public void Configure(EntityTypeBuilder<RunbookDocument> builder)
    {
        builder.ToTable("documents", SchemaConstants.Runbooks);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Title)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Description)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.RiskLevel)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Category)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.RawMarkdown)
            .IsRequired();

        builder.Property(x => x.AstJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.Tags)
            .HasColumnType("text[]")
            .IsRequired();

        builder.Property(x => x.CurrentVersionNumber)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.Property(x => x.VersionToken)
            .IsRowVersion()
            .HasColumnName("xmin");

        builder.HasIndex(x => x.Slug)
            .IsUnique()
            .HasDatabaseName("ix_documents_slug");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("ix_documents_status");

        builder.HasIndex(x => x.Tags)
            .HasMethod("gin")
            .HasDatabaseName("gin_documents_tags");

        builder.HasIndex(x => x.AstJson)
            .HasMethod("gin")
            .HasDatabaseName("gin_documents_ast_json");

        builder.HasMany(x => x.Versions)
            .WithOne(x => x.Document)
            .HasForeignKey(x => x.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.LintResults)
            .WithOne(x => x.Document)
            .HasForeignKey(x => x.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Workflow)
            .WithOne(x => x.Document)
            .HasForeignKey<RunbookWorkflow>(x => x.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

