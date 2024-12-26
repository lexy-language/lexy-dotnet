using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language
{
    public class VariableDefinition : Node
    {
        public ILiteralToken Default { get; }
        public VariableSource Source { get; }
        public VariableDeclarationType Type { get; }
        public string Name { get; }

        private VariableDefinition(string name, VariableDeclarationType type,
            VariableSource Source, SourceReference reference, ILiteralToken @default = null) : base(reference)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Name = name ?? throw new ArgumentNullException(nameof(name));

            Default = @default;
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

            var variableType = VariableDeclarationType.Parse(type);
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

            var defaultValue = tokens.LiteralToken(3);
            return new VariableDefinition(name, variableType, source, context.LineStartReference(), defaultValue);
        }

        public override IEnumerable<INode> GetChildren()
        {
            yield break;
        }

        protected override void Validate(IValidationContext context)
        {
            var variableType = Type.CreateVariableType(context);

            context.FunctionCodeContext.RegisterVariableAndVerifyUnique(Reference, Name, variableType, Source);

            context.ValidateTypeAndDefault(Reference, Type, Default);
        }
    }
}