using System.Collections.Generic;
using Lexy.Compiler.Language.Enums;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Language.Tables;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Scenarios;

public class Scenario : ComponentNode, IHasNodeDependencies
{
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

    private Scenario(string name, LexyScriptNode parentReference, SourceReference reference) : base(name, new NodeReference(parentReference), reference)
    {
    }

    internal static Scenario Parse(NodeName name, LexyScriptNode parent, SourceReference reference)
    {
        return new Scenario(name.Name, parent, reference);
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        var line = context.Line;
        var name = line.Tokens.TokenValue(0);
        var reference = line.Tokens.AllReference();
        if (!line.Tokens.IsTokenType<KeywordToken>(0))
        {
            context.Logger.Fail(reference, $"Invalid token '{name}'. Keyword expected.");
            return this;
        }

        return name switch
        {
            Keywords.Function => ParseFunction(context, reference),
            Keywords.EnumKeyword => ParseEnum(context, reference),
            Keywords.TableKeyword => ParseTable(context, reference),

            Keywords.Parameters => Parameters = new Parameters(this, reference),
            Keywords.Results => Results = new Results(this, reference),
            Keywords.ValidationTable => ValidationTable = new ValidationTable($"{Name}Table", this, reference),

            Keywords.ExecutionLogging => ExecutionLogging = new ExecutionLogging(this, reference),

            Keywords.ExpectErrors => ExpectErrors = new ExpectErrors(this, reference),
            Keywords.ExpectComponentErrors => ExpectComponentErrors = new ExpectComponentErrors(this, reference),
            Keywords.ExpectExecutionErrors => ExpectExecutionErrors = new ExpectExecutionErrors(this, reference),

            _ => InvalidToken(context, name, reference)
        };
    }

    private IParsableNode ParseFunction(IParseLineContext context, SourceReference reference)
    {
        if (Function != null)
        {
            context.Logger.Fail(reference, $"Duplicated inline Function '{Name}'.");
            return null;
        }

        var tokenName = NodeName.Parse(context);
        if (tokenName.Name != null)
        {
            return ParseFunctionName(context, reference);
        }

        Function = Function.Create($"{Name}Function", true, new NodeReference(this), reference, context.ExpressionFactory);
        context.Logger.SetCurrentNode(Function);
        return Function;
    }

    private IParsableNode ParseFunctionName(IParseLineContext context, SourceReference reference)
    {
        FunctionName = FunctionName.Parse(context, this, reference);

        return this;
    }

    private IParsableNode ParseEnum(IParseLineContext context, SourceReference reference)
    {
        if (Enum != null)
        {
            context.Logger.Fail(reference, $"Duplicated inline Enum '{Name}'.");
            return null;
        }

        var tokenName = NodeName.Parse(context);

        Enum = EnumDefinition.Parse(tokenName.Name, true, this, reference);
        context.Logger.SetCurrentNode(Enum);
        return Enum;
    }

    private IParsableNode ParseTable(IParseLineContext context, SourceReference reference)
    {
        if (Table != null)
        {
            context.Logger.Fail(reference, $"Duplicated inline table '{Name}'.");
            return null;
        }

        var tokenName = NodeName.Parse(context);

        Table = new Table(tokenName.Name, new NodeReference(this), reference);
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
        context.InNodeVariableScope(this, _ =>
        {
            AddFunctionParametersAndResultsForValidation(context);
            base.ValidateChild(context, child);
        });
    }

    private void AddFunctionParametersAndResultsForValidation(IValidationContext context)
    {
        var function = Function ?? (FunctionName != null ? context.ComponentNodes.GetFunction(FunctionName.Value) : null);
        if (function == null) return;
        function.AddParametersAndResultsForVariables(context);

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

    public IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        var result = new List<IComponentNode>();
        if (Function != null) result.Add(Function);
        if (FunctionName?.IsEmpty() == false)
        {
            var functionNode = componentNodes.GetFunction(FunctionName.Value);
            if (functionNode != null)
            {
                result.Add(functionNode);
            }
        }
        if (Enum != null) result.Add(Enum);
        if (Table != null) result.Add(Table);
        return result;
    }

    public override Symbol GetSymbol()
    {
        return new Symbol(Reference, "scenario: " + Name, "Test scenario", SymbolKind.Scenario);
    }

    public override SuggestionEdit[] GetSuggestions()
    {
        return Suggestions.Edit(with => with
            .Keyword(Keywords.Parameters)
            .Keyword(Keywords.Results)
            .Keyword(Keywords.ValidationTable)
            //Omit system language keywords (Expect..., Execute...)
        );
    }
}
