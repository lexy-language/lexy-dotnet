using System.Collections.Generic;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.Compiler;

public interface ILexyCompiler
{
    CompilationResult Compile(IEnumerable<IRootNode> nodes);
}