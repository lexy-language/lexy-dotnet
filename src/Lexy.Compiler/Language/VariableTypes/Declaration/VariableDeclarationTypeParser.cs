using Lexy.Compiler.Parser;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.VariableTypes.Declaration;

public static class VariableDeclarationTypeParser
{
    public static VariableTypeDeclaration Parse(string type, SourceReference reference)
    {
        Assert.NotNull(reference, nameof(reference));

        if (type == Keywords.ImplicitVariableDeclaration) return new ImplicitVariableTypeDeclaration(reference);
        if (TypeNames.Contains(type)) return new PrimitiveVariableTypeDeclaration(type, reference);

        return new ObjectVariableTypeDeclaration(type, reference);
    }
}