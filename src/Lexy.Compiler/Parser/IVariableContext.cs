using Lexy.Compiler.Language;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.RunTime;

namespace Lexy.Compiler.Parser;

public interface IVariableContext
{
    void AddVariable(string variableName, VariableType type, VariableSource source);

    void RegisterVariableAndVerifyUnique(SourceReference reference, string variableName, VariableType type,
        VariableSource source);

    bool Contains(string variableName);
    bool Contains(IdentifierPath path, IValidationContext context);

    VariableType GetVariableType(string variableName);
    VariableType GetVariableType(IdentifierPath path, IValidationContext context);

    VariableEntry GetVariable(string variableName);

    VariableReference CreateVariableReference(SourceReference reference, IdentifierPath path, IValidationContext validationContext);
}