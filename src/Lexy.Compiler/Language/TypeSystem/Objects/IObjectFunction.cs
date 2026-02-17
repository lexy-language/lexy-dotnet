using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.TypeSystem.Functions;
using Lexy.Compiler.Parser.Context;

namespace Lexy.Compiler.Language.TypeSystem.Objects;

public interface IObjectFunction
{
    ValidateMemberFunctionArgumentsResult ValidateArguments(IValidationContext context, IReadOnlyList<Expression> arguments, SourceReference reference);
    Type GetResultsType(IReadOnlyList<Expression> arguments);
}
