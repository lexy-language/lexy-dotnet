using System;
using Lexy.Compiler.FunctionLibraries;
using Lexy.Compiler.Language;
using Lexy.Compiler.Parser.Logging;

namespace Lexy.Compiler.Parser.Context;

public interface IValidationContext
{
    IParserLogger Logger { get; }
    ComponentNodeList ComponentNodes { get; }

    IVariableContext VariableContext { get; }
    ITreeValidationVisitor Visitor { get; }

    ILibraries Libraries { get; }

    IDisposable CreateVariableScope();
}
