namespace RunbookForge.Modules.RunbookEngine.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RunbookForge.BuildingBlocks.Persistence.Constants;
using RunbookForge.Modules.RunbookEngine.Domain.Entities;

public sealed class RunbookWorkflowConfiguration : IEntityTypeConfiguration<RunbookWorkflow>
{
    public void Configure(EntityTypeBuilder<RunbookWorkflow> builder)
    {
        builder.ToTable("workflows", SchemaConstants.Runbooks);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.DocumentId).IsRequired();
        builder.Property(x => x.VersionNumber).IsRequired();

        builder.Property(x => x.NodesJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.EdgesJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.CompiledAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => new { x.DocumentId, x.VersionNumber })
            .IsUnique()
            .HasDatabaseName("uq_workflows_document_version");

        builder.HasIndex(x => x.NodesJson)
            .HasMethod("gin")
            .HasDatabaseName("gin_workflows_nodes");

        builder.HasIndex(x => x.EdgesJson)
            .HasMethod("gin")
            .HasDatabaseName("gin_workflows_edges");
    }
}

