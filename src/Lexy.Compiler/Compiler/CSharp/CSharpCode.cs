using System;
using Lexy.Compiler.Compiler.CSharp.Components;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Enums;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Scenarios;
using Lexy.Compiler.Language.Types;
using Table = Lexy.Compiler.Language.Tables.Table;

namespace Lexy.Compiler.Compiler.CSharp;

internal static class CSharpCode
{
    public static GeneratedClass Generate(IComponentNode componentNode)
    {
        return componentNode switch
        {
            Function function => FunctionClass.CreateCode(function),
            EnumDefinition enumDefinition => EnumClass.CreateCode(enumDefinition),
            Table table => TableClass.CreateCode(table),
            TypeDefinition typeDefinition => TypeClass.CreateCode(typeDefinition),
            Scenario _ => null,
            _ => throw new InvalidOperationException("No writer defined: " + componentNode.GetType())
        };
    }
}