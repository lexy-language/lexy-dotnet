using System.Collections.Generic;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Language.VariableTypes.Declaration;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Tables;

public class ColumnHeader : Node
{
    public string Name { get; }
    public VariableTypeDeclaration Type { get; }

    private ColumnHeader(string name, VariableTypeDeclaration type, SourceReference reference) : base(reference)
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