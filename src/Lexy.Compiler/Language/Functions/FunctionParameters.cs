using System.Collections.Generic;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser.Context;

namespace Lexy.Compiler.Language.Functions;

public class FunctionParameters : ParsableNode
{
    private readonly List<VariableDefinition> variables = new();

    public IReadOnlyList<VariableDefinition> Variables => variables;

    public FunctionParameters(Function parent, SourceReference reference) : base(new NodeReference(parent), reference)
    {
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        var variableDefinition = VariableDefinition.Parse(VariableSource.Parameters, context, new NodeReference(this));
        if (variableDefinition != null) variables.Add(variableDefinition);
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
        return new Symbol(Reference, "parameters", "function parameter variables", SymbolKind.Keyword);
    }

    public override string ToString() => variables.Count.ToString();
}
