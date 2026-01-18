using Lexy.Compiler.Generation.CSharp.Syntax;
using Lexy.Compiler.Language.Types;
using Lexy.RunTime;

namespace Lexy.Compiler.Generation.CSharp.Components;

internal static class TypeClass
{
    public static GeneratedClass CreateCode(TypeDefinition typeDefinition)
    {
        Assert.NotNull(typeDefinition, nameof(typeDefinition));

        var className = ClassNames.TypeClassName(typeDefinition.Name);

        var classDeclaration = VariableClass.Syntax(className, typeDefinition.Variables);

        return new GeneratedClass(typeDefinition, className, classDeclaration);
    }
}
