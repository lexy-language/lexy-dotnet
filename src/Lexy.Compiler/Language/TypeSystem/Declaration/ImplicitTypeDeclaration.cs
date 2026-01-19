using System;
using System.Collections.Generic;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.TypeSystem.Declaration;

public sealed class ImplicitTypeDeclaration : TypeDeclaration
{
    public ImplicitTypeDeclaration(SourceReference reference) : base(reference)
    {
    }

    protected override Type ValidateType(IValidationContext context)
    {
        throw new InvalidOperationException("Not supported. Nodes should be Validated first.");
    }

    public void Define(Type type)
    {
        Type = type;
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
