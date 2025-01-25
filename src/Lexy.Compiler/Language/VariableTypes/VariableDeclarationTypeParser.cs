using System;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.VariableTypes;

public static class VariableDeclarationTypeParser
{
    public static VariableDeclarationType Parse(string type, SourceReference reference)
    {
        if (reference == null) throw new ArgumentNullException(nameof(reference));

        if (type == Keywords.ImplicitVariableDeclaration) return new ImplicitVariableDeclaration(reference);
        if (TypeNames.Contains(type)) return new PrimitiveVariableDeclarationType(type, reference);

        return new CustomVariableDeclarationType(type, reference);
    }
}