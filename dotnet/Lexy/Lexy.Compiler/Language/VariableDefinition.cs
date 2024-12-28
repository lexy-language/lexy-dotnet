using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language
{
    public class VariableDefinition : Node, IHasNodeDependencies
    {
        public Expression DefaultExpression{ get; }
        public VariableSource Source { get; }
        public VariableDeclarationType Type { get; }
        public VariableType VariableType { get; private set; }
        public string Name { get; }

        private VariableDefinition(string name, VariableDeclarationType type,
            VariableSource Source, SourceReference reference, Expression defaultExpression = null) : base(reference)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Name = name ?? throw new ArgumentNullException(nameof(name));

            DefaultExpression = defaultExpression;
            this.Source = Source;
        }

        public static VariableDefinition Parse(VariableSource source, IParserContext context)
        {
            var line = context.CurrentLine;
            if (line.IsEmpty()) return null;

            var result = context.ValidateTokens<VariableDefinition>()
                .CountMinimum(2)
                .StringLiteral(0)
                .StringLiteral(1)
                .IsValid;

            if (!result) return null;

            var tokens = line.Tokens;
            var name = tokens.TokenValue(1);
            var type = tokens.TokenValue(0);

            var variableType = VariableDeclarationType.Parse(type, context.TokenReference(0));
            if (variableType == null) return null;

            if (tokens.Length == 2)
            {
                return new VariableDefinition(name, variableType, source, context.LineStartReference());
            }

            if (tokens.Token<OperatorToken>(2).Type != OperatorType.Assignment)
            {
                context.Logger.Fail(context.TokenReference(2), "Invalid variable declaration token. Expected '='.");
                return null;
            }

            if (tokens.Length != 4)
            {
                context.Logger.Fail(context.LineEndReference(),
                    "Invalid variable declaration. Expected literal token.");
                return null;
            }

            var defaultValue = ExpressionFactory.Parse(context.SourceCode.File, tokens.TokensFrom(3), line);
            if (context.Failed(defaultValue, context.TokenReference(3))) return null;

            return new VariableDefinition(name, variableType, source, context.LineStartReference(), defaultValue.Result);
        }

        public override IEnumerable<INode> GetChildren()
        {
            if (DefaultExpression != null)
            {
                yield return DefaultExpression;
            }
            yield return Type;
        }

        protected override void Validate(IValidationContext context)
        {
            VariableType = Type.CreateVariableType(context);

            context.VariableContext.RegisterVariableAndVerifyUnique(Reference, Name, VariableType, Source);

            context.ValidateTypeAndDefault(Reference, Type, DefaultExpression);
        }

        public IEnumerable<IRootNode> GetDependencies(RootNodeList rootNodeList)
        {
            if (VariableType is not CustomType customType)
            {
                yield break;
            }

            yield return customType.TypeDefinition;
        }
    }
}