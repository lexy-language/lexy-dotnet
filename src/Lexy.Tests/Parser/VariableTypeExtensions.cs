using Lexy.Compiler.Language.TypeSystem.Declaration;
using Shouldly;

namespace Lexy.Tests.Parser;

internal static class TypeExtensions
{
    public static void ShouldBeValueType(this TypeDeclaration type, string name)
    {
        type.ShouldBeOfType<ValueTypeDeclaration>()
            .TypeName.ShouldBe(name);
    }
}
