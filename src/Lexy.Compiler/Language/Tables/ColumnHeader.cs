using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.TypeSystem.Declaration;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Symbols;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Tables;

public class ColumnHeader : Node
{
    public TypeDeclaration TypeDeclaration { get; }
    public VariableNameExpression NameExpression { get; }

    public string Name { get; }

    private ColumnHeader(VariableNameExpression nameExpression, TypeDeclaration typeDeclaration, SourceReference reference) : base(reference)
    {
        NameExpression = Assert.NotNull(nameExpression, nameof(nameExpression));
        TypeDeclaration = Assert.NotNull(typeDeclaration, nameof(typeDeclaration));
        Name = nameExpression.Name;
    }

    public static ColumnHeader Parse(IParseLineContext context, int index)
    {
        var tokens = context.Line.Tokens;
        var isValid = context.ValidateTokens<TableHeader>()
            .Type<StringLiteralToken>(index)
            .Type<StringLiteralToken>(index + 1)
            .Type<TableSeparatorToken>(index + 2)
            .IsValid;

        if (!isValid) return null;

        var typeToken = tokens[index];
        var typeReference = tokens.Reference(index, 1);
        var type = TypeDeclarationParser.Parse(typeToken, typeReference);

        var nameToken = tokens[index + 1];
        var source = new ExpressionSource(context.Line, new TokenList(context.Line, nameToken));
        var name = VariableNameExpression.Parse(source, SymbolKind.TableColumn);
        if (name.IsSuccess == false) return null;

        var reference = tokens.Reference(index, 2);

        return new ColumnHeader(name.Result, type, reference);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield return TypeDeclaration;
        yield return NameExpression;
    }

    protected override void Validate(IValidationContext context)
    {
    }

    public override Symbol GetSymbol()
    {
        return new Symbol(Reference, $"{TypeDeclaration} {NameExpression.ToString()}", string.Empty, SymbolKind.TableColumn);
    }
}
