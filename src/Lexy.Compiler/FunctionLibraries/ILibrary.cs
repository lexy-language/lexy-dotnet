using System;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.VariableTypes;

namespace Lexy.Compiler.FunctionLibraries;

public interface ILibrary
{
    Type Type { get; }
    IObjectTypeFunction GetFunction(string identifier);
}