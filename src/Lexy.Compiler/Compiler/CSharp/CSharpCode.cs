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
    public static Func<IComponentNode, GeneratedClass> GetWriter(IComponentNode componentNode)
    {
        return componentNode switch
        {
            Function _ => Cast<Function>(FunctionWriter.CreateCode),
            EnumDefinition _ => Cast<EnumDefinition>(EnumWriter.CreateCode),
            Table _ => Cast<Table>(TableWriter.CreateCode),
            TypeDefinition _ => Cast<TypeDefinition>(TypeWriter.CreateCode),
            Scenario _ => null,
            _ => throw new InvalidOperationException("No writer defined: " + componentNode.GetType())
        };
    }

    private static Func<IComponentNode, GeneratedClass> Cast<T>(Func<T, GeneratedClass> function) where T : class
    {
        return node =>
        {
            if (node is not T specific)
            {
                throw new InvalidOperationException($"Node is not: '{typeof(T)}'");
            }
            return function(specific);
        };
    }
}