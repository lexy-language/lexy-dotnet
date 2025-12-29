using Lexy.Compiler.Language.Functions;

namespace Lexy.Compiler.Language.VariableTypes;

public class FunctionType : TypeWithMembers
{
    public string Type { get; }
    public Function Function { get; }

    public FunctionType(string type, Function function)
    {
        Type = type;
        Function = function;
    }

    protected bool Equals(FunctionType other)
    {
        return Type == other.Type;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((FunctionType)obj);
    }

    public override int GetHashCode()
    {
        return Type != null ? Type.GetHashCode() : 0;
    }

    public override string ToString()
    {
        return Type;
    }

    public override VariableType MemberType(string name, IComponentNodeList componentNodes)
    {
        return name switch
        {
            Function.ParameterName => FunctionParametersType(componentNodes),
            Function.ResultsName => FunctionResultsType(componentNodes),
            _ => null
        };
    }

    private ComplexType FunctionParametersType(IComponentNodeList componentNodes)
    {
        return componentNodes.GetFunction(Type)?.GetParametersType();
    }

    private ComplexType FunctionResultsType(IComponentNodeList componentNodes)
    {
        return componentNodes.GetFunction(Type)?.GetResultsType() as ComplexType;
    }
}