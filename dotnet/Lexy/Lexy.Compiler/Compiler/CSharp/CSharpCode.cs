using System;
using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Compiler.CSharp
{
    internal static class CSharpCode
    {
        public static IRootTokenWriter GetWriter(IRootNode rootNode)
        {
            return rootNode switch
            {
                Function _ => new FunctionWriter(),
                EnumDefinition _ => new EnumWriter(),
                Table _ => new TableWriter(),
                Scenario _ => null,
                _ => throw new InvalidOperationException("No writer defined: " + rootNode.GetType())
            };
        }
    }
}