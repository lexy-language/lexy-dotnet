using System;
using System.Collections.Generic;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;

namespace Lexy.Poc.Core.Language
{
    public class Scenario : RootNode
    {
        public ScenarioName Name { get; }

        public Function Function { get; private set; }
        public EnumDefinition Enum { get; private set; }
        public Table Table { get; private set; }

        public ScenarioFunctionName FunctionName { get; }

        public ScenarioParameters Parameters { get; }
        public ScenarioResults Results { get; }
        public ScenarioTable ValidationTable { get; }

        public ScenarioExpectError ExpectError { get; }
        public ScenarioExpectRootErrors ExpectRootErrors { get; }

        public override string NodeName => Name.Value;

        private Scenario(string name, SourceReference reference) : base(reference)
        {
            Name = new ScenarioName(reference);
            FunctionName = new ScenarioFunctionName(reference);

            Parameters = new ScenarioParameters(reference);
            Results = new ScenarioResults(reference);
            ValidationTable = new ScenarioTable(reference);

            ExpectError = new ScenarioExpectError(reference);
            ExpectRootErrors = new ScenarioExpectRootErrors(reference);

            Name.ParseName(name);
        }

        internal static Scenario Parse(NodeName name, SourceReference reference)
        {
            return new Scenario(name.Name, reference);
        }

        public override IParsableNode Parse(IParserContext context)
        {
            var line = context.CurrentLine;
            if (line.IsComment())
            {
                throw new InvalidOperationException("No comments expected. Comment should be parsed by Document only.");
            }

            var name = line.Tokens.TokenValue(0);
            var reference = context.LineStartReference();
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

                Keywords.Function => ResetRootNode(context, ParseFunctionName(context)),
                Keywords.Parameters => ResetRootNode(context, Parameters),
                Keywords.Results => ResetRootNode(context, Results),
                Keywords.ValidationTable => ResetRootNode(context, ValidationTable),
                Keywords.ExpectError => ResetRootNode(context, ExpectError.Parse(context)),
                Keywords.ExpectRootErrors => ResetRootNode(context, ExpectRootErrors),

                _ => InvalidToken(context, name, reference)
            };
        }

        private IParsableNode ResetRootNode(IParserContext parserContext, IParsableNode node)
        {
            parserContext.ProcessNode(this);
            return node;
        }

        private IParsableNode ParseFunctionName(IParserContext context)
        {
            FunctionName.Parse(context);
            return this;
        }

        private IParsableNode ParseFunction(IParserContext context, SourceReference reference)
        {
            if (Function != null)
            {
                context.Logger.Fail(reference, $"Duplicated inline Function '{NodeName}'.");
                return null;
            }

            var tokenName = Parser.NodeName.Parse(context.CurrentLine, context);
            if (tokenName.Name != null)
            {
                context.Logger.Fail(context.TokenReference(1),
                    $"Unexpected function name. Inline function should not have a name: '{tokenName.Name}'");
            }

            Function = Function.Create($"{Name.Value}Function", reference);
            context.Logger.SetCurrentNode(Function);
            return Function;
        }

        private IParsableNode ParseEnum(IParserContext context, SourceReference reference)
        {
            if (Enum != null)
            {
                context.Logger.Fail(reference, $"Duplicated inline Enum '{NodeName}'.");
                return null;
            }

            var tokenName = Parser.NodeName.Parse(context.CurrentLine, context);

            Enum = EnumDefinition.Parse(tokenName, reference);
            context.Logger.SetCurrentNode(Enum);
            return Enum;
        }

        private IParsableNode ParseTable(IParserContext context, SourceReference reference)
        {
            if (Table != null)
            {
                context.Logger.Fail(reference, $"Duplicated inline Enum '{NodeName}'.");
                return null;
            }

            var tokenName = Parser.NodeName.Parse(context.CurrentLine, context);

            Table = Table.Parse(tokenName, reference);
            context.Logger.SetCurrentNode(Table);
            return Table;
        }

        private IParsableNode InvalidToken(IParserContext parserContext, string name, SourceReference reference)
        {
            parserContext.Logger.Fail(reference, $"Invalid token '{name}'.");
            return this;
        }

        public override IEnumerable<INode> GetChildren()
        {
            if (Function != null) yield return Function;
            if (Enum != null) yield return Enum;
            if (Table != null) yield return Table;

            yield return Name;
            yield return FunctionName;
            yield return Parameters;
            yield return Results;
            yield return ValidationTable;
            yield return ExpectError;
            yield return ExpectRootErrors;
        }

        protected override void ValidateNodeTree(IValidationContext context, INode child)
        {
            if (child == Parameters || child == Results)
            {
                ValidateParameterOrResultNode(context, child);
                return;
            }
            base.ValidateNodeTree(context, child);
        }

        private void ValidateParameterOrResultNode(IValidationContext context, INode child)
        {
            using (context.CreateCodeContextScope())
            {
                AddFunctionParametersAndResultsForValidation(context);
                base.ValidateNodeTree(context, child);
            }
        }

        private void AddFunctionParametersAndResultsForValidation(IValidationContext context)
        {
            var function = Function ?? (FunctionName != null ? context.Nodes.GetFunction(FunctionName.Value) : null);
            if (function == null) return;

            AddVariablesForValidation(context, function.Results.Variables);
            AddVariablesForValidation(context, function.Parameters.Variables);
        }

        private static void AddVariablesForValidation(IValidationContext context, IList<VariableDefinition> definitions)
        {
            foreach (var result in definitions)
            {
                var variableType = result.Type.CreateVariableType(context);
                context.FunctionCodeContext.AddVariable(result.Name, variableType);
            }
        }

        protected override void Validate(IValidationContext context)
        {
            if (FunctionName.IsEmpty() && Function == null && Enum == null && Table == null && !ExpectRootErrors.HasValues)
            {
                context.Logger.Fail(Reference, "Scenario as no function, enum, table or expect errors.");
            }
        }
    }
}
