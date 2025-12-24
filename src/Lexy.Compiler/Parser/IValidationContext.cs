using System;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser;

public interface IValidationContext
{
    IParserLogger Logger { get; }
    ComponentNodeList ComponentNodes { get; }

    IVariableContext VariableContext { get; }
    ITreeValidationVisitor Visitor { get; }

    IDisposable CreateVariableScope();
}