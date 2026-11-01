namespace RunbookForge.Modules.RunbookEngine.Application.Features.ParseRunbook;

using RunbookForge.BuildingBlocks.Common.Contracts;
using RunbookForge.BuildingBlocks.Common.Results;
using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;

public sealed record ParseRunbookMarkdownQuery(string RawMarkdown) : IQuery<Result<RunbookAstDocument>>;
