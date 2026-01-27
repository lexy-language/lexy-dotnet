using Lexy.Compiler.Language;
using Lexy.Compiler.Language.TypeSystem;

namespace Lexy.Compiler.Parser.Context;

public class VariableEntry
{
    public Type Type { get; }
    public VariableSource VariableSource { get; }

    public VariableEntry(Type type, VariableSource variableSource)
    {
        Type = type;
        VariableSource = variableSource;
    }
}