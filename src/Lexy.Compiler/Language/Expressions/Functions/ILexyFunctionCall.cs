using System.Collections.Generic;

namespace Lexy.Compiler.Language.Expressions.Functions;

public interface ILexyFunctionCall
{
    IEnumerable<VariableUsage> UsedVariables();
}