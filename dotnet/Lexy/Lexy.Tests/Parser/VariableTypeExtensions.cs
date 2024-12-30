using Lexy.Compiler.Language.Types;
using Shouldly;

namespace Lexy.Tests.Parser;

internal static class VariableTypeExtensions
{
    public static void ShouldBePrimitiveType(this VariableDeclarationType type, string name)
    {
        type.ShouldBeOfType<PrimitiveVariableDeclarationType>()
            .Type.ShouldBe(name);
    }
}