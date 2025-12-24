using System;
using Lexy.Compiler.Compiler.CSharp.Syntax;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Tables;
using Lexy.Compiler.Language.Types;

namespace Lexy.Compiler.Compiler.CSharp.Writers;

internal class TypeWriter : IComponentTokenWriter
{
    public GeneratedClass CreateCode(IComponentNode node)
    {
        if (node is not TypeDefinition typeDefinition) throw new InvalidOperationException("Component token not type");

        var className = ClassNames.TypeClassName(typeDefinition.Name.Value);

        var classDeclaration = VariableClass.TranslateVariablesClass(className, typeDefinition.Variables);

        return new GeneratedClass(node, className, classDeclaration);
    }
}