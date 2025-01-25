using System;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser;

public interface IValidationContext
{
    IParserLogger Logger { get; }
    RootNodeList RootNodes { get; }

    IVariableContext VariableContext { get; }

    IDisposable CreateVariableScope();
}