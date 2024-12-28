using System.Collections.Generic;
using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions
{
    public class IdentifierExpression : Expression
    {
        public VariableSource VariableSource { get; private set; }

        public string Identifier { get; }

        private IdentifierExpression(string identifier, ExpressionSource source, SourceReference reference) : base(source, reference)
        {
            Identifier = identifier;
        }

        public static ParseExpressionResult Parse(ExpressionSource source)
        {
            var tokens = source.Tokens;
            if (!IsValid(tokens))
            {
                return ParseExpressionResult.Invalid<IdentifierExpression>("Invalid expression");
            }

            var variableName = tokens.TokenValue(0);
            var reference = source.CreateReference();

            var expression = new IdentifierExpression(variableName, source, reference);

            return ParseExpressionResult.Success(expression);
        }

        public static bool IsValid(TokenList tokens)
        {
            return tokens.Length == 1
                   && tokens.IsTokenType<StringLiteralToken>(0);
        }

        public override IEnumerable<INode> GetChildren()
        {
            yield break;
        }

        protected override void Validate(IValidationContext context)
        {
            context.VariableContext.EnsureVariableExists(Reference, Identifier);

            VariableSource = context.VariableContext.GetVariableSource(Identifier) ?? VariableSource.Unknown;
        }

        public override VariableType DeriveType(IValidationContext context) =>
            context.VariableContext.GetVariableType(Identifier);
    }

}