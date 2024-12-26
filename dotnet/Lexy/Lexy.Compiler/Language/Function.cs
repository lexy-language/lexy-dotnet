using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language
{
    public class Function : RootNode
    {
        private static readonly LambdaComparer<IRootNode> NodeComparer =
            new LambdaComparer<IRootNode>((token1, token2) => token1.NodeName == token2.NodeName);

        public FunctionName Name { get; }
        public FunctionParameters Parameters { get; }
        public FunctionResults Results { get; }
        public FunctionCode Code { get; }

        public override string NodeName => Name.Value;

        public const string ParameterName = "Parameters";
        public const string ResultsName = "Results";

        private Function(string name, SourceReference reference) : base(reference)
        {
            Name = new FunctionName(reference);
            Parameters = new FunctionParameters(reference);
            Results = new FunctionResults(reference);
            Code = new FunctionCode(reference);

            Name.ParseName(name);
        }

        internal static Function Create(string name, SourceReference reference)
        {
            return new Function(name, reference);
        }

        public override IParsableNode Parse(IParserContext context)
        {
            var line = context.CurrentLine;
            if (line.IsComment())
            {
                throw new InvalidOperationException("No comments expected. Comment should be parsed by Document only.");
            }

            var name = line.Tokens.TokenValue(0);
            if (!line.Tokens.IsTokenType<KeywordToken>(0))
            {
                return InvalidToken(name, context);
            }

            return name switch
            {
                Keywords.Parameters => Parameters,
                Keywords.Results => Results,
                Keywords.Code => Code,
                 _ => InvalidToken(name, context)
            };
        }

        private IParsableNode InvalidToken(string name, IParserContext parserContext)
        {
            parserContext.Logger.Fail(Reference, $"Invalid token '{name}'.");
            return this;
        }

        public IEnumerable<IRootNode> GetDependencies(Nodes nodes)
        {
            var result = new List<IRootNode>();
            AddTableTypes(nodes, Code.Expressions, result);
            AddEnumTypes(nodes, Parameters.Variables, result);
            AddEnumTypes(nodes, Results.Variables, result);
            return result.Distinct(NodeComparer);
        }

        private void AddTableTypes(Nodes nodes, IReadOnlyList<Expression> codeExpressions, List<IRootNode> result)
        {
            NodesWalker.Walk(codeExpressions, node =>
            {
                if (node is IHasNodeDependencies hasDependencies)
                {
                    result.AddRange(hasDependencies.GetNodes(nodes));
                }
            });
        }

        private static void AddEnumTypes(Nodes nodes, IList<VariableDefinition> variableDefinitions, List<IRootNode> result)
        {
            foreach (var parameter in variableDefinitions)
            {
                if (!(parameter.Type is CustomVariableDeclarationType enumVariableType)) continue;

                var dependency = nodes.GetEnum(enumVariableType.Type);
                if (dependency == null)
                {
                    throw new InvalidOperationException($"Type or enum not found: {parameter.Type}");
                }

                result.Add(dependency);
            }
        }

        public override void ValidateTree(IValidationContext context)
        {
            using (context.CreateCodeContextScope())
            {
                base.ValidateTree(context);
            }
        }

        public override IEnumerable<INode> GetChildren()
        {
            yield return Name;

            yield return Parameters;
            yield return Results;

            yield return Code;
        }

        protected override void Validate(IValidationContext context)
        {
        }

        public ComplexType GetParametersType(IValidationContext context)
        {
            var members = Parameters.Variables
                .Select(parameter => new ComplexTypeMember(parameter.Name, parameter.Type.CreateVariableType(context)))
                .ToList();

            return new ComplexType(Name.Value, ComplexTypeSource.FunctionParameters, members);
        }

        public ComplexType GetResultsType(IValidationContext context)
        {
            var members = Results.Variables
                .Select(parameter => new ComplexTypeMember(parameter.Name, parameter.Type.CreateVariableType(context)))
                .ToList();

            return new ComplexType(Name.Value, ComplexTypeSource.FunctionResults, members);
        }
    }
}