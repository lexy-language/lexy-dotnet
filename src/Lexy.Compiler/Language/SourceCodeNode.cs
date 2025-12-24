using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Enums;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Scenarios;
using Lexy.Compiler.Language.Tables;
using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Parser;
using Table = Lexy.Compiler.Language.Tables.Table;

namespace Lexy.Compiler.Language;

public class SourceCodeNode : ComponentNode
{
    private readonly IList<Include> includes = new List<Include>();
    private IEnumerable<IComponentNode> sortedNodes;

    public override string NodeName => "SourceCodeNode";

    public Comments Comments { get; }
    public ComponentNodeList ComponentNodes { get; } = new();

    public SourceCodeNode() : base(new SourceReference(new SourceFile("SourceCodeNode"), 1, 1))
    {
        Comments = new Comments(Reference);
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        var line = context.Line;

        if (line.Tokens.IsComment()) return Comments;

        var componentNode = ParseComponentNode(context);
        if (componentNode == null) return this;

        ComponentNodes.Add(componentNode);

        return componentNode;
    }

    private IComponentNode ParseComponentNode(IParseLineContext context)
    {
        if (Include.IsValid(context.Line))
        {
            var include = Include.Parse(context);
            if (include != null)
            {
                includes.Add(include);
                return null;
            }
        }

        var reference = context.Line.LineStartReference();
        var tokenName = Parser.NodeName.Parse(context);
        if (tokenName == null)
        {
            var firstToken = context.Line.Tokens.Length > 0 ? context.Line.Tokens[0].Value : context.Line.Content;
            context.Logger.Fail(reference, $"Invalid token '{firstToken}'. Keyword and name expected.");
            return null;
        }

        var componentNode = tokenName.Keyword switch
        {
            Keywords.FunctionKeyword => Function.Create(tokenName.Name, reference, context.ExpressionFactory),
            Keywords.EnumKeyword => EnumDefinition.Parse(tokenName, reference),
            Keywords.ScenarioKeyword => Scenario.Parse(tokenName, reference),
            Keywords.TableKeyword => new Table(tokenName.Name, reference),
            Keywords.TypeKeyword => TypeDefinition.Parse(tokenName, reference),
            _ => InvalidNode(tokenName, context, reference)
        };

        return componentNode;
    }

    private IComponentNode InvalidNode(NodeName tokenName, IParseLineContext context, SourceReference reference)
    {
        context.Logger.Fail(reference, $"Unknown keyword: {tokenName.Keyword}");
        return null;
    }

    public override IEnumerable<INode> GetChildren()
    {
        return sortedNodes ?? ComponentNodes;
    }

    protected override void Validate(IValidationContext context)
    {
        DuplicateChecker.Validate(
            context,
            node => node.Reference,
            node => node.NodeName,
            node => $"Duplicated node name: '{node.NodeName}'",
            ComponentNodes);
    }

    public IEnumerable<Include> GetDueIncludes()
    {
        return includes.Where(include => !include.IsProcessed).ToList();
    }

    public void SortByDependency(IList<IComponentNode> sortedNodes)
    {
        this.sortedNodes = WithoutScenarioInlineNode(sortedNodes);
    }

    private IList<IComponentNode> WithoutScenarioInlineNode(IList<IComponentNode> sortedNodes)
    {
        return sortedNodes
            .Where(where => ComponentNodes.GetNode(where.NodeName) != null)
            .ToList();
    }
}