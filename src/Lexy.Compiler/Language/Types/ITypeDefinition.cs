using System.Collections.Generic;

namespace Lexy.Compiler.Language.Types;

public interface ITypeDefinition : IComponentNode
{
    IReadOnlyList<VariableDefinition> Variables { get; }
}