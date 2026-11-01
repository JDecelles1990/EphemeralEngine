namespace RunbookForge.Modules.RunbookEngine.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RunbookForge.BuildingBlocks.Persistence.Constants;
using RunbookForge.Modules.RunbookEngine.Domain.Entities;

public sealed class RunbookVersionConfiguration : IEntityTypeConfiguration<RunbookVersion>
{
    public void Configure(EntityTypeBuilder<RunbookVersion> builder)
    {
        builder.ToTable("versions", SchemaConstants.Runbooks);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.DocumentId).IsRequired();
        builder.Property(x => x.VersionNumber).IsRequired();
        builder.Property(x => x.RawMarkdown).IsRequired();

        builder.Property(x => x.ParsedAst)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.ChangeSummary)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.PublishedByUserId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PublishedAt).IsRequired();

        builder.HasIndex(x => new { x.DocumentId, x.VersionNumber })
            .IsUnique()
            .HasDatabaseName("uq_versions_document_version");

        builder.HasIndex(x => x.DocumentId)
            .HasDatabaseName("ix_versions_document_id");

        builder.HasIndex(x => x.ParsedAst)
            .HasMethod("gin")
            .HasDatabaseName("gin_versions_parsed_ast");
    }
}

