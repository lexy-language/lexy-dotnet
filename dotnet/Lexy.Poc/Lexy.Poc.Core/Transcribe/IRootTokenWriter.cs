using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Compiler
{
    internal interface IRootTokenWriter
    {
        GeneratedClass CreateCode(ClassWriter classWriter, IRootComponent generateNode, Components components);
    }
}