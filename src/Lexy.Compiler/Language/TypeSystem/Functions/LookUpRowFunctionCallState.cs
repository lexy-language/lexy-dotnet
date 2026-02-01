using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.TypeSystem.Functions;

internal class LookUpRowFunctionCallState : IFunctionCallState
{
    public string TableName { get; }

    public Expression ValueExpression { get; }

    public Expression DiscriminatorExpression { get; }

    public string SearchValueColumn { get; }

    public string DiscriminatorColumn { get; }

    public Type ResultsType { get; }

    public SourceReference Reference { get; }

    public LookUpRowFunctionCallState(SourceReference reference, string tableName, Expression valueExpression, Expression discriminatorExpression, string searchValueColumn, string discriminatorColumn, Type resultsType)
    {
        Reference = reference;
        TableName = tableName;
        ValueExpression = valueExpression;
        DiscriminatorExpression = discriminatorExpression;
        SearchValueColumn = searchValueColumn;
        DiscriminatorColumn = discriminatorColumn;
        ResultsType = resultsType;
    }

    public Symbol GetSymbol()
    {
        return LookUpRowFunctionSymbol.Create(Reference, TableName, ResultsType);
    }
}
