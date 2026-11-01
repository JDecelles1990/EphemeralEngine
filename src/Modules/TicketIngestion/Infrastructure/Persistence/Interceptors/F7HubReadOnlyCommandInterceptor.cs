namespace RunbookForge.Modules.TicketIngestion.Infrastructure.Persistence.Interceptors;

using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
/// Invariant INV-001 (layer 2): inspects every command EF Core sends to the F7Hub SQLite database and throws
/// <see cref="InvalidOperationException"/> for anything that could mutate it. This closes the
/// <c>Database.ExecuteSqlRaw*</c> / <c>FromSqlRaw</c> bypass of the <c>SaveChanges</c> blockade.
/// <para>
/// Layer 1 is the <c>Mode=ReadOnly</c> connection itself, so even raw ADO.NET commands that skip EF interceptors
/// are rejected by the SQLite engine. This interceptor fails fast with a clear message before SQLite is reached.
/// </para>
/// </summary>
internal sealed partial class F7HubReadOnlyCommandInterceptor : DbCommandInterceptor
{
    public const string MutationForbiddenMessage = "Mutating operations are forbidden on the F7Hub SQLite database.";
    public const string UnanalyzableMessage =
        "F7Hub command text could not be safely analysed (unterminated quote, bracket identifier). Command rejected.";

    // Evaluated against SQL text with string literals, quoted identifiers, and comments already removed,
    // so 'DELETE me' or a column named "update" cannot trigger a false positive.
    [GeneratedRegex(
        @"\b(?:INSERT|UPDATE|DELETE|DROP|ALTER|CREATE|VACUUM|REINDEX|ANALYZE|ATTACH|DETACH)\b"
        + @"|\bREPLACE\s+INTO\b"
        + @"|\bPRAGMA\s+writable_schema\b"
        + @"|\bPRAGMA\b[^;]*=",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex MutationPattern();

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        Guard(command);
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        Guard(command);
        return ValueTask.FromResult(result);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result)
    {
        Guard(command);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Guard(command);
        return ValueTask.FromResult(result);
    }

    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result)
    {
        Guard(command);
        return result;
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        Guard(command);
        return ValueTask.FromResult(result);
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="commandText"/> contains a mutating statement. Text that cannot be
    /// analysed reliably is treated as mutating (fail closed).
    /// </summary>
    public static bool IsMutating(string? commandText)
    {
        if (string.IsNullOrWhiteSpace(commandText))
        {
            return false;
        }

        return !TryStripLiteralsAndComments(commandText, out var sanitized) || MutationPattern().IsMatch(sanitized);
    }

    private static void Guard(DbCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var text = command.CommandText;
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        if (!TryStripLiteralsAndComments(text, out var sanitized))
        {
            throw new InvalidOperationException(UnanalyzableMessage);
        }

        if (MutationPattern().IsMatch(sanitized))
        {
            throw new InvalidOperationException(MutationForbiddenMessage);
        }
    }

    /// <summary>
    /// Left-to-right scan mirroring SQLite tokenisation: removes '...' literals, "..." and `...` identifiers,
    /// [...] identifiers, <c>--</c> line comments and <c>/* */</c> block comments. Returns false when a quote or
    /// bracket is never closed.
    /// </summary>
    private static bool TryStripLiteralsAndComments(string sql, out string sanitized)
    {
        var builder = new StringBuilder(sql.Length);
        var i = 0;

        while (i < sql.Length)
        {
            var c = sql[i];

            switch (c)
            {
                case '\'' or '"' or '`':
                    i = SkipQuoted(sql, i, c);
                    if (i < 0)
                    {
                        sanitized = string.Empty;
                        return false;
                    }

                    builder.Append(' ');
                    break;

                case '[':
                    var close = sql.IndexOf(']', i + 1);
                    if (close < 0)
                    {
                        sanitized = string.Empty;
                        return false;
                    }

                    i = close + 1;
                    builder.Append(' ');
                    break;

                case '-' when i + 1 < sql.Length && sql[i + 1] == '-':
                    var eol = sql.IndexOf('\n', i + 2);
                    i = eol < 0 ? sql.Length : eol + 1;
                    builder.Append(' ');
                    break;

                case '/' when i + 1 < sql.Length && sql[i + 1] == '*':
                    // SQLite treats an unterminated block comment as running to end of input.
                    var end = sql.IndexOf("*/", i + 2, StringComparison.Ordinal);
                    i = end < 0 ? sql.Length : end + 2;
                    builder.Append(' ');
                    break;

                default:
                    builder.Append(c);
                    i++;
                    break;
            }
        }

        sanitized = builder.ToString();
        return true;
    }

    /// <summary>Returns the index after the closing quote, or -1 when unterminated. Doubled quotes escape.</summary>
    private static int SkipQuoted(string sql, int openIndex, char quote)
    {
        var j = openIndex + 1;
        while (j < sql.Length)
        {
            if (sql[j] == quote)
            {
                if (j + 1 < sql.Length && sql[j + 1] == quote)
                {
                    j += 2;
                    continue;
                }

                return j + 1;
            }

            j++;
        }

        return -1;
    }
}

