using Lexy.Compiler.Language.TypeSystem;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class LexyFunctionCallState
{
    public VariablesMapping ParametersMapping { get; }
    public Type ResultsObjectType { get; }
    public string ReturnSingleResultsVariablesName { get; }

    public LexyFunctionCallState(VariablesMapping parametersMapping, Type resultsObjectType, string returnSingleResultsVariablesName)
    {
        ParametersMapping = parametersMapping;
        ResultsObjectType = resultsObjectType;
        ReturnSingleResultsVariablesName = returnSingleResultsVariablesName;
    }
}