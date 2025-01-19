using System.Collections.Generic;

namespace Lexy.Compiler.Language.Scenarios;

public interface IAssignmentDefinition : INode
{
    IEnumerable<AssignmentDefinition> Flatten();
}