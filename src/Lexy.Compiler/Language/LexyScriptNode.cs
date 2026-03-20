using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language.Enums;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Scenarios;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.RunTime;
using Table = Lexy.Compiler.Language.Tables.Table;

namespace Lexy.Compiler.Language;

public class LexyScriptNode : ComponentNode
{
    private readonly IList<Include> includes = new List<Include>();
    private IEnumerable<IComponentNode> sortedNodes;

    public Comments Comments { get; }
    public ComponentNodeList ComponentNodes { get; } = new();

    public LexyScriptNode(IProject project) : base(nameof(LexyScriptNode), new NodeReference(null), new SourceReference(project.File("LexyScript"), 1, 1, 1))
    {
        Comments = new Comments(new NodeReference(this), Reference);
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        var line = context.Line;

        if (line.Tokens.IsComment()) return Comments;

        var componentNode = ParseComponentNode(context);
        if (componentNode == null) return this;

        ComponentNodes.Add(componentNode);
        context.Symbols.Add(componentNode);

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

        var reference = context.Line.Tokens.AllReference();
        var tokenName = NodeName.Parse(context);
        if (tokenName == null)
        {
            var firstToken = context.Line.Tokens.Length > 0 ? context.Line.Tokens[0].Value : context.Line.Content;
            context.Logger.Fail(reference, $"Invalid token '{firstToken}'. Keyword and name expected.");
            return null;
        }

        var componentNode = tokenName.Keyword switch
        {
            Keywords.Function => Function.Create(tokenName.Name, false, new NodeReference(this), reference),
            Keywords.EnumKeyword => EnumDefinition.Parse(tokenName.Name, false, this, reference),
            Keywords.ScenarioKeyword => Scenario.Parse(tokenName, this, reference),
            Keywords.TableKeyword => new Table(tokenName.Name, new NodeReference(this), reference),
            Keywords.TypeKeyword => TypeDefinition.Parse(tokenName, this, reference),
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
            node => node.Name,
            node => $"Duplicated node name: '{node.Name}'",
            ComponentNodes);
    }

    public IEnumerable<Include> GetDueIncludes()
    {
        return includes.Where(include => include.State?.IsProcessed != true).ToList();
    }

    public void SortByDependency(IEnumerable<IComponentNode> sortedNodes)
    {
        Assert.NotNull(sortedNodes, nameof(sortedNodes));
        this.sortedNodes = WithoutScenarioInlineNode(sortedNodes);
    }

    private IList<IComponentNode> WithoutScenarioInlineNode(IEnumerable<IComponentNode> sortedNodes)
    {
        return sortedNodes
            .Where(where => (where as INestedNode)?.Nested != true)
            .ToList();
    }

    public override Symbol GetSymbol() => null;

    public override SuggestionEdit[] GetSuggestions()
    {
        return Suggestions.Edit(SuggestionsScope.CurrentLevel, with => with
            .Keyword(Keywords.Function)
            .Keyword(Keywords.EnumKeyword)
            .Keyword(Keywords.TypeKeyword)
            .Keyword(Keywords.TableKeyword)
            .Keyword(Keywords.ScenarioKeyword)
            .Keyword(Keywords.Include)
        );
    }
}
