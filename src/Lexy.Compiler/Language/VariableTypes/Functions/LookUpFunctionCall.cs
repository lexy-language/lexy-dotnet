using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Functions;

namespace Lexy.Compiler.Language.VariableTypes.Functions;

internal class LookUpFunctionCall : IInstanceFunctionCall
{
    public string TableName { get; }

    public Expression ValueExpression { get; }

    public Expression DiscriminatorExpression { get; }

    public string ResultColumn { get; }

    public string SearchValueColumn { get; }

    public string DiscriminatorColumn { get; }

    public LookUpFunctionCall(string tableName, Expression valueExpression, Expression discriminatorExpression, string resultColumn, string searchValueColumn, string discriminatorColumn)
    {
        TableName = tableName;
        ValueExpression = valueExpression;
        DiscriminatorExpression = discriminatorExpression;
        ResultColumn = resultColumn;
        SearchValueColumn = searchValueColumn;
        DiscriminatorColumn = discriminatorColumn;
    }
}