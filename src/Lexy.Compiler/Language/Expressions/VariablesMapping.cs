using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Expressions;

public class VariablesMapping : IReadOnlyList<Mapping>
{
    private readonly IReadOnlyList<Mapping> mapping;

    public GeneratedType MappingType { get; }

    public int Count => mapping.Count;

    public Mapping this[int index] => mapping[index];

    public VariablesMapping(GeneratedType mappingType, IReadOnlyList<Mapping> mapping)
    {
        MappingType = Assert.NotNull(mappingType, nameof(mappingType));
        this.mapping = Assert.NotNull(mapping, nameof(mapping));
    }

    public IEnumerable<VariableUsage> UsedVariables(VariableAccess access)
    {
        return mapping.Select(map => map.ToUsedVariable(access));
    }

    public IEnumerator<Mapping> GetEnumerator() => mapping.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
