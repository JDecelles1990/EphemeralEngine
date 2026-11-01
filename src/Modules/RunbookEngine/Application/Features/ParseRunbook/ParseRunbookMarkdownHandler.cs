namespace RunbookForge.Modules.RunbookEngine.Application.Features.ParseRunbook;

using Markdig;
using MediatR;
using RunbookForge.BuildingBlocks.Common.Results;
using RunbookForge.Modules.RunbookEngine.Domain.Ast;
using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;

public sealed class ParseRunbookMarkdownHandler
    : IRequestHandler<ParseRunbookMarkdownQuery, Result<RunbookAstDocument>>
{
    public Task<Result<RunbookAstDocument>> Handle(
        ParseRunbookMarkdownQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.RawMarkdown))
        {
            return Task.FromResult(Result<RunbookAstDocument>.Failure(
                Error.Validation("ParseRunbook.EmptyContent", "Raw markdown content cannot be empty.")));
        }

        try
        {
            var markdownDoc = Markdown.Parse(request.RawMarkdown, RunbookMarkdownPipeline.Instance);
            var ast = RunbookAstVisitor.Visit(markdownDoc, request.RawMarkdown);

            return Task.FromResult(Result<RunbookAstDocument>.Success(ast));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<RunbookAstDocument>.Failure(
                Error.Failure("ParseRunbook.SyntaxError", $"Failed to parse markdown AST: {ex}")));
        }
    }
}
