using System.Collections.Generic;

namespace Lexy.Compiler.Language.VariableTypes;

public abstract class VariableType
{
    public abstract bool IsAssignableFrom(VariableType type);
}