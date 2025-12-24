using System.Collections.Generic;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.Compiler;

public interface ILexyCompiler
{
    ICompilationResult Compile(IEnumerable<IComponentNode> nodes);
}