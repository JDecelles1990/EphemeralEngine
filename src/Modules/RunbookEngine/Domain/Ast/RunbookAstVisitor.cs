namespace RunbookForge.Modules.RunbookEngine.Domain.Ast;

using System.Text;
using Markdig.Extensions.CustomContainers;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using RunbookForge.Modules.RunbookEngine.Domain.Ast.Blocks;
using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Ast.Parsers;

public static class RunbookAstVisitor
{
    public static RunbookAstDocument Visit(MarkdownDocument document, string rawMarkdown)
    {
        ArgumentNullException.ThrowIfNull(document);

        var metadata = ExtractMetadata(document, rawMarkdown);
        var steps = new List<StepNode>();
        var escalationTriggers = new List<string>();
        var symptoms = new List<string>();
        var rawSectionsFound = new List<string>();

        var inEscalationSection = false;
        var inSymptomsSection = false;

        foreach (var block in document)
        {
            if (block is YamlFrontMatterBlock) continue;

            if (block is HeadingBlock heading)
            {
                var headingText = ExtractHeadingText(heading, rawMarkdown);
                if (!string.IsNullOrWhiteSpace(headingText))
                {
                    rawSectionsFound.Add(headingText);
                }
                inEscalationSection = headingText.Contains("Escalation", StringComparison.OrdinalIgnoreCase);
                inSymptomsSection = headingText.Contains("Symptoms", StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (inEscalationSection && block is ListBlock listBlock)
            {
                escalationTriggers.AddRange(ExtractListItems(listBlock, rawMarkdown));
                continue;
            }

            if (inSymptomsSection)
            {
                if (block is ListBlock symptomListBlock)
                {
                    symptoms.AddRange(ExtractListItems(symptomListBlock, rawMarkdown));
                    continue;
                }
                else if (block is ParagraphBlock paragraphBlock)
                {
                    var text = ExtractParagraphText(paragraphBlock, rawMarkdown);
                    if (!string.IsNullOrWhiteSpace(text)) symptoms.Add(text);
                    continue;
                }
            }

            if (block is StepBlock stepBlock)
            {
                steps.Add(VisitStepBlock(stepBlock, rawMarkdown));
            }
            else if (block is CustomContainer container &&
                     container.Info?.Equals("step", StringComparison.OrdinalIgnoreCase) == true)
            {
                steps.Add(VisitCustomContainerStep(container, rawMarkdown));
            }
        }

        return new RunbookAstDocument(metadata, steps, escalationTriggers, rawMarkdown, symptoms, rawSectionsFound);
    }

    private static RunbookMetadata ExtractMetadata(MarkdownDocument document, string rawMarkdown)
    {
        var yamlBlock = document.FirstOrDefault(b => b is YamlFrontMatterBlock) as YamlFrontMatterBlock;
        if (yamlBlock is null)
        {
            return new RunbookMetadata(
                Id: string.Empty,
                Title: "Untitled Runbook",
                Slug: "untitled-runbook",
                Category: "General",
                Level: "L1",
                Risk: "low",
                Author: "Unknown",
                Version: "1.0.0",
                SchemaVersion: "rf-v1",
                Tags: [],
                LastReviewed: null,
                Audience: null,
                HasFrontmatter: false);
        }

        var lines = new List<string>();
        if (yamlBlock.Lines.Lines is not null)
        {
            foreach (var line in yamlBlock.Lines.Lines)
            {
                var str = line.Slice.ToString();
                if (!string.IsNullOrWhiteSpace(str)) lines.Add(str);
            }
        }
        else if (yamlBlock.Span.Length > 0 && yamlBlock.Span.End < rawMarkdown.Length)
        {
            var rawYaml = rawMarkdown[yamlBlock.Span.Start..(yamlBlock.Span.End + 1)];
            lines.AddRange(rawYaml.Split(["\r\n", "\r", "\n"], StringSplitOptions.None));
        }

        var parsed = ParseYamlLines(lines);

        return new RunbookMetadata(
            Id: parsed.GetValueOrDefault("id", string.Empty),
            Title: parsed.GetValueOrDefault("title", "Untitled Runbook"),
            Slug: parsed.GetValueOrDefault("slug", "untitled-runbook"),
            Category: parsed.GetValueOrDefault("category", "General"),
            Level: parsed.GetValueOrDefault("level", "L1"),
            Risk: parsed.GetValueOrDefault("risk", "low"),
            Author: parsed.GetValueOrDefault("author", "Unknown"),
            Version: parsed.GetValueOrDefault("version", "1.0.0"),
            SchemaVersion: parsed.GetValueOrDefault("schema_version", "rf-v1"),
            Tags: parsed.TryGetValue("tags", out var tagsObj) && tagsObj is List<string> tagsList ? tagsList : [],
            LastReviewed: parsed.TryGetValue("last_reviewed", out var lr) ? (string)lr : parsed.TryGetValue("lastreviewed", out var lr2) ? (string)lr2 : null,
            Audience: parsed.TryGetValue("audience", out var aud) ? (string)aud : parsed.TryGetValue("target_audience", out var aud2) ? (string)aud2 : null,
            HasFrontmatter: lines.Count > 0);
    }

    private static Dictionary<string, dynamic> ParseYamlLines(List<string> lines)
    {
        var dict = new Dictionary<string, dynamic>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#') || trimmed == "---") continue;

            var colonIdx = trimmed.IndexOf(':');
            if (colonIdx <= 0) continue;

            var key = trimmed[..colonIdx].Trim();
            var val = trimmed[(colonIdx + 1)..].Trim();

            if (val.StartsWith('[') && val.EndsWith(']'))
            {
                var inner = val[1..^1];
                var items = inner.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(s => s.Trim('"', '\''))
                    .ToList();
                dict[key] = items;
            }
            else
            {
                dict[key] = val.Trim('"', '\'');
            }
        }
        return dict;
    }

    private static StepNode VisitCustomContainerStep(CustomContainer container, string rawMarkdown)
    {
        var argString = container.Arguments ?? string.Empty;
        var attributes = AttributeParser.Parse(argString);

        var commands = new List<CommandNode>();
        var assertions = new List<AssertionNode>();
        var questions = new List<QuestionNode>();
        var descriptionBuilder = new StringBuilder();

        foreach (var child in container)
        {
            switch (child)
            {
                case ParagraphBlock paragraph:
                    var text = ExtractParagraphText(paragraph, rawMarkdown);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        if (descriptionBuilder.Length > 0) descriptionBuilder.AppendLine();
                        descriptionBuilder.Append(text);
                    }
                    break;

                case FencedCodeBlock codeBlock:
                    commands.Add(VisitCodeBlock(codeBlock, rawMarkdown));
                    break;

                case CustomContainer innerContainer:
                    if (innerContainer.Info?.Equals("assert", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        assertions.Add(VisitCustomContainerAssert(innerContainer, rawMarkdown));
                    }
                    else if (innerContainer.Info?.Equals("question", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        questions.Add(VisitCustomContainerQuestion(innerContainer));
                    }
                    break;

                case AssertionBlock assertBlock:
                    assertions.Add(new AssertionNode(
                        Target: assertBlock.Target,
                        Comparator: assertBlock.Comparator,
                        ExpectedValue: assertBlock.ExpectedValue,
                        Message: ExtractContainerText(assertBlock, rawMarkdown)));
                    break;

                case QuestionBlock questionBlock:
                    questions.Add(new QuestionNode(
                        Prompt: questionBlock.Prompt,
                        Options: questionBlock.Options));
                    break;
            }
        }

        return new StepNode(
            Id: attributes.GetValueOrDefault("id", string.Empty),
            Title: attributes.GetValueOrDefault("title", string.Empty),
            Risk: attributes.GetValueOrDefault("risk", "low"),
            Commands: commands,
            Assertions: assertions,
            Questions: questions,
            Description: descriptionBuilder.Length > 0 ? descriptionBuilder.ToString() : null);
    }

    private static AssertionNode VisitCustomContainerAssert(CustomContainer container, string rawMarkdown)
    {
        var argString = container.Arguments ?? string.Empty;
        var attributes = AttributeParser.Parse(argString);

        var target = attributes.GetValueOrDefault("target", "stdout");
        var comparator = attributes.GetValueOrDefault("condition", attributes.GetValueOrDefault("comparator", "equals"));
        var expected = attributes.GetValueOrDefault("value", attributes.GetValueOrDefault("expected", attributes.GetValueOrDefault("exitcode", string.Empty)));
        var message = ExtractContainerText(container, rawMarkdown);

        return new AssertionNode(target, comparator, expected, message);
    }

    private static QuestionNode VisitCustomContainerQuestion(CustomContainer container)
    {
        var argString = container.Arguments ?? string.Empty;
        var attributes = AttributeParser.Parse(argString);

        var prompt = attributes.GetValueOrDefault("prompt", string.Empty);
        var options = new List<string>();

        if (attributes.TryGetValue("options", out var optionsRaw) && !string.IsNullOrWhiteSpace(optionsRaw))
        {
            var split = optionsRaw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            options.AddRange(split);
        }

        return new QuestionNode(prompt, options);
    }

    private static StepNode VisitStepBlock(StepBlock stepBlock, string rawMarkdown)
    {
        var commands = new List<CommandNode>();
        var assertions = new List<AssertionNode>();
        var questions = new List<QuestionNode>();
        var descriptionBuilder = new StringBuilder();

        foreach (var child in stepBlock)
        {
            switch (child)
            {
                case ParagraphBlock paragraph:
                    var text = ExtractParagraphText(paragraph, rawMarkdown);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        if (descriptionBuilder.Length > 0) descriptionBuilder.AppendLine();
                        descriptionBuilder.Append(text);
                    }
                    break;

                case FencedCodeBlock codeBlock:
                    commands.Add(VisitCodeBlock(codeBlock, rawMarkdown));
                    break;

                case AssertionBlock assertBlock:
                    assertions.Add(new AssertionNode(
                        Target: assertBlock.Target,
                        Comparator: assertBlock.Comparator,
                        ExpectedValue: assertBlock.ExpectedValue,
                        Message: ExtractContainerText(assertBlock, rawMarkdown)));
                    break;

                case QuestionBlock questionBlock:
                    questions.Add(new QuestionNode(
                        Prompt: questionBlock.Prompt,
                        Options: questionBlock.Options));
                    break;
            }
        }

        return new StepNode(
            Id: stepBlock.StepId,
            Title: stepBlock.Title,
            Risk: stepBlock.Risk,
            Commands: commands,
            Assertions: assertions,
            Questions: questions,
            Description: descriptionBuilder.Length > 0 ? descriptionBuilder.ToString() : null);
    }

    private static CommandNode VisitCodeBlock(FencedCodeBlock codeBlock, string rawMarkdown)
    {
        var rawLine = string.Empty;
        if (codeBlock.Span.Start >= 0 && codeBlock.Span.Start < rawMarkdown.Length)
        {
            var spanText = rawMarkdown[codeBlock.Span.Start..];
            var newlineIdx = spanText.IndexOfAny(['\r', '\n']);
            rawLine = newlineIdx >= 0 ? spanText[..newlineIdx] : spanText;
        }

        var header = $"{rawLine} {codeBlock.Info} {codeBlock.Arguments}".Trim();
        var attributes = AttributeParser.Parse(header);

        var language = "bash";
        if (attributes.TryGetValue("lang", out var langVal) && !string.IsNullOrWhiteSpace(langVal))
        {
            language = langVal;
        }
        else if (!string.IsNullOrWhiteSpace(codeBlock.Info))
        {
            var firstWord = codeBlock.Info.Split([' ', '{'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(firstWord))
            {
                language = firstWord;
            }
        }
        else if (!string.IsNullOrWhiteSpace(rawLine))
        {
            var fenceEnd = rawLine.TrimStart('`', '~');
            var firstWord = fenceEnd.Split([' ', '{'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(firstWord))
            {
                language = firstWord;
            }
        }

        var timeout = attributes.GetValueOrDefault("timeout");
        var mode = attributes.GetValueOrDefault("mode");
        var requiresElevation = false;
        if (attributes.TryGetValue("requires_elevation", out var elevVal))
        {
            requiresElevation = bool.TryParse(elevVal, out var b) && b;
        }

        var sb = new StringBuilder();
        if (codeBlock.Lines.Lines is not null)
        {
            foreach (var line in codeBlock.Lines.Lines)
            {
                var slice = line.Slice;
                if (!slice.IsEmpty)
                {
                    sb.AppendLine(slice.ToString());
                }
            }
        }
        else if (codeBlock.Span.Length > 0 && codeBlock.Span.End < rawMarkdown.Length)
        {
            var raw = rawMarkdown[codeBlock.Span.Start..(codeBlock.Span.End + 1)];
            var lines = raw.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
            for (var i = 1; i < lines.Length - 1; i++)
            {
                sb.AppendLine(lines[i]);
            }
        }

        return new CommandNode(
            Language: language,
            Script: sb.ToString().TrimEnd(),
            RequiresElevation: requiresElevation,
            Timeout: timeout,
            Mode: mode);
    }

    private static string ExtractContainerText(ContainerBlock container, string rawMarkdown)
    {
        var sb = new StringBuilder();
        foreach (var child in container)
        {
            if (child is ParagraphBlock paragraph)
            {
                var text = ExtractParagraphText(paragraph, rawMarkdown);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    if (sb.Length > 0) sb.AppendLine();
                    sb.Append(text);
                }
            }
        }
        return sb.ToString();
    }

    private static string ExtractInlineText(ContainerInline? inline)
    {
        if (inline is null) return string.Empty;
        var sb = new StringBuilder();
        foreach (var item in inline)
        {
            if (item is LiteralInline literal)
            {
                sb.Append(literal.Content.ToString());
            }
            else if (item is ContainerInline container)
            {
                sb.Append(ExtractInlineText(container));
            }
            else
            {
                sb.Append(item.ToString());
            }
        }
        return sb.ToString().Trim();
    }

    private static string ExtractParagraphText(ParagraphBlock paragraph, string rawMarkdown)
    {
        if (paragraph.Inline is not null)
        {
            var text = ExtractInlineText(paragraph.Inline);
            if (!string.IsNullOrWhiteSpace(text)) return text;
        }

        if (paragraph.Lines.Lines is not null)
        {
            var sb = new StringBuilder();
            foreach (var line in paragraph.Lines.Lines)
            {
                var slice = line.Slice;
                if (!slice.IsEmpty)
                {
                    sb.Append(slice.ToString()).Append(' ');
                }
            }
            return sb.ToString().Trim();
        }

        if (paragraph.Span.Length > 0 && paragraph.Span.End < rawMarkdown.Length)
        {
            return rawMarkdown[paragraph.Span.Start..(paragraph.Span.End + 1)].Trim();
        }

        return string.Empty;
    }

    private static string ExtractHeadingText(HeadingBlock heading, string rawMarkdown)
    {
        if (heading.Inline is not null)
        {
            var text = ExtractInlineText(heading.Inline);
            if (!string.IsNullOrWhiteSpace(text)) return text;
        }

        if (heading.Lines.Lines is not null)
        {
            var sb = new StringBuilder();
            foreach (var line in heading.Lines.Lines)
            {
                var slice = line.Slice;
                if (!slice.IsEmpty)
                {
                    sb.Append(slice.ToString());
                }
            }
            return sb.ToString().Trim();
        }

        if (heading.Span.Length > 0 && heading.Span.End < rawMarkdown.Length)
        {
            var raw = rawMarkdown[heading.Span.Start..(heading.Span.End + 1)];
            return raw.TrimStart('#').Trim();
        }

        return string.Empty;
    }

    private static List<string> ExtractListItems(ListBlock listBlock, string rawMarkdown)
    {
        var items = new List<string>();
        foreach (var child in listBlock)
        {
            if (child is ListItemBlock listItem)
            {
                var sb = new StringBuilder();
                foreach (var itemChild in listItem)
                {
                    if (itemChild is ParagraphBlock paragraph)
                    {
                        sb.Append(ExtractParagraphText(paragraph, rawMarkdown));
                    }
                }
                var str = sb.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(str)) items.Add(str);
            }
        }
        return items;
    }
}
