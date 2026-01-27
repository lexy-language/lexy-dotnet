using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Symbols;

namespace Lexy.Compiler.Language.TypeSystem.Functions;

internal class LookUpFunctionCallState : IFunctionCallState
{
    public string TableName { get; }

    public Expression ValueExpression { get; }

    public Expression DiscriminatorExpression { get; }

    public string ResultColumn { get; }

    public Type ResultColumnType { get; }

    public string SearchValueColumn { get; }

    public string DiscriminatorColumn { get; }

    public SourceReference Reference { get; }

    public LookUpFunctionCallState(SourceReference reference, string tableName, Expression valueExpression, Expression discriminatorExpression, string resultColumn, Type resultColumnType, string searchValueColumn, string discriminatorColumn)
    {
        Reference = reference;
        TableName = tableName;
        ValueExpression = valueExpression;
        DiscriminatorExpression = discriminatorExpression;
        ResultColumn = resultColumn;
        ResultColumnType = resultColumnType;
        SearchValueColumn = searchValueColumn;
        DiscriminatorColumn = discriminatorColumn;
    }

    public Symbol GetSymbol()
    {
        return LookUpFunctionSymbol.Create(Reference, TableName, ResultColumnType);
    }
}
