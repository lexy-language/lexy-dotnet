using System.Collections.Generic;
using Lexy.Compiler.Language.VariableTypes;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class LexyFunctionCall : ILexyFunctionCall
{
    public IReadOnlyList<VariableType> ParametersTypes { get; }
    public VariableType ResultsType { get; }
    public IReadOnlyList<Expression> ArgumentExpressions { get; set; }

    public LexyFunctionCall(IReadOnlyList<VariableType> parametersTypes, VariableType resultsType, IReadOnlyList<Expression> argumentExpressions)
    {
        ParametersTypes = parametersTypes;
        ResultsType = resultsType;
        ArgumentExpressions = argumentExpressions;
    }


    public IEnumerable<VariableUsage> UsedVariables()
    {
        //returned by FunctionCallExpression.UsedVariables;
        yield break;
    }
}