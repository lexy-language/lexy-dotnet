using System.IO;
using Lexy.Compiler.Compiler.CSharp;

namespace Lexy.Compiler.Compiler;

public interface ICompilationEnvironment
{
    string Namespace { get; }

    void AddType(GeneratedClass generatedType);

    CompilationResult Result();

    void CreateExecutables(MemoryStream dllStream);
}