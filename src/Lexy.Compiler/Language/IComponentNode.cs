namespace Lexy.Compiler.Language;

public interface IComponentNode : IParsableNode
{
    string NodeName { get; }
}