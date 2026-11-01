namespace RunbookForge.Modules.RunbookEngine.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RunbookForge.BuildingBlocks.Persistence.Constants;

public sealed class RunbookDbContextFactory : IDesignTimeDbContextFactory<RunbookDbContext>
{
    public RunbookDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RunbookDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=runbookforge;Username=postgres;Password=postgres",
            npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", SchemaConstants.Runbooks);
                npgsqlOptions.MigrationsAssembly(typeof(RunbookDbContext).Assembly.FullName);
            });

        return new RunbookDbContext(optionsBuilder.Options);
    }
}

