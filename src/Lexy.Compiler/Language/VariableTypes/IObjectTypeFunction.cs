using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.VariableTypes.Functions;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.VariableTypes;

public interface IObjectTypeFunction
{
    ValidateMemberFunctionArgumentsResult ValidateArguments(IValidationContext context, IReadOnlyList<Expression> arguments, SourceReference reference);
    VariableType GetResultsType(IReadOnlyList<Expression> arguments);
}