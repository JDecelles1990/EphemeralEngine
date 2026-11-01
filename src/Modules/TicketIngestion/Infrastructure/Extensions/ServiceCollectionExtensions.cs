namespace RunbookForge.Modules.TicketIngestion.Infrastructure.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RunbookForge.Modules.TicketIngestion.Infrastructure.Persistence;
using RunbookForge.Modules.TicketIngestion.Infrastructure.Persistence.Interceptors;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the read-only F7Hub adapter. Whatever <paramref name="sqliteConnectionString"/> specifies, the
    /// effective connection is forced to <c>Mode=ReadOnly;Cache=Shared</c> (invariant INV-001).
    /// </summary>
    public static IServiceCollection AddF7HubReadOnlyPersistence(
        this IServiceCollection services,
        string sqliteConnectionString)
    {
        ArgumentNullException.ThrowIfNull(services);

        var enforcedConnectionString = F7HubConnectionPolicy.Enforce(sqliteConnectionString);

        services.AddSingleton<F7HubReadOnlyCommandInterceptor>();
        services.AddDbContext<F7HubReadOnlyDbContext>((serviceProvider, options) =>
        {
            var interceptor = serviceProvider.GetRequiredService<F7HubReadOnlyCommandInterceptor>();

            options
                .UseSqlite(enforcedConnectionString)
                .AddInterceptors(interceptor)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        return services;
    }
}

