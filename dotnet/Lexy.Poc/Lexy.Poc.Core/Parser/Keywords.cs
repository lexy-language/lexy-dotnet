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
        public const string Switch = "switch";
        public const string Case = "case";
        public const string Default = "default";

        public const string For = "for";
        public const string From = "from";
        public const string To = "to";

        public const string While = "while";

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

            Switch,
            Case,
            Default,

            For,
            From,
            To,

            While,

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