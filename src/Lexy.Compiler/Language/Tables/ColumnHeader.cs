using System.Collections.Generic;
using Lexy.Compiler.Language.TypeSystem.Declaration;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Tables;

public class ColumnHeader : Node
{
    public string Name { get; }
    public TypeDeclaration TypeDeclaration { get; }

    private ColumnHeader(string name, TypeDeclaration typeDeclaration, SourceReference reference) : base(reference)
    {
        Name = name;
        TypeDeclaration = typeDeclaration;
    }

    public static ColumnHeader Parse(string name, string typeName, SourceReference reference)
    {
        var type = VariableDeclarationTypeParser.Parse(typeName, reference);
        return new ColumnHeader(name, type, reference);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield return TypeDeclaration;
    }

    protected override void Validate(IValidationContext context)
    {
    }
}
