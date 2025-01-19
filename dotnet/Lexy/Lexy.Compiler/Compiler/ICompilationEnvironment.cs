using System.IO;

namespace Lexy.Compiler.Compiler;

public interface ICompilationEnvironment : ICompilationResult
{
    string Namespace { get; }

    void AddType(GeneratedClass generatedType);

    void CreateExecutables(MemoryStream dllStream);
}