using System.Reflection;
using Lexy.Poc.Core.Compiler.CSharp;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Compiler
{
    public interface IExecutionEnvironment
    {
        void CreateExecutables(Assembly assembly);

        void AddType(GeneratedClass generatedType);
        CompilerResult Result();
    }
}