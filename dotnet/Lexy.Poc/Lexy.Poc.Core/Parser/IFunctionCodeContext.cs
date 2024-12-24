namespace Lexy.Poc.Core.Parser
{
    public interface IFunctionCodeContext
    {
        void RegisterVariableAndVerifyUnique(SourceReference reference, string variableName, VariableType variableType);
        void EnsureVariableExists(SourceReference reference, string variableName);
        VariableType GetVariableType(string variableName);
    }
}