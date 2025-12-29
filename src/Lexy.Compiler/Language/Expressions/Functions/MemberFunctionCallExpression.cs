using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class MemberFunctionCallExpression : FunctionCallExpression, IHasNodeDependencies
{
    public IdentifierPath FunctionPath { get; }
    public IReadOnlyList<Expression> Arguments { get; }
    public IInstanceFunctionCall FunctionCall { get; private set; }

    public MemberFunctionCallExpression(IdentifierPath functionPath, IReadOnlyList<Expression> arguments, ExpressionSource source) : base(source)
    {
        FunctionPath = functionPath ?? throw new ArgumentNullException(nameof(functionPath));
        Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
    }

    public IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        var component = componentNodes.GetNode(FunctionPath.RootIdentifier);
        if (component != null) yield return component;
    }

    public override IEnumerable<INode> GetChildren()
    {
        return Arguments;
    }

    protected override void Validate(IValidationContext context)
    {
        if (FunctionPath.Parts == 0)
        {
            context.Logger.Fail(Reference, $"Invalid function name: '{FunctionPath}'");
            return;
        }

        var function = GetFunction(context);
        if (function == null)
        {
            context.Logger.Fail(Reference, $"Invalid function name: '{FunctionPath}'");
            return;
        }

        var result = function.ValidateArguments(context, Arguments, Reference);
        if (!result.IsSuccess) return;

        FunctionCall = result.FunctionCall;
    }

    private IInstanceFunction GetFunction(IValidationContext context)
    {
        var variable = context.VariableContext.GetVariableType(FunctionPath.WithoutLastPart(), context);
        if (variable != null)
        {
            return GetVariableTypeFunction(context, variable);
        }

        var type = context.ComponentNodes.GetType(FunctionPath.RootIdentifier);
        if (type != null)
        {
            return GetVariableTypeFunction(context, type);
        }
        return GetLibraryFunction(context);
    }

    private IInstanceFunction GetVariableTypeFunction(IValidationContext context, VariableType variable)
    {
        return variable is not ITypeWithMembers typeWithMember
            ? null
            : typeWithMember.GetFunction(FunctionPath.LastPart());
    }

    private IInstanceFunction GetLibraryFunction(IValidationContext context)
    {
        var library = context.Libraries.GetLibrary(FunctionPath.WithoutLastPart());
        return library?.GetFunction(FunctionPath.LastPart());
    }

    public override VariableType DeriveType(IValidationContext context)
    {
        var function = GetFunction(context);
        return function?.GetResultsType(Arguments);
    }
}
