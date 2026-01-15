using Lexy.Compiler.Language;
using Lexy.RunTime;

namespace Lexy.Compiler.DependencyGraph;

public static class DependencyGraphFactory
{
    public static Dependencies Create(ComponentNodeList componentNodes)
    {
        Assert.NotNull(componentNodes, nameof(componentNodes));

        var dependencies = new Dependencies(componentNodes);
        dependencies.Build();
        return dependencies;
    }
}
