using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;

namespace Lexy.Poc.Core.Language
{
    public class Scenario : RootComponent
    {
        public Comments Comments { get; } = new Comments();
        public ScenarioName Name { get; } = new ScenarioName();

        public Function Function { get; private set; }
        public ScenarioFunctionName FunctionName { get; } = new ScenarioFunctionName();

        public ScenarioParameters Parameters { get; } = new ScenarioParameters();
        public ScenarioResults Results { get; } = new ScenarioResults();
        public ScenarioTable Table { get; } = new ScenarioTable();

        public ScenarioExpectError ExpectError { get; } = new ScenarioExpectError();
        public ScenarioExpectRootErrors ExpectRootErrors { get; } = new ScenarioExpectRootErrors();

        public override string ComponentName => Name.Value;

        private Scenario(string name)
        {
            Name.ParseName(name);
        }

        internal static Scenario Parse(ComponentName name)
        {
            return new Scenario(name.Parameter);
        }

        public override IComponent Parse(IParserContext context)
        {
            var line = context.CurrentLine;
            if (line.Tokens.IsTokenType<CommentToken>(0))
            {
                return Comments;
            }

            var name = line.Tokens.TokenValue(0);
            if (!line.Tokens.IsTokenType<KeywordToken>(0))
            {
                context.Logger.Fail($"Invalid token '{name}'. Keyword expected.", this);
                return this;
            }

            return name switch
            {
                TokenValues.FunctionComponent => ParseFunction(context),
                TokenValues.Function => ResetRootComponent(context, ParseFunctionName(context)),
                TokenValues.Parameters => ResetRootComponent(context, Parameters),
                TokenValues.Results => ResetRootComponent(context, Results),
                TokenValues.Table => ResetRootComponent(context, Table),
                TokenValues.Comment => ResetRootComponent(context, Comments),
                TokenValues.ExpectError => ResetRootComponent(context, ExpectError.Parse(context)),
                TokenValues.ExpectRootErrors => ResetRootComponent(context, ExpectRootErrors),
                _ => InvalidToken(context, name)
            };
        }

        private IComponent ResetRootComponent(IParserContext parserContext, IComponent component)
        {
            parserContext.ProcessComponent(this);

            return component;
        }

        private IComponent ParseFunctionName(IParserContext context)
        {
            FunctionName.Parse(context);
            return this;
        }

        private IComponent ParseFunction(IParserContext context)
        {
            if (Function != null)
            {
                context.Logger.Fail($"Duplicated inline Function '{ComponentName}'.", this);
                return null;
            }

            var tokenName = Parser.ComponentName.Parse(context.CurrentLine, context);
            if (tokenName == null) return null;

            Function = Function.Parse(tokenName);
            context.SetCurrentComponent(Function);
            return Function;
        }

        private IComponent InvalidToken(IParserContext parserContext, string name)
        {
            parserContext.Logger.Fail($"Invalid token '{name}'.", this);
            return null;
        }
    }
}
