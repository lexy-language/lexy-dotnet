using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions.Functions
{
    public class NewFunction : ExpressionFunction
    {
        public const string Name = "new";

        protected string FunctionHelp => $"{Name} expects 1 argument (Function.Parameters)";


        public MemberAccessLiteral TypeLiteral { get; }

        public Expression ValueExpression { get; }

        public ComplexTypeType Type { get; private set; }

        private NewFunction(Expression valueExpression, SourceReference reference)
            : base(reference)
        {
            ValueExpression = valueExpression ?? throw new ArgumentNullException(nameof(valueExpression));
            TypeLiteral = (valueExpression as MemberAccessExpression)?.MemberAccessLiteral;
        }

        public static ExpressionFunction Create(SourceReference reference, Expression expression) =>
            new NewFunction(expression, reference);

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

    public class Mapping
    {
        public string VariableName { get; }
        public VariableType VariableType { get; }
        public VariableSource VariableSource { get; }

        public Mapping(string variableName, VariableType variableType, VariableSource variableSource)
        {
            VariableName = variableName;
            VariableType = variableType;
            VariableSource = variableSource;
        }
    }
}