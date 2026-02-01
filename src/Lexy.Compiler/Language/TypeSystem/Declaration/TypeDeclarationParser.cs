using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.TypeSystem.Declaration;

public static class TypeDeclarationParser
{
    public static TypeDeclaration Parse(Token typeToken, NodeReference parentReference, SourceReference reference)
    {
        Assert.NotNull(reference, nameof(reference));

        var type = typeToken.Value;

        if (type == Keywords.ImplicitVariableDeclaration) return new ImplicitTypeDeclaration(parentReference, reference);
        if (TypeNames.Contains(type)) return new ValueTypeDeclaration(type, parentReference, reference);

        return new ObjectTypeDeclaration(type, parentReference, reference);
    }

    public static TypeDeclaration Parse(string type, NodeReference parentReference, SourceReference reference)
    {
        Assert.NotNull(reference, nameof(reference));

        if (type == Keywords.ImplicitVariableDeclaration) return new ImplicitTypeDeclaration(parentReference, reference);
        if (TypeNames.Contains(type)) return new ValueTypeDeclaration(type, parentReference, reference);

        return new ObjectTypeDeclaration(type, parentReference, reference);
    }
}
