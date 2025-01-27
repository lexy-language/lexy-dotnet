using System;
using System.Collections.Generic;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.VariableTypes;

public sealed class ImplicitVariableDeclaration : VariableDeclarationType
{
    public ImplicitVariableDeclaration(SourceReference reference) : base(reference)
    {
    }

    protected override VariableType CreateVariableType(IValidationContext context)
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