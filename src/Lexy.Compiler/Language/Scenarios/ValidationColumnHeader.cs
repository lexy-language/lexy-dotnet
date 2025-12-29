using System.Collections.Generic;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Scenarios;

public class ValidationColumnHeader : Node
{
    public string Name { get; }

    private ValidationColumnHeader(string name, SourceReference reference) : base(reference)
    {
        Name = name;
    }

    public static ValidationColumnHeader Parse(string name, SourceReference reference)
    {
        return new ValidationColumnHeader(name, reference);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield break;
    }

    protected override void Validate(IValidationContext context)
    {
        var variablePath = IdentifierPath.Parse(Name);
        var variable = context.VariableContext.GetVariableType(variablePath, context);
        if (variable == null)
        {
            context.Logger.Fail(Reference,  $"Unknown variable: '{Name}'");
        }
    }
}