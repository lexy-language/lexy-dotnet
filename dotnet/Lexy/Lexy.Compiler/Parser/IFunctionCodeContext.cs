using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Types;

namespace Lexy.Compiler.Parser
{
    public interface IFunctionCodeContext
    {
        void AddVariable(string name, VariableType type, VariableSource source);
        void RegisterVariableAndVerifyUnique(SourceReference reference, string name, VariableType type, VariableSource source);
        void EnsureVariableExists(SourceReference reference, string name);
        bool Contains(string name);

        VariableType GetVariableType(string variableName);
        VariableSource? GetVariableSource(string variableName);

        VariableEntry GetVariable(string variableName);
    }
}