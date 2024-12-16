using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class Function : RootToken
    {
        public Comments Comments { get; } = new Comments();
        public FunctionName Name { get; } = new FunctionName();
        public FunctionParameters Parameters { get; } = new FunctionParameters();
        public FunctionResult Result { get; } = new FunctionResult();
        public FunctionCode Code { get; } = new FunctionCode();
        public FunctionIncludes Include { get; } = new FunctionIncludes();
        public override string TokenName => Name.Value;

        private Function(string name)
        {
            Name.ParseName(name);
        }

        internal static Function Parse(TokenName name)
        {
            return new Function(name.Parameter);
        }

        public override IToken Parse(Line line)
        {
            var name = line.FirstTokenName();
            return name switch
            {
                TokenNames.Parameters => Parameters,
                TokenNames.Result => Result,
                TokenNames.Code => Code,
                TokenNames.Include => Include,
                TokenNames.Comment => Comments,
                _ => throw new InvalidOperationException($"Invalid token '{name}'. {line}")
            };
        }

        public IEnumerable<IRootToken> GetDependencies(TypeSystem typeSystem)
        {
            var result = new List<IRootToken>();
            AddEnumTypes(typeSystem, Parameters.Variables, result);
            AddEnumTypes(typeSystem, Result.Variables, result);
            return result.Distinct(new LambdaComparer<IRootToken>((token1, token2) => token1.TokenName == token2.TokenName));
        }

        private static void AddEnumTypes(TypeSystem typeSystem, IList<VariableDefinition> variableDefinitions, List<IRootToken> result)
        {
            foreach (var parameter in variableDefinitions)
            {
                if (!TypeNames.Exists(parameter.Type))
                {
                    var dependency = typeSystem.GetEnum(parameter.Type);
                    if (dependency == null)
                    {
                        throw new InvalidOperationException("Type or enum not found: " + parameter.Type);
                    }

                    result.Add(dependency);
                }
            }
        }
    }
}