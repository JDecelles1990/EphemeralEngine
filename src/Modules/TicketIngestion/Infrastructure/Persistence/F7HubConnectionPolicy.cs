namespace RunbookForge.Modules.TicketIngestion.Infrastructure.Persistence;

using Microsoft.Data.Sqlite;

/// <summary>
/// Invariant INV-001 (layer 1): the F7Hub connection string is always
/// <c>Data Source=&lt;path&gt;;Mode=ReadOnly;Cache=Shared</c>, regardless of what the caller supplied.
/// </summary>
internal static class F7HubConnectionPolicy
{
    public static string Enforce(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var builder = new SqliteConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(builder.DataSource))
        {
            throw new ArgumentException("The F7Hub connection string must specify a Data Source.", nameof(connectionString));
        }

        builder.Mode = SqliteOpenMode.ReadOnly;
        builder.Cache = SqliteCacheMode.Shared;
        return builder.ToString();
    }

    public static bool IsEnforced(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return false;
        }

        try
        {
            var builder = new SqliteConnectionStringBuilder(connectionString);
            return builder.Mode == SqliteOpenMode.ReadOnly && builder.Cache == SqliteCacheMode.Shared;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}

