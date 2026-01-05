using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Language.VariableTypes.Declaration;
using Shouldly;

namespace Lexy.Tests.Parser;

internal static class VariableTypeExtensions
{
    public static void ShouldBePrimitiveType(this VariableTypeDeclaration type, string name)
    {
        type.ShouldBeOfType<PrimitiveVariableTypeDeclaration>()
            .Type.ShouldBe(name);
    }
}