using Lexy.Compiler.Language.TypeSystem.Objects;

namespace Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;

public class NewFunctionState
{
    public GeneratedType Type { get; }

    public NewFunctionState(GeneratedType type)
    {
        Type = type;
    }
}
