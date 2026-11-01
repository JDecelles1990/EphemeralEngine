namespace RunbookForge.Modules.RunbookEngine.Domain.Ast.Blocks;

using Markdig.Parsers;
using Markdig.Syntax;

public class StepBlock : ContainerBlock
{
    public StepBlock(BlockParser parser) : base(parser)
    {
    }

    public string StepId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Risk { get; set; } = "low";
    public Dictionary<string, string> Attributes { get; } = new(StringComparer.OrdinalIgnoreCase);
}

public class AssertionBlock : ContainerBlock
{
    public AssertionBlock(BlockParser parser) : base(parser)
    {
    }

    public string Target { get; set; } = "stdout";
    public string Comparator { get; set; } = "equals";
    public string ExpectedValue { get; set; } = string.Empty;
    public Dictionary<string, string> Attributes { get; } = new(StringComparer.OrdinalIgnoreCase);
}

public class QuestionBlock : ContainerBlock
{
    public QuestionBlock(BlockParser parser) : base(parser)
    {
    }

    public string Prompt { get; set; } = string.Empty;
    public List<string> Options { get; } = [];
    public Dictionary<string, string> Attributes { get; } = new(StringComparer.OrdinalIgnoreCase);
}

