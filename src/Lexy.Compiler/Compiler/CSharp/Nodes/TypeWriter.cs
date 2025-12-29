using System;
using Lexy.Compiler.Compiler.CSharp.Syntax;
using Lexy.Compiler.Language.Types;

namespace Lexy.Compiler.Compiler.CSharp.Writers;

internal static class TypeWriter
{
    public static GeneratedClass CreateCode(TypeDefinition typeDefinition)
    {
        if (typeDefinition == null) throw new ArgumentNullException(nameof(typeDefinition));

        var className = ClassNames.TypeClassName(typeDefinition.Name.Value);

        var classDeclaration = VariableClass.TranslateVariablesClass(className, typeDefinition.Variables);

        return new GeneratedClass(typeDefinition, className, classDeclaration);
    }
}