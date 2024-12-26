using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions.Functions
{
    public class FillParametersFunction : ExpressionFunction
    {
        public const string Name = "fill";

        private readonly IList<Mapping> mapping = new List<Mapping>();

        protected string FunctionHelp => $"{Name} expects 1 argument (Function.Parameters)";

        public MemberAccessLiteral TypeLiteral { get; }

        public Expression ValueExpression { get; }

        public ComplexTypeType Type { get; private set; }

        public IEnumerable<Mapping> Mapping => mapping;

        private FillParametersFunction(Expression valueExpression, SourceReference reference)
            : base(reference)
        {
            ValueExpression = valueExpression ?? throw new ArgumentNullException(nameof(valueExpression));
            TypeLiteral = (valueExpression as MemberAccessExpression)?.MemberAccessLiteral;
        }

        public static ExpressionFunction Create(SourceReference reference, Expression expression) =>
            new FillParametersFunction(expression, reference);

        public override IEnumerable<INode> GetChildren()
        {
            yield return ValueExpression;
        }

        protected override void Validate(IValidationContext context)
        {
            var valueType = ValueExpression.DeriveType(context);
            if (!(valueType is ComplexTypeType complexTypeType))
            {
                context.Logger.Fail(Reference,
                    $"Invalid argument 1. 'Value' should be of type 'ComplexTypeType' but is 'ValueType'. {FunctionHelp}");
                return;
            }

            Type = complexTypeType;

            var complexType = complexTypeType.GetComplexType(context);

            if (complexType == null) return;

            GetMapping(Reference, context, complexType, mapping);
        }

        internal static void GetMapping(SourceReference reference, IValidationContext context, ComplexType complexType, IList<Mapping> mapping)
        {
            if (reference == null) throw new ArgumentNullException(nameof(reference));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));

            if (complexType == null) return;

            foreach (var member in complexType.Members)
            {
                var variable = context.FunctionCodeContext.GetVariable(member.Name);
                if (variable == null) continue;

                if (!variable.VariableType.Equals(member.Type))
                {
                    context.Logger.Fail(reference,
                        $"Invalid parameter mapping. Variable '{member.Name}' of type '{variable.VariableType}' can't be mapped to parameter '{member.Name}' of type '{member.Type}'.");
                }
                else
                {
                    mapping.Add(new Mapping(member.Name, variable.VariableType, variable.VariableSource));
                }
            }

            if (mapping.Count == 0)
            {
                context.Logger.Fail(reference,
                    $"Invalid parameter mapping. No parameter could be mapped from variables.");
            }
        }

        public override VariableType DeriveReturnType(IValidationContext context)
        {
            var function = context.Nodes.GetFunction(TypeLiteral.Parent);
            if (TypeLiteral.Member == Function.ParameterName)
            {
                return function.GetParametersType(context);
            }

            if (TypeLiteral.Member == Function.ResultsName)
            {
                return function.GetResultsType(context);
            }

            return null;
        }
    }
}