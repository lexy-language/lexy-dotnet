using System;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Tables;
using Lexy.Compiler.Language.Types;

namespace Lexy.Compiler.Compiler.CSharp.Writers;

internal class TypeWriter : IRootTokenWriter
{
    public GeneratedClass CreateCode(IRootNode node)
    {
        if (node is not TypeDefinition typeDefinition) throw new InvalidOperationException("Root token not type");

        var className = ClassNames.TypeClassName(typeDefinition.Name.Value);

        var classDeclaration = VariableClassFactory.TranslateVariablesClass(className, typeDefinition.Variables);

        return new GeneratedClass(node, className, classDeclaration);
    }
}