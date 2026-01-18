using Lexy.Compiler.Language;
using Lexy.Compiler.Language.TypeSystem;

namespace Lexy.Compiler.Parser;

public interface IVariableContext
{
    void AddVariable(string variableName, Type type, VariableSource source);

    void RegisterVariableAndVerifyUnique(SourceReference reference, string variableName, Type type,
        VariableSource source);

    bool Contains(string variableName);
    bool Contains(IdentifierPath path);

    Type GetVariableType(string variableName);
    Type GetVariableType(IdentifierPath path);

    VariableEntry GetVariable(string variableName);

    VariableReference CreateVariableReference(SourceReference reference, IdentifierPath path);
}
