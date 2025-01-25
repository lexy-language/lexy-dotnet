using Lexy.Compiler.Language;
using Lexy.Compiler.Language.VariableTypes;

namespace Lexy.Compiler.Parser;

public class VariableEntry
{
    public VariableType VariableType { get; }
    public VariableSource VariableSource { get; }

    public VariableEntry(VariableType variableType, VariableSource variableSource)
    {
        VariableType = variableType;
        VariableSource = variableSource;
    }
}