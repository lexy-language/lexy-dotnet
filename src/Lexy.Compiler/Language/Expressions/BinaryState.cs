using Lexy.Compiler.Language.TypeSystem;

namespace Lexy.Compiler.Language.Expressions;

public class BinaryState
{
    public Type LeftType { get; }
    public Type RightType { get; }

    public BinaryState(Type leftType, Type rightType)
    {
        LeftType = leftType;
        RightType = rightType;
    }
}