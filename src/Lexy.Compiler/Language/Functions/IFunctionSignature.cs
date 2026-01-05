using System.Collections.Generic;
using Lexy.Compiler.Language.VariableTypes;

namespace Lexy.Compiler.Language.Functions;

public interface IFunctionSignature
{
    VariableType ResultsType { get; }
    IReadOnlyList<VariableType> ParametersTypes { get; }
    bool Matches(IReadOnlyList<VariableType> argumentTypes);
}