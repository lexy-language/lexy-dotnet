using Lexy.Compiler.Language.VariableTypes;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class LexyFunctionCallState
{
    public VariablesMapping ParametersMapping { get; }
    public VariableType ResultsObjectType { get; }
    public string ReturnSingleResultsVariablesName { get; }

    public LexyFunctionCallState(VariablesMapping parametersMapping, VariableType resultsObjectType, string returnSingleResultsVariablesName)
    {
        ParametersMapping = parametersMapping;
        ResultsObjectType = resultsObjectType;
        ReturnSingleResultsVariablesName = returnSingleResultsVariablesName;
    }
}