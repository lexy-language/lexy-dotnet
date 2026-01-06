namespace Lexy.Compiler.Language.VariableTypes;

public interface IObjectTypeVariable
{
    string Name { get; }
    VariableType Type { get; }
}