using System;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Types;

namespace Lexy.Compiler.Parser
{
    public interface IValidationContext
    {
        IParserContext ParserContext { get; }
        IParserLogger Logger { get; }
        IVariableContext VariableContext { get; }
        RootNodeList RootNodes { get; }

        IDisposable CreateVariableScope();
    }
}