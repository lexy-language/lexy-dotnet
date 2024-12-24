using System.Collections.Generic;

namespace Lexy.Poc.Core.Parser
{
    public static class Keywords
    {
        public const string FunctionKeyword = "Function:";
        public const string EnumKeyword = "Enum:";
        public const string TableKeyword = "Table:";
        public const string ScenarioKeyword = "Scenario:";

        public const string Function = "Function";
        public const string ValidationTable = "ValidationTable";

        public const string If = "if";
        public const string Else = "else";

        public const string Include = "Include";
        public const string Parameters = "Parameters";
        public const string Results = "Results";
        public const string Code = "Code";
        public const string ExpectError = "ExpectError";
        public const string ExpectRootErrors = "ExpectRootErrors";

        public const string Comment = "#";

        private static readonly IList<string> values = new List<string>
        {
            FunctionKeyword,
            EnumKeyword,
            TableKeyword,
            ScenarioKeyword,

            Function,
            ValidationTable,

            If,
            Else,

            Include,
            Parameters,
            Results,
            Code,
            ExpectError,
            ExpectRootErrors,

            Comment,
        };

        public static bool Contains(string keyword) => values.Contains(keyword);
    }
}