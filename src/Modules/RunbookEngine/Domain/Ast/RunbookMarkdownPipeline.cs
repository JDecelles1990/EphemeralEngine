namespace RunbookForge.Modules.RunbookEngine.Domain.Ast;

using Markdig;
using RunbookForge.Modules.RunbookEngine.Domain.Ast.Parsers;

public static class RunbookMarkdownPipeline
{
    private static readonly MarkdownPipeline DefaultPipeline = new MarkdownPipelineBuilder()
        .UseYamlFrontMatter()
        .UseRunbookSyntax()
        .UseGenericAttributes()
        .Build();

    public static MarkdownPipeline Instance => DefaultPipeline;

    public static MarkdownPipeline Create() => DefaultPipeline;
}

