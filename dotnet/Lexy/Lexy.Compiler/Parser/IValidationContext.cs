using System;
using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Parser
{
    public interface IValidationContext
    {
        IParserContext ParserContext { get; }
        IParserLogger Logger { get; }
        IFunctionCodeContext FunctionCodeContext { get; }
        Nodes Nodes { get; }

        IDisposable CreateCodeContextScope();
    }
}