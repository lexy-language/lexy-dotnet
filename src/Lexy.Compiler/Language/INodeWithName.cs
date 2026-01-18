namespace Lexy.Compiler.Language;

public interface INodeWithName: INode
{
    string Name { get; }
}
