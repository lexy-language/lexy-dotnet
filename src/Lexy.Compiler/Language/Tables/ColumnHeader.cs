using System.Collections.Generic;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Tables;

public class ColumnHeader : Node
{
    public string Name { get; }
    public VariableDeclarationType Type { get; }

    public ColumnHeader(string name, VariableDeclarationType type, SourceReference reference) : base(reference)
    {
        Name = name;
        Type = type;
    }

    public static ColumnHeader Parse(string name, string typeName, SourceReference reference)
    {
        var type = VariableDeclarationTypeParser.Parse(typeName, reference);
        return new ColumnHeader(name, type, reference);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield return Type;
    }

    protected override void Validate(IValidationContext context)
    {
    }
}