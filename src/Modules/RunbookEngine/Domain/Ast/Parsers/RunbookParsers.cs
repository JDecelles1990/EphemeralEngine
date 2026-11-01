namespace RunbookForge.Modules.RunbookEngine.Domain.Ast.Parsers;

using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.CustomContainers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Syntax;

public static partial class AttributeParser
{
    [GeneratedRegex(@"([a-zA-Z0-9_-]+)\s*=\s*(?:""([^""]*)""|'([^']*)'|(\S+))", RegexOptions.Compiled)]
    private static partial Regex AttributeRegex();

    public static Dictionary<string, string> Parse(string input)
    {
        var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(input)) return attributes;

        var matches = AttributeRegex().Matches(input);
        foreach (Match match in matches)
        {
            var key = match.Groups[1].Value;
            var value = match.Groups[2].Success ? match.Groups[2].Value :
                        match.Groups[3].Success ? match.Groups[3].Value :
                        match.Groups[4].Value;

            attributes[key] = value;
        }

        return attributes;
    }
}

public class RunbookContainerParser : FencedBlockParserBase<CustomContainer>
{
    public RunbookContainerParser()
    {
        OpeningCharacters = [':'];
        InfoPrefix = "::";
    }

    protected override CustomContainer CreateFencedBlock(BlockProcessor processor)
    {
        return new CustomContainer(this);
    }

    public override BlockState TryContinue(BlockProcessor processor, Block block)
    {
        if (processor.CurrentContainer != block)
        {
            return BlockState.Continue;
        }

        return base.TryContinue(processor, block);
    }
}

public sealed class StepBlockParser : RunbookContainerParser
{
}

public sealed class AssertionBlockParser : RunbookContainerParser
{
}

public sealed class QuestionBlockParser : RunbookContainerParser
{
}

public sealed class RunbookSyntaxExtension : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        pipeline.BlockParsers.AddIfNotAlready(new RunbookContainerParser());
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
    }
}

public static class RunbookPipelineExtensions
{
    public static MarkdownPipelineBuilder UseRunbookSyntax(this MarkdownPipelineBuilder pipeline)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        pipeline.Extensions.AddIfNotAlready<RunbookSyntaxExtension>();
        return pipeline;
    }
}
