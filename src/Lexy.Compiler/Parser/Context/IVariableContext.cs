using System.Collections.Generic;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.TypeSystem;

namespace Lexy.Compiler.Parser.Context;

public interface IVariableContext
{
    void AddVariable(string variableName, Type type, VariableSource source);

    void RegisterVariableAndVerifyUnique(SourceReference reference, string variableName, Type type,
        VariableSource source);

    VariableReference CreateVariableReference(SourceReference reference, IdentifierPath path);

    bool Contains(string variableName);
    bool Contains(IdentifierPath path);

    Type GetType(string variableName);
    Type GetType(IdentifierPath path);

    VariableEntry GetVariable(string variableName);
}
