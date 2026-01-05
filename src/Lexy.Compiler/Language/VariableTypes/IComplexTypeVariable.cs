namespace Lexy.Compiler.Language.VariableTypes;

public interface IComplexTypeVariable
{
    string Name { get; }
    VariableType Type { get; }
}