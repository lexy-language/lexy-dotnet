using Lexy.Compiler.Language;

namespace Lexy.Compiler.Compiler;

internal interface IComponentTokenWriter
{
    GeneratedClass CreateCode(IComponentNode generateNode);
}