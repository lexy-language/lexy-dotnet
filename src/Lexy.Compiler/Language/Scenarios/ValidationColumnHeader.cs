using System.Collections.Generic;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser.Context;

namespace Lexy.Compiler.Language.Scenarios;

public class ValidationColumnHeader : Node
{
    public string Name { get; }

    private ValidationColumnHeader(string name, NodeReference parentReference, SourceReference reference) :
        base(parentReference, reference)
    {
        Name = name;
    }

    public static ValidationColumnHeader Parse(string name, NodeReference parentReference, SourceReference reference)
    {
        return new ValidationColumnHeader(name, parentReference, reference);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield break;
    }

    protected override void Validate(IValidationContext context)
    {
        var identifierPath = IdentifierPath.Parse(Name);
        var variable = context.VariableContext.GetType(identifierPath);
        if (variable == null)
        {
            context.Logger.Fail(Reference,  $"Unknown variable: '{Name}'");
        }
    }

    public override Symbol GetSymbol() => null;

    public override string ToString() => Name;
}
