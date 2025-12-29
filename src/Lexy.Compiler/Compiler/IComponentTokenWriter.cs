using Lexy.Compiler.Language;

namespace Lexy.Compiler.Compiler;

internal interface IComponentTokenWriter<TComponentNode> where TComponentNode : IComponentNode
{
    GeneratedClass CreateCode(TComponentNode generateNode);
}