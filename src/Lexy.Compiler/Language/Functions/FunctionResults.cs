using System.Collections.Generic;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;

namespace Lexy.Compiler.Language.Functions;

public class FunctionResults : ParsableNode
{
    private readonly List<VariableDefinition> variables = new();

    public IReadOnlyList<VariableDefinition> Variables => variables;

    public FunctionResults(Function parent, SourceReference reference) : base(new NodeReference(parent), reference)
    {
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        var variableDefinition = VariableDefinition.Parse(VariableSource.Results, context, new NodeReference(this));
        if (variableDefinition == null) return this;

        if (variableDefinition.DefaultExpression != null)
        {
            context.Logger.Fail(Reference,
                $"Result variable '{variableDefinition.Name}' should not have a default value.");
            return this;
        }

        variables.Add(variableDefinition);

        return this;
    }

    public override IEnumerable<INode> GetChildren()
    {
        return Variables;
    }

    protected override void Validate(IValidationContext context)
    {
    }

    public override Symbol GetSymbol()
    {
        return new Symbol(Reference, "results", "function result variables", SymbolKind.Keyword);
    }

    public override string ToString() => variables.Count.ToString();
}
