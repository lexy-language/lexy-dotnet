using System.Collections.Generic;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Functions;

public class FunctionResults : ParsableNode
{
    private readonly List<VariableDefinition> variables = new();

    public IReadOnlyList<VariableDefinition> Variables => variables;

    public FunctionResults(SourceReference reference) : base(reference)
    {
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        var variableDefinition = VariableDefinition.Parse(VariableSource.Results, context);
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
}