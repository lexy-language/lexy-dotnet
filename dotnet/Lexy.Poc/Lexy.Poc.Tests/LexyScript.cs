using System;
using Lexy.Poc.Core.Compiler;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc
{
    public static class LexyScript
    {
        public static ExecutableFunction Create(string code)
        {
            if (code == null) throw new ArgumentNullException(nameof(code));

            var parser = new LexyParser();
            var typeSystem = parser.ParseFunctionCode(code);
            var function = typeSystem.GetSingleFunction();

            var compiler = new LexyCompiler();
            var environment = compiler.Compile(typeSystem, function);
            return environment.GetFunction(function);
        }
    }
}