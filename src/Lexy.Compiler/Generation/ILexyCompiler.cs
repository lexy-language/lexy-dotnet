using System.Collections.Generic;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.Generation;

public interface ILexyCompiler
{
    ICompilationResult Compile(IEnumerable<IComponentNode> nodes);
}