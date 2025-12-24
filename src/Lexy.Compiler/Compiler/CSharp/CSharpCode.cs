using System;
using Lexy.Compiler.Compiler.CSharp.Writers;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Enums;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Scenarios;
using Lexy.Compiler.Language.Types;
using Table = Lexy.Compiler.Language.Tables.Table;

namespace Lexy.Compiler.Compiler.CSharp;

internal static class CSharpCode
{
    public static IComponentTokenWriter GetWriter(IComponentNode componentNode)
    {
        return componentNode switch
        {
            Function _ => new FunctionWriter(),
            EnumDefinition _ => new EnumWriter(),
            Table _ => new TableWriter(),
            TypeDefinition _ => new TypeWriter(),
            Scenario _ => null,
            _ => throw new InvalidOperationException("No writer defined: " + componentNode.GetType())
        };
    }
}