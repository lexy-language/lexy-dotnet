using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Tables;

public class TableHeader : Node
{
    public IList<ColumnHeader> Columns { get; } = new List<ColumnHeader>();

    private TableHeader(ColumnHeader[] columns, SourceReference reference) : base(reference)
    {
        Columns = columns;
    }

    public static TableHeader Parse(IParserContext context)
    {
        var index = 0;
        var validator = context.ValidateTokens<TableHeader>();

        if (!validator.Type<TableSeparatorToken>(index).IsValid) return null;

        var headers = new List<ColumnHeader>();
        var tokens = context.CurrentLine.Tokens;
        while (++index < tokens.Length)
        {
            if (!validator
                    .Type<StringLiteralToken>(index)
                    .Type<StringLiteralToken>(index + 1)
                    .Type<TableSeparatorToken>(index + 2)
                    .IsValid)
                return null;

            var typeName = tokens.TokenValue(index);
            var name = tokens.TokenValue(++index);
            var reference = context.TokenReference(index);

            var header = ColumnHeader.Parse(name, typeName, reference);
            headers.Add(header);

            ++index;
        }

        return new TableHeader(headers.ToArray(), context.LineStartReference());
    }

    public override IEnumerable<INode> GetChildren()
    {
        return Columns;
    }

    protected override void Validate(IValidationContext context)
    {
    }

    public ColumnHeader Get(MemberAccessLiteral memberAccess)
    {
        var parts = memberAccess.Parts;
        if (parts.Length < 2) return null;
        var name = parts[1];

        return Columns.FirstOrDefault(value => value.Name == name);
    }
}