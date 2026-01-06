using System.Collections.Generic;
using Lexy.Compiler.Language.VariableTypes;

namespace Lexy.Compiler.Language.Functions;

public class FunctionSignature
{
    public VariableType ResultsType { get; }
    public IReadOnlyList<VariableType> ParametersTypes { get; }

    public FunctionSignature(IReadOnlyList<VariableType> parametersTypes, VariableType resultsType)
    {
        ParametersTypes = parametersTypes;
        ResultsType = resultsType;
    }

    public bool Matches(IReadOnlyList<VariableType> argumentTypes)
    {
        if (argumentTypes.Count != ParametersTypes.Count) return false;

        for (var index = 0; index < ParametersTypes.Count; index++)
        {
            var parametersType = ParametersTypes[index];
            var argumentType = argumentTypes[index];

            if (parametersType == null || argumentType == null)
            {
                return false;
            }

            if (!parametersType.IsAssignableFrom(argumentType))
            {
                return false;
            }
        }

        return true;
    }
}