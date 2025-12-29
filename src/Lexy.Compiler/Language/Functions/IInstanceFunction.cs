using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Functions;

public interface IInstanceFunction
{
    ValidateInstanceFunctionArgumentsResult ValidateArguments(IValidationContext context, IReadOnlyList<Expression> arguments, SourceReference reference);
    VariableType GetResultsType(IReadOnlyList<Expression> arguments);
}