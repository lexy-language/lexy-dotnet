using System.Collections.Generic;
using Lexy.Compiler.Language.TypeSystem.Functions;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Symbols;
using Lexy.RunTime;
using Type = Lexy.Compiler.Language.TypeSystem.Type;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class MemberFunctionCallExpression : FunctionCallExpression, IHasNodeDependencies
{
    public IdentifierPath FunctionPath { get; }
    public IReadOnlyList<Expression> Arguments { get; }
    public IFunctionCallState State { get; private set; }

    public override string Name => FunctionPath.LastPart();

    public MemberFunctionCallExpression(IdentifierPath functionPath, IReadOnlyList<Expression> arguments, ExpressionSource source) : base(source)
    {
        FunctionPath = Assert.NotNull(functionPath, nameof(functionPath));
        Arguments = Assert.NotNull(arguments, nameof(arguments));
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
        if (!result.IsSuccess)
        {
            return;
        }

        State = result.FunctionCallState;
    }

    private IObjectFunction GetFunction(IValidationContext context)
    {
        var variable = context.VariableContext.GetType(FunctionPath.WithoutLastPart());
        if (variable != null)
        {
            return GetTypeFunction(context, variable);
        }

        var type = context.ComponentNodes.GetType(FunctionPath.RootIdentifier);
        if (type != null)
        {
            return GetTypeFunction(context, type);
        }
        return GetLibraryFunction(context);
    }

    private IObjectFunction GetTypeFunction(IValidationContext context, Type variable)
    {
        return variable is not ObjectType typeWithMember
            ? null
            : typeWithMember.GetFunction(FunctionPath.LastPart());
    }

    private IObjectFunction GetLibraryFunction(IValidationContext context)
    {
        var library = context.Libraries.GetLibrary(FunctionPath.WithoutLastPart());
        return library?.GetFunction(FunctionPath.LastPart());
    }

    public override Type DeriveType(IValidationContext context)
    {
        var function = GetFunction(context);
        return function?.GetResultsType(Arguments);
    }

    public override Symbol GetSymbol()
    {
        return State?.GetSymbol();
    }
}
