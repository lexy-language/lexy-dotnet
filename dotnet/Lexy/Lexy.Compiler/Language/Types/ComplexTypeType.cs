using System;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Types
{
    public abstract class ComplexTypeType : VariableType
    {
        protected ComplexTypeType(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public abstract ComplexType GetComplexType(IValidationContext context);
    }

    public class FunctionParametersType : ComplexTypeType
    {
        public string FunctionName { get; }

        public FunctionParametersType(string functionName) : base(functionName)
        {
            FunctionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
        }

        public override ComplexType GetComplexType(IValidationContext context) =>
            context.Nodes.GetFunction(FunctionName)?.GetParametersType(context);
    }

    public class FunctionResultsType : ComplexTypeType
    {
        public string FunctionName { get; }

        public FunctionResultsType(string functionName) : base(functionName)
        {
            FunctionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
        }

        public override ComplexType GetComplexType(IValidationContext context) =>
            context.Nodes.GetFunction(FunctionName)?.GetResultsType(context);
    }

    public class TableRowType : ComplexTypeType
    {
        public string TableName { get; }

        public TableRowType(string tableName) : base(tableName)
        {
            TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }

        public override ComplexType GetComplexType(IValidationContext context) =>
            context.Nodes.GetTable(TableName)?.GetRowType(context);
    }
}