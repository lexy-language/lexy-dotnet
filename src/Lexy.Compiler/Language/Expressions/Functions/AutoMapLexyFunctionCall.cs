using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.VariableTypes;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class AutoMapLexyFunctionCall : ILexyFunctionCall
{
    private readonly IReadOnlyList<Mapping> mappingParameters;
    private readonly IReadOnlyList<Mapping> mappingResults;

    public IEnumerable<Mapping> MappingParameters => mappingParameters;
    public IEnumerable<Mapping> MappingResults => mappingResults;

    public VariableType ParametersType { get; }
    public VariableType ResultsType { get; }

    public AutoMapLexyFunctionCall(IReadOnlyList<Mapping> mappingParameters, IReadOnlyList<Mapping> mappingResults, VariableType parametersType, VariableType resultsType)
    {
        this.mappingParameters = mappingParameters;
        this.mappingResults = mappingResults;
        ParametersType = parametersType;
        ResultsType = resultsType;
    }

    public IEnumerable<VariableUsage> UsedVariables()
    {
        return mappingParameters.Select(map => map.ToUsedVariable(VariableAccess.Read))
            .Union(mappingResults.Select(map => map.ToUsedVariable(VariableAccess.Write)));
    }
}