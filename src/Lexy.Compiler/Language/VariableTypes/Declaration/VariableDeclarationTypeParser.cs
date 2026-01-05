using System;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.VariableTypes.Declaration;

public static class VariableDeclarationTypeParser
{
    public static VariableTypeDeclaration Parse(string type, SourceReference reference)
    {
        if (reference == null) throw new ArgumentNullException(nameof(reference));

        if (type == Keywords.ImplicitVariableDeclaration) return new ImplicitVariableTypeDeclaration(reference);
        if (TypeNames.Contains(type)) return new PrimitiveVariableTypeDeclaration(type, reference);

        return new ComplexVariableTypeDeclaration(type, reference);
    }
}