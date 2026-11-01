namespace RunbookForge.Modules.RunbookEngine.Infrastructure.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RunbookForge.BuildingBlocks.Persistence.Constants;
using RunbookForge.Modules.RunbookEngine.Application.Contracts;
using RunbookForge.Modules.RunbookEngine.Domain.Rules;
using RunbookForge.Modules.RunbookEngine.Domain.Rules.Metadata;
using RunbookForge.Modules.RunbookEngine.Domain.Rules.Structure;
using RunbookForge.Modules.RunbookEngine.Infrastructure.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRunbookEngineInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);

        var connectionString = configuration?.GetConnectionString("PostgresDatabase")
            ?? "Host=localhost;Database=runbookforge;Username=postgres;Password=postgres";

        services.AddDbContext<RunbookDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", SchemaConstants.Runbooks);
                npgsqlOptions.MigrationsAssembly(typeof(RunbookDbContext).Assembly.FullName);
            });
        });

        services.AddScoped<IRunbookDbContext>(sp => sp.GetRequiredService<RunbookDbContext>());

        services.AddRunbookLinter();

        return services;
    }

    public static IServiceCollection AddRunbookLinter(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Metadata rules (RF001–RF010)
        services.AddSingleton<IRunbookRule, RF001_MissingFrontmatterRule>();
        services.AddSingleton<IRunbookRule, RF002_InvalidDocumentIdRule>();
        services.AddSingleton<IRunbookRule, RF003_MissingTitleRule>();
        services.AddSingleton<IRunbookRule, RF004_InvalidCategoryRule>();
        services.AddSingleton<IRunbookRule, RF005_InvalidRiskLevelRule>();
        services.AddSingleton<IRunbookRule, RF006_MissingSupportTierRule>();
        services.AddSingleton<IRunbookRule, RF007_VerboseTitleRule>();
        services.AddSingleton<IRunbookRule, RF008_MissingTagsRule>();
        services.AddSingleton<IRunbookRule, RF009_MissingTargetAudienceRule>();
        services.AddSingleton<IRunbookRule, RF010_MissingAuthorOrReviewerRule>();

        // Structure rules (RF011–RF015)
        services.AddSingleton<IRunbookRule, RF011_MissingSymptomsSectionRule>();
        services.AddSingleton<IRunbookRule, RF012_MissingDiagnosticsSectionRule>();
        services.AddSingleton<IRunbookRule, RF013_MissingRemediationSectionRule>();
        services.AddSingleton<IRunbookRule, RF014_MissingValidationSectionRule>();
        services.AddSingleton<IRunbookRule, RF015_MissingEscalationSectionRule>();

        services.AddSingleton<RunbookLinterEngine>();

        return services;
    }
}
