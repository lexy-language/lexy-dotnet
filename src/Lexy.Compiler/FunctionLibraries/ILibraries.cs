using System;
using System.Collections.Generic;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.FunctionLibraries;

public interface ILibraries
{
    ILibrary GetLibrary(IdentifierPath identifier);

    IEnumerable<Type> AllTypes();
}