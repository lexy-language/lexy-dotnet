using System.Collections.Generic;
using Lexy.Compiler.Language.TypeSystem;

namespace Lexy.Compiler.Language.Functions;

public class FunctionSignature
{
    public Type ResultsType { get; }
    public IReadOnlyList<Type> ParametersTypes { get; }

    public FunctionSignature(IReadOnlyList<Type> parametersTypes, Type resultsType)
    {
        ParametersTypes = parametersTypes;
        ResultsType = resultsType;
    }

    public bool Matches(IReadOnlyList<Type> argumentTypes)
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
