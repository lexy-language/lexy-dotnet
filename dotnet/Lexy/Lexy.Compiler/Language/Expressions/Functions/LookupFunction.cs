using System;
using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language.Expressions.Functions
{
    internal class LookupFunction : BuiltInFunction
    {
        private const string FunctionHelp = Name + " expects 4 arguments (Table, lookUpValue, SearchValueColumnName, ResultColumnName)";

        public const string Name = "LOOKUP";

        private const int Arguments = 4;
        private const int ArgumentTable = 0;
        private const int ArgumentLookupValue = 1;
        private const int ArgumentSearchValueColumn = 2;
        private const int ArgumentResultColumn = 3;

        public string TableType { get; }

        public Expression ValueExpression { get; }

        public string ResultColumnHeaderName { get; }
        public string SearchValueColumnHeaderName { get; }

        public VariableType ResultColumnType { get; private set; }
        public VariableType SearchValueColumnType { get; private set; }

        private LookupFunction(string tableType, Expression valueExpression,
            string columnHeaderName, string searchValueColumnHeaderName, SourceReference tableNameArgumentReference)
            : base(tableNameArgumentReference)
        {
            TableType = tableType ?? throw new ArgumentNullException(nameof(tableType));
            ValueExpression = valueExpression ?? throw new ArgumentNullException(nameof(valueExpression));
            ResultColumnHeaderName = columnHeaderName ?? throw new ArgumentNullException(nameof(columnHeaderName));
            SearchValueColumnHeaderName = searchValueColumnHeaderName;
        }

        public static ParseBuiltInFunctionsResult Parse(string name, SourceReference functionCallReference, IReadOnlyList<Expression> arguments)
        {
            if (arguments.Count != Arguments)
            {
                return ParseBuiltInFunctionsResult.Failed($"Invalid number of arguments. {FunctionHelp}");
            }

            var tableNameExpression = arguments[ArgumentTable] as IdentifierExpression;
            var tableNameArgumentReference = arguments[ArgumentTable].Reference;
            if (tableNameExpression == null)
            {
                return ParseBuiltInFunctionsResult.Failed($"Invalid argument 1. Should be valid table name. {FunctionHelp}");
            }

            var tableName = tableNameExpression.Identifier;

            var searchValueColumnHeader = arguments[ArgumentSearchValueColumn] as IdentifierExpression;
            if (searchValueColumnHeader == null)
            {
                return ParseBuiltInFunctionsResult.Failed($"Invalid argument 3. Should be search column name. {FunctionHelp}");
            }
            var searchValueColumnHeaderName = searchValueColumnHeader.Identifier;

            if (!(arguments[ArgumentResultColumn] is IdentifierExpression resultColumnExpression))
            {
                return ParseBuiltInFunctionsResult.Failed($"Invalid argument 4. Should be lookup column name. {FunctionHelp}");
            }
            var resultColumnHeaderName = resultColumnExpression.Identifier;

            var valueExpression = arguments[ArgumentLookupValue];

            var lookupFunction = new LookupFunction(tableName, valueExpression, resultColumnHeaderName, searchValueColumnHeaderName, tableNameArgumentReference);
            return ParseBuiltInFunctionsResult.Success(lookupFunction);
        }

        public override IEnumerable<INode> GetChildren()
        {
            yield return ValueExpression;
        }

        protected override void Validate(IValidationContext context)
        {
            if (string.IsNullOrEmpty(TableType) || string.IsNullOrEmpty(ResultColumnHeaderName)) return;

            var tableType = context.Nodes.GetTable(TableType);
            if (tableType == null)
            {
                context.Logger.Fail(Reference, $"Invalid argument 1. Table name '{TableType}' not found. {FunctionHelp}");
                return;
            }

            var resultColumnHeader = tableType.Header.Get(ResultColumnHeaderName);
            if (resultColumnHeader == null)
            {
                context.Logger.Fail(Reference, $"Invalid argument 3. Column name '{ResultColumnHeaderName}' not found in table '{TableType}'. {FunctionHelp}");
                return;
            }

            var searchColumnHeader = tableType.Header.Get(SearchValueColumnHeaderName);
            if (searchColumnHeader == null)
            {
                context.Logger.Fail(Reference, $"Invalid argument 4. Column name '{SearchValueColumnHeaderName}' not found in table '{TableType}'. {FunctionHelp}");
                return;
            }

            var conditionValueType = ValueExpression.DeriveType(context);
            ResultColumnType = resultColumnHeader.Type.CreateVariableType(context);
            SearchValueColumnType = searchColumnHeader.Type.CreateVariableType(context);

            if (conditionValueType == null || !conditionValueType.Equals(SearchValueColumnType))
            {
                context.Logger.Fail(Reference, $"Invalid argument 2. Column type '{SearchValueColumnHeaderName}': '{SearchValueColumnType}' doesn't match condition type '{conditionValueType}'. {FunctionHelp}");
            }
        }

        public override VariableType DeriveReturnType(IValidationContext context)
        {
            var tableType = context.Nodes.GetTable(TableType);
            var resultColumnHeader = tableType?.Header.Get(ResultColumnHeaderName);

            return resultColumnHeader?.Type.CreateVariableType(context);
        }
    }
}