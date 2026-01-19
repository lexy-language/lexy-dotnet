using Lexy.Compiler.Parser;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.TypeSystem.Declaration;

public static class TypeDeclarationParser
{
    public static TypeDeclaration Parse(string type, SourceReference reference)
    {
        Assert.NotNull(reference, nameof(reference));

        if (type == Keywords.ImplicitVariableDeclaration) return new ImplicitTypeDeclaration(reference);
        if (TypeNames.Contains(type)) return new ValueTypeDeclaration(type, reference);

        return new ObjectTypeDeclaration(type, reference);
    }
}
