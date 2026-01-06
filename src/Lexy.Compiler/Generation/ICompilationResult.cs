using System;
using Lexy.Compiler.Language.Functions;

namespace Lexy.Compiler.Generation;

public interface ICompilationResult: IDisposable
{
    ExecutableFunction GetFunction(Function function);

    Type GetEnumType(string type);
}