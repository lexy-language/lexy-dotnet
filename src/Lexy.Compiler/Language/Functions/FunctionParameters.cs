using System.Collections.Generic;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Symbols;

namespace Lexy.Compiler.Language.Functions;

public class FunctionParameters : ParsableNode
{
    private readonly List<VariableDefinition> variables = new();

    public IReadOnlyList<VariableDefinition> Variables => variables;

    public FunctionParameters(SourceReference reference) : base(reference)
    {
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        var variableDefinition = VariableDefinition.Parse(VariableSource.Parameters, context);
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
}
