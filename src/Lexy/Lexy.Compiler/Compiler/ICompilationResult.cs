using System;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Tables;

namespace Lexy.Compiler.Compiler;

public interface ICompilationResult: IDisposable
{
    ExecutableFunction GetFunction(Function function);

    Type GetEnumType(string type);
}