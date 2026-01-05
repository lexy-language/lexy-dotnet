using System;
using System.Collections.Generic;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.VariableTypes.Declaration;

public sealed class ImplicitVariableTypeDeclaration : VariableTypeDeclaration
{
    public ImplicitVariableTypeDeclaration(SourceReference reference) : base(reference)
    {
    }

    protected override VariableType ValidateVariableType(IValidationContext context)
    {
        throw new InvalidOperationException("Not supported. Nodes should be Validated first.");
    }

    public void Define(VariableType variableType)
    {
        VariableType = variableType;
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield break;
    }

    protected override void Validate(IValidationContext context)
    {
        //suppress base validator
    }
}