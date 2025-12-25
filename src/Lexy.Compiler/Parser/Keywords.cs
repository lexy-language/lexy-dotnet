using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lexy.Compiler.Parser;

public static class Keywords
{
    public const string Function = "function";
    public const string EnumKeyword = "enum";
    public const string TableKeyword = "table";
    public const string TypeKeyword = "type";
    public const string ScenarioKeyword = "scenario";

    public const string If = "if";
    public const string Else = "else";
    public const string ElseIf = "elseif";
    public const string Switch = "switch";
    public const string Case = "case";
    public const string Default = "default";

    public const string Include = "include";
    public const string Parameters = "parameters";
    public const string Results = "results";
    public const string ValidationTable = "validationTable";

    public const string Code = "Code";
    public const string ExpectErrors = "ExpectErrors";
    public const string ExpectComponentErrors = "ExpectComponentErrors";
    public const string ExpectExecutionErrors = "ExpectExecutionErrors";
    public const string ExecutionLogging = "ExecutionLogging";
    public const string ExecutionLog = "Log";

    public const string ImplicitVariableDeclaration = "var";

    private static readonly Lazy<IList<string>> values = new(LoadValues);

    private static IList<string> LoadValues()
    {
        return typeof(Keywords).GetFields(BindingFlags.Public | BindingFlags.Static |
                                          BindingFlags.FlattenHierarchy)
            .Where(field => field.IsLiteral && !field.IsInitOnly)
            .Select(field => (string)field.GetRawConstantValue())
            .ToList();
    }

    public static bool Contains(string keyword)
    {
        return values.Value.Contains(keyword);
    }
}