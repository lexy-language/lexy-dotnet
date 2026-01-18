using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.TypeSystem.Functions;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.TypeSystem.Objects;

public abstract class ObjectFunction : IObjectMember, IObjectTypeFunction
{
    public string Name { get; }
    public Type Type { get; }

    public ObjectFunction(string name, Type type)
    {
        Name = name;
        Type = type;
    }

    public abstract ValidateMemberFunctionArgumentsResult ValidateArguments(IValidationContext context,
        IReadOnlyList<Expression> arguments,
        SourceReference reference);

    public abstract Type GetResultsType(IReadOnlyList<Expression> arguments);
}
