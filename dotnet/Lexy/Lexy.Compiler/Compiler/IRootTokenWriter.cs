using Lexy.Poc.Core.Compiler.CSharp;
using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Compiler
{
    internal interface IRootTokenWriter
    {
        GeneratedClass CreateCode(IRootNode generateNode);
    }
}