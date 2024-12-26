using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Parser
{
    public interface IFunctionCodeContext
    {
        void AddVariable(string name, VariableType type);
        void RegisterVariableAndVerifyUnique(SourceReference reference, string name, VariableType type);
        void EnsureVariableExists(SourceReference reference, string name);
        bool Contains(string name);

        VariableType GetVariableType(string variableName);
    }
}