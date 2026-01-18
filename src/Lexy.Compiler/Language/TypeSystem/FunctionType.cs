using System.Collections.Generic;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.TypeSystem.Objects;

namespace Lexy.Compiler.Language.TypeSystem;

public class FunctionType : ObjectType
{
    public Function Function { get; }

    public FunctionType(Function function) : base(function.Name)
    {
        Function = function;
    }

    public override bool IsAssignableFrom(Type type) => Equals(type);

    protected bool Equals(FunctionType other)
    {
        return Name == other?.Name;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((FunctionType)obj);
    }

    protected override IEnumerable<IObjectMember> CreateMembers()
    {
        return new []
        {
            new ObjectNestedType(Function.ParameterName, Function.GetParametersType()),
            new ObjectNestedType(Function.ResultsName, Function.GetResultsType())
        };
    }
}
