using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Enums;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Tables;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Scenarios;

public class Scenario : RootNode
{
    public ScenarioName Name { get; }

    public Function Function { get; private set; }
    public EnumDefinition Enum { get; private set; }
    public Table Table { get; private set; }

    public FunctionName FunctionName { get; private set; }

    public Parameters Parameters { get; private set; }
    public Results Results { get; private set; }
    public Table ValidationTable { get; private set; }
    public ExecutionLogging ExecutionLogging { get; private set; }

    public ExpectErrors ExpectErrors { get; private set; }
    public ExpectRootErrors ExpectRootErrors { get; private set; }
    public ExpectExecutionErrors ExpectExecutionErrors { get; private set; }

    public override string NodeName => Name.Value;

    private Scenario(string name, SourceReference reference) : base(reference)
    {
        Name = new ScenarioName(name, reference);
    }

    internal static Scenario Parse(NodeName name, SourceReference reference)
    {
        return new Scenario(name.Name, reference);
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        var line = context.Line;
        var name = line.Tokens.TokenValue(0);
        var reference = line.LineStartReference();
        if (!line.Tokens.IsTokenType<KeywordToken>(0))
        {
            context.Logger.Fail(reference, $"Invalid token '{name}'. Keyword expected.");
            return this;
        }

        return name switch
        {
            Keywords.FunctionKeyword => ParseFunction(context, reference),
            Keywords.EnumKeyword => ParseEnum(context, reference),
            Keywords.TableKeyword => ParseTable(context, reference),

            Keywords.Function => ResetRootNode(context, ParseFunctionName(reference, context)),
            Keywords.Parameters => ResetRootNode(context, Parameters, () => Parameters = new Parameters(reference)),
            Keywords.Results => ResetRootNode(context, Results, () => Results = new Results(reference)),
            Keywords.ValidationTable => ParseValidationTable(context, reference),

            Keywords.ExecutionLogging => ResetRootNode(context, ExecutionLogging, () => ExecutionLogging = new ExecutionLogging(reference)),

            Keywords.ExpectErrors => ResetRootNode(context, ExpectErrors, () => ExpectErrors = new ExpectErrors(reference)),
            Keywords.ExpectRootErrors => ResetRootNode(context, ExpectRootErrors, () => ExpectRootErrors = new ExpectRootErrors(reference)),
            Keywords.ExpectExecutionErrors => ResetRootNode(context, ExpectExecutionErrors, () => ExpectExecutionErrors = new ExpectExecutionErrors(reference)),

            _ => InvalidToken(context, name, reference)
        };
    }

    private IParsableNode ResetRootNode(IParseLineContext parserContext, IParsableNode node, Func<IParsableNode> initializer = null)
    {
        if (node == null)
        {
            if (initializer == null)
            {
                throw new InvalidOperationException("node should not be null");
            }

            node = initializer();
        }
        parserContext.Logger.SetCurrentNode(this);
        return node;
    }

    private IParsableNode ParseFunctionName(SourceReference reference, IParseLineContext context)
    {
        if (FunctionName == null)
        {
            FunctionName = new FunctionName(reference);
        }
        FunctionName.Parse(context);
        return this;
    }

    private IParsableNode ParseFunction(IParseLineContext context, SourceReference reference)
    {
        if (Function != null)
        {
            context.Logger.Fail(reference, $"Duplicated inline Function '{NodeName}'.");
            return null;
        }

        var tokenName = Parser.NodeName.Parse(context);
        if (tokenName.Name != null)
            context.Logger.Fail(context.Line.TokenReference(1),
                $"Unexpected function name. Inline function should not have a name: '{tokenName.Name}'");

        Function = Function.Create($"{Name.Value}Function", reference, context.ExpressionFactory);
        context.Logger.SetCurrentNode(Function);
        return Function;
    }

    private IParsableNode ParseEnum(IParseLineContext context, SourceReference reference)
    {
        if (Enum != null)
        {
            context.Logger.Fail(reference, $"Duplicated inline Enum '{NodeName}'.");
            return null;
        }

        var tokenName = Parser.NodeName.Parse(context);

        Enum = EnumDefinition.Parse(tokenName, reference);
        context.Logger.SetCurrentNode(Enum);
        return Enum;
    }

    private IParsableNode ParseTable(IParseLineContext context, SourceReference reference)
    {
        if (Table != null)
        {
            context.Logger.Fail(reference, $"Duplicated inline table '{NodeName}'.");
            return null;
        }

        var tokenName = Parser.NodeName.Parse(context);

        Table = new Table(tokenName.Name, reference);
        context.Logger.SetCurrentNode(Table);
        return Table;
    }

    private IParsableNode ParseValidationTable(IParseLineContext context, SourceReference reference)
    {
        if (ValidationTable != null)
        {
            context.Logger.Fail(reference, $"Duplicated validation table '{NodeName}'.");
            return null;
        }

        var tokenName = Parser.NodeName.Parse(context);
        if (tokenName.Name != null)
        {
            context.Logger.Fail(context.Line.TokenReference(1),
                $"Unexpected table name. 'ValidationTable' should not have a name: '{tokenName.Name}'");
        }

        var tableName = Name?.Value != null ? Name?.Value  + "ValidationTable" : "ValidationTable";
        ValidationTable = new Table(tableName, reference);
        context.Logger.SetCurrentNode(ValidationTable);
        return ValidationTable;
    }

    private IParsableNode InvalidToken(IParseLineContext context, string name, SourceReference reference)
    {
        context.Logger.Fail(reference, $"Invalid token '{name}'.");
        return this;
    }

    public override IEnumerable<INode> GetChildren()
    {
        if (Function != null) yield return Function;
        if (Enum != null) yield return Enum;
        if (Table != null) yield return Table;

        yield return Name;

        if (FunctionName != null) yield return FunctionName;
        if (Parameters != null) yield return Parameters;
        if (Results != null) yield return Results;
        if (ValidationTable != null) yield return ValidationTable;
        if (ExpectErrors != null) yield return ExpectErrors;
        if (ExpectRootErrors != null) yield return ExpectRootErrors;
        if (ExpectExecutionErrors != null) yield return ExpectExecutionErrors;
    }

    protected override void ValidateNodeTree(IValidationContext context, INode child)
    {
        if (ReferenceEquals(child, Parameters) || ReferenceEquals(child, Results))
        {
            ValidateParameterOrResultNode(context, child);
            return;
        }

        base.ValidateNodeTree(context, child);
    }

    private void ValidateParameterOrResultNode(IValidationContext context, INode child)
    {
        using (context.CreateVariableScope())
        {
            AddFunctionParametersAndResultsForValidation(context);
            base.ValidateNodeTree(context, child);
        }
    }

    private void AddFunctionParametersAndResultsForValidation(IValidationContext context)
    {
        var function = Function ?? (FunctionName != null ? context.RootNodes.GetFunction(FunctionName.Value) : null);
        if (function == null) return;

        AddVariablesForValidation(context, function.Parameters.Variables, VariableSource.Parameters);
        AddVariablesForValidation(context, function.Results.Variables, VariableSource.Results);
    }

    private static void AddVariablesForValidation(IValidationContext context, IReadOnlyList<VariableDefinition> definitions,
        VariableSource source)
    {
        if (definitions == null) return;

        foreach (var result in definitions)
        {
            var variableType = result.Type.VariableType;
            context.VariableContext.AddVariable(result.Name, variableType, source);
        }
    }

    protected override void Validate(IValidationContext context)
    {
        if ((FunctionName == null || FunctionName.IsEmpty())
            && Function == null
            && Enum == null
            && Table == null
            && (ExpectRootErrors == null || !ExpectRootErrors.HasValues))
        {
            context.Logger.Fail(Reference, "Scenario has no function, enum, table or expect errors.");
        }
    }
}