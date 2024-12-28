using System;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser;

public interface IValidationContext
{
    IParserContext ParserContext { get; }
    IParserLogger Logger { get; }
    IVariableContext VariableContext { get; }
    RootNodeList RootNodes { get; }

    IDisposable CreateVariableScope();
}