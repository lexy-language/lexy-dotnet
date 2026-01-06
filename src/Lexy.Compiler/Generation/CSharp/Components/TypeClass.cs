using System;
using Lexy.Compiler.Generation.CSharp.Syntax;
using Lexy.Compiler.Language.Types;

namespace Lexy.Compiler.Generation.CSharp.Components;

internal static class TypeClass
{
    public static GeneratedClass CreateCode(TypeDefinition typeDefinition)
    {
        if (typeDefinition == null) throw new ArgumentNullException(nameof(typeDefinition));

        var className = ClassNames.TypeClassName(typeDefinition.Name.Value);

        var classDeclaration = VariableClass.Syntax(className, typeDefinition.Variables);

        return new GeneratedClass(typeDefinition, className, classDeclaration);
    }
}