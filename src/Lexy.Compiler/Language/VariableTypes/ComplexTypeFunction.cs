using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.VariableTypes.Functions;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.VariableTypes;

internal abstract class ComplexTypeFunction : IComplexTypeFunction
{
    public string Name { get; }

    public abstract ValidateInstanceFunctionArgumentsResult ValidateArguments(IValidationContext context,
        IReadOnlyList<Expression> arguments,
        SourceReference reference);

    public abstract VariableType GetResultsType(IReadOnlyList<Expression> arguments);
}