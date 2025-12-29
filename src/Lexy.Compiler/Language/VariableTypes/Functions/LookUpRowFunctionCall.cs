using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Functions;

namespace Lexy.Compiler.Language.VariableTypes.Functions;

internal class LookUpRowFunctionCall : IInstanceFunctionCall
{
    public string TableName { get; }

    public Expression ValueExpression { get; }

    public Expression DiscriminatorExpression { get; }

    public string SearchValueColumn { get; }

    public string DiscriminatorColumn { get; }

    public LookUpRowFunctionCall(string tableName, Expression valueExpression, Expression discriminatorExpression, string searchValueColumn, string discriminatorColumn)
    {
        TableName = tableName;
        ValueExpression = valueExpression;
        DiscriminatorExpression = discriminatorExpression;
        SearchValueColumn = searchValueColumn;
        DiscriminatorColumn = discriminatorColumn;
    }
}