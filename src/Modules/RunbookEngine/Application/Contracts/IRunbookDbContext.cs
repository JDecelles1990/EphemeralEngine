namespace RunbookForge.Modules.RunbookEngine.Application.Contracts;

using Microsoft.EntityFrameworkCore;
using RunbookForge.Modules.RunbookEngine.Domain.Entities;

public interface IRunbookDbContext
{
    DbSet<RunbookDocument> Documents { get; }
    DbSet<RunbookVersion> Versions { get; }
    DbSet<RunbookLintResult> LintResults { get; }
    DbSet<RunbookWorkflow> Workflows { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

