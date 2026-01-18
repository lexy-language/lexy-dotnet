using System;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Type = System.Type;

namespace Lexy.Compiler.FunctionLibraries;

public interface ILibrary
{
    Type Type { get; }
    IObjectTypeFunction GetFunction(string identifier);
}