using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Compiler.Transcribe
{
    internal interface IRootTokenWriter
    {
        GeneratedClass CreateCode(IRootNode generateNode, Nodes nodes);
    }
}