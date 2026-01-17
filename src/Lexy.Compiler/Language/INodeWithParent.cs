namespace Lexy.Compiler.Language;

public interface INodeWithParent : INode
{
    void SetParent(INode node);
}
