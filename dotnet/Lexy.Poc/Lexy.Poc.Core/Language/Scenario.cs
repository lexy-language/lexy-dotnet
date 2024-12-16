using System;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class Scenario : RootToken
    {
        public Comments Comments { get; } = new Comments();
        public ScenarioName Name { get; } = new ScenarioName();
        public ScenarioFunctionName FunctionName { get; } = new ScenarioFunctionName();
        public ScenarioParameters Parameters { get; } = new ScenarioParameters();
        public ScenarioResult Result { get; } = new ScenarioResult();
        public ScenarioExpectError ExpectError { get; } = new ScenarioExpectError();
        public ScenarioTable Table { get; } = new ScenarioTable();

        public override string TokenName => Name.Value;

        private Scenario(string name)
        {
            Name.ParseName(name);
        }

        internal static Scenario Parse(TokenName name)
        {
            return new Scenario(name.Parameter);
        }

        public override IToken Parse(Line line)
        {
            var name = line.FirstTokenName();
            return name switch
            {
                TokenNames.Function => FunctionName.Parse(line),
                TokenNames.Parameters => Parameters,
                TokenNames.Result => Result,
                TokenNames.Table => Table,
                TokenNames.Comment => Comments,
                TokenNames.ExpectError => ExpectError.Parse(line),
                _ => throw new InvalidOperationException($"Invalid token '{name}'. {line}")
            };
        }
    }
}
