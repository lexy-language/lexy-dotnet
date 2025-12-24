using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Enums;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Tables;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Scenarios;

public class Scenario : ComponentNode, IHasNodeDependencies
{
    public ScenarioName Name { get; }

    public Function Function { get; private set; }
    public EnumDefinition Enum { get; private set; }
    public Table Table { get; private set; }

    public FunctionName FunctionName { get; private set; }

    public Parameters Parameters { get; private set; }
    public Results Results { get; private set; }
    public ValidationTable ValidationTable { get; private set; }
    public ExecutionLogging ExecutionLogging { get; private set; }

    public ExpectErrors ExpectErrors { get; private set; }
    public ExpectComponentErrors ExpectComponentErrors { get; private set; }
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

            Keywords.Function => ResetComponentNode(context, ParseFunctionName(reference, context)),
            Keywords.Parameters => ResetComponentNode(context, Parameters, () => NewParameters(reference)),
            Keywords.Results => ResetComponentNode(context, Results, () => NewResults(reference)),
            Keywords.ValidationTable => ResetComponentNode(context, ValidationTable, () => NewValidationTable(reference)),

            Keywords.ExecutionLogging => ResetComponentNode(context, ExecutionLogging, () => NewExecutionLogging(reference)),

            Keywords.ExpectErrors => ResetComponentNode(context, ExpectErrors, () => NewExpectErrors(reference)),
            Keywords.ExpectComponentErrors => ResetComponentNode(context, ExpectComponentErrors, () => NewExpectComponentErrors(reference)),
            Keywords.ExpectExecutionErrors => ResetComponentNode(context, ExpectExecutionErrors, () => NewExpectExecutionErrors(reference)),

            _ => InvalidToken(context, name, reference)
        };
    }

    private Parameters NewParameters(SourceReference reference) => Parameters = new Parameters(reference);
    private Results NewResults(SourceReference reference) => Results = new Results(reference);
    private ValidationTable NewValidationTable(SourceReference reference) => ValidationTable = new ValidationTable(Name.Value + "Table", reference);
    private ExecutionLogging NewExecutionLogging(SourceReference reference) => ExecutionLogging = new ExecutionLogging(reference);

    private ExpectErrors NewExpectErrors(SourceReference reference) => ExpectErrors = new ExpectErrors(reference);
    private ExpectComponentErrors NewExpectComponentErrors(SourceReference reference) => ExpectComponentErrors = new ExpectComponentErrors(reference);
    private ExpectExecutionErrors NewExpectExecutionErrors(SourceReference reference) => ExpectExecutionErrors = new ExpectExecutionErrors(reference);

    private IParsableNode ResetComponentNode(IParseLineContext parserContext, IParsableNode node, Func<IParsableNode> initializer = null)
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
        {
            context.Logger.Fail(context.Line.TokenReference(1),
                $"Unexpected function name. Inline function should not have a name: '{tokenName.Name}'. Remove ':' to target an existing function.");
        }

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
        if (ExpectComponentErrors != null) yield return ExpectComponentErrors;
        if (ExpectExecutionErrors != null) yield return ExpectExecutionErrors;
    }

    protected override void ValidateChild(IValidationContext context, INode child)
    {
        if (ReferenceEquals(child, Function))
        {
            base.ValidateChild(context, child);
        }
        else
        {
            ValidateWithFunctionVariables(context, child);
        }
    }

    private void ValidateWithFunctionVariables(IValidationContext context, INode child)
    {
        using (context.CreateVariableScope())
        {
            AddFunctionParametersAndResultsForValidation(context);
            base.ValidateChild(context, child);
        }
    }

    private void AddFunctionParametersAndResultsForValidation(IValidationContext context)
    {
        var function = Function ?? (FunctionName != null ? context.ComponentNodes.GetFunction(FunctionName.Value) : null);
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
            && (ExpectComponentErrors == null || !ExpectComponentErrors.HasValues))
        {
            context.Logger.Fail(Reference, "Scenario has no function, enum, table or expect errors.");
        }
    }

    public IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodeList)
    {
        var result = new List<IComponentNode>();
        if (Function != null) result.Add(Function);
        if (FunctionName?.IsEmpty() == false)
        {
            var functionNode = componentNodeList.GetFunction(FunctionName.Value);
            if (functionNode != null) {
                result.Add(functionNode);
            }
        }
        if (Enum != null) result.Add(Enum);
        if (Table != null) result.Add(this.Table);
        return result;
    }
}