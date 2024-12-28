using System.Reflection;
using Lexy.Compiler.Compiler.CSharp;

namespace Lexy.Compiler.Compiler;

public interface IExecutionEnvironment
{
    void CreateExecutables(Assembly assembly);

    void AddType(GeneratedClass generatedType);
    CompilerResult Result();
}