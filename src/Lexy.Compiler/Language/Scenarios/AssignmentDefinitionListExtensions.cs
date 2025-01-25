using System.Collections.Generic;

namespace Lexy.Compiler.Language.Scenarios;

public static class AssignmentDefinitionListExtensions
{
    public static IEnumerable<AssignmentDefinition> Flatten(this IEnumerable<IAssignmentDefinition> assignmentDefinitions)
    {
        foreach (var assignmentDefinition in assignmentDefinitions)
        foreach (var flattenAssignmentDefinition in assignmentDefinition.Flatten())
        {
            yield return flattenAssignmentDefinition;
        }
    }
}