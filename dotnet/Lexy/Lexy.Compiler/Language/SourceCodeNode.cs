using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Enums;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Scenarios;
using Lexy.Compiler.Language.Tables;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language;

public class SourceCodeNode : RootNode
{
    private readonly IList<Include> includes = new List<Include>();

    public override string NodeName => "SourceCodeNode";

    public Comments Comments { get; }
    public RootNodeList RootNodes { get; } = new();

    public SourceCodeNode() : base(new SourceReference(new SourceFile("SourceCodeNode"), 1, 1))
    {
        Comments = new Comments(Reference);
    }

    public override IParsableNode Parse(IParserContext context)
    {
        var line = context.CurrentLine;

        if (line.IsEmpty()) return this;
        if (line.Tokens.IsComment()) return Comments;

        var indent = line.Indent(context);
        if (indent == null || indent > 0)
        {
            context.Logger.Fail(context.LineReference(0), $"Unexpected line: {line}");
            return this;
        }

        var rootNode = ParseRootNode(context, line);
        if (rootNode == null) return this;

        RootNodes.Add(rootNode);
        context.ProcessNode(rootNode);

        return rootNode;
    }

    private IRootNode ParseRootNode(IParserContext context, Line line)
    {
        if (Include.IsValid(line))
        {
            var include = Include.Parse(line, context);
            if (include != null)
            {
                includes.Add(include);
                return null;
            }
        }

        var tokenName = Parser.NodeName.Parse(line, context);
        if (tokenName == null) return null;

        var reference = context.LineStartReference();
        var rootNode = tokenName.Keyword switch
        {
            null => null,
            Keywords.FunctionKeyword => Function.Create(tokenName.Name, reference),
            Keywords.EnumKeyword => EnumDefinition.Parse(tokenName, reference),
            Keywords.ScenarioKeyword => Scenario.Parse(tokenName, reference),
            Keywords.TableKeyword => Table.Parse(tokenName, reference),
            Keywords.TypeKeyword => TypeDefinition.Parse(tokenName, reference),
            _ => InvalidNode(tokenName, context, reference)
        };

        return rootNode;
    }

    private IRootNode InvalidNode(NodeName tokenName, IParserContext context, SourceReference reference)
    {
        context.Logger.Fail(reference, $"Unknown keyword: {tokenName.Keyword}");
        return null;
    }

    public override IEnumerable<INode> GetChildren()
    {
        return RootNodes;
    }

    protected override void Validate(IValidationContext context)
    {
        DuplicateChecker.ValidateNode(
            context,
            node => node.Reference,
            node => node.NodeName,
            node => $"Duplicated node name: '{node.NodeName}'",
            RootNodes);
    }

    public IEnumerable<Include> GetDueIncludes()
    {
        return includes.Where(include => !include.IsProcessed).ToList();
    }
}