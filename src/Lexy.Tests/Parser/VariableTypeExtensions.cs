using Lexy.Compiler.Language.TypeSystem.Declaration;
using Shouldly;

namespace Lexy.Tests.Parser;

internal static class VariableTypeExtensions
{
    public static void ShouldBePrimitiveType(this TypeDeclaration type, string name)
    {
        type.ShouldBeOfType<PrimitiveTypeDeclaration>()
            .TypeName.ShouldBe(name);
    }
}
