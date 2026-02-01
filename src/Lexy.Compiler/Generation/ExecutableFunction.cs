using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Lexy.Compiler.Parser.Tokens;
using Lexy.Compiler.Specifications;
using Lexy.RunTime;
using Microsoft.Extensions.Logging;
using Type = Lexy.Compiler.Language.TypeSystem.Type;
using ValueType = Lexy.Compiler.Language.TypeSystem.ValueType;

namespace Lexy.Compiler.Generation;

public class ExecutableFunction
{
    private record ParameterSetter(Type Type, Action<object> SetValue);

    private readonly Function function;
    private readonly ICompilationEnvironment compilationEnvironment;
    private readonly System.Type parametersType;

    private readonly MethodInfo runMethod;
    private readonly ILogger<ExecutionContext> executionLogger;

    public ExecutableFunction(Function function,
        System.Type functionType,
        ICompilationEnvironment compilationEnvironment,
        ILogger<ExecutionContext> executionLogger)
    {
        this.executionLogger = executionLogger;
        this.function = function;
        this.compilationEnvironment = compilationEnvironment;
        var methodInfos = functionType.GetMethods(BindingFlags.Static | BindingFlags.Public);
        runMethod = methodInfos.FirstOrDefault(method => IsDefaultRunMethod(functionType, method));
        parametersType = functionType.GetNestedType(LexyCodeConstants.ParametersType);
    }

    private static bool IsDefaultRunMethod(System.Type functionType, MethodInfo method)
    {
        if (method.Name != LexyCodeConstants.RunMethod) return false;

        var parameters = method.GetParameters();
        if (parameters.Length != 2) return false;

        var parameterType = parameters[0].ParameterType;
        return parameterType.IsNested
            && parameterType.DeclaringType == functionType
            && parameterType.Name == LexyCodeConstants.ParametersType;
    }

    public FunctionResult Run(IDictionary<string, object> values = null)
    {
        values ??= new Dictionary<string, object>();
        ValidateValues(values);

        var parameters = CreateParameters(values);

        var context = new ExecutionContext(executionLogger);
        var results = runMethod.Invoke(null, new[] { parameters, context });

        return new FunctionResult(results, context.Entries);
    }

    private void ValidateValues(IDictionary<string,object> values)
    {
        var validationErrors = new List<string>();
        ValidateParameters(values, validationErrors);

        if (validationErrors.Count > 0)
        {
            throw new InvalidOperationException("Validation failed: \n" + validationErrors.Format(2));
        }
    }

    private void ValidateParameters(IDictionary<string,object> values, List<string> validationErrors)
    {
        if (function.Parameters == null) return;

        var variables = function.Parameters.Variables;
        Validate(values, validationErrors, variables);
    }

    private void Validate(IDictionary<string, object> values, List<string> validationErrors, IReadOnlyList<VariableDefinition> variables)
    {
        foreach (var parameter in variables)
        {
            ValidateParameter(null, values, validationErrors, parameter);
        }
    }

    private void ValidateParameter(string name, IDictionary<string, object> values, List<string> validationErrors, VariableDefinition parameter)
    {
        var optional = parameter.DefaultExpression != null;
        var value = values.TryGetValue(parameter.Name, out var objectValue) ? objectValue : null;
        switch (parameter.State.Type)
        {
            case EnumType enumType:
                ValidateEumType(VariablePath(name, parameter.Name), enumType, value, optional, validationErrors);
                break;
            case ValueType valueType:
                ValidateType(VariablePath(name, parameter.Name), valueType, value, optional, validationErrors);
                break;
            case DeclaredType declaredType:
                ValidateCustomType(VariablePath(name, parameter.Name), declaredType, value, validationErrors);
                break;
            case GeneratedType generatedType:
                ValidateObjectType(VariablePath(name, parameter.Name), generatedType, value, validationErrors);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unexpected variable type: '{parameter.State.Type?.GetType().Name}'");
        }
    }

    private void ValidateMember(string name, IDictionary<string, object> values, List<string> validationErrors, IObjectMember variable)
    {
        var optional = false;
        var value = values.TryGetValue(variable.Name, out var objectValue) ? objectValue : null;
        switch (variable.Type)
        {
            case DeclaredType declaredType:
                ValidateCustomType(VariablePath(name, variable.Name), declaredType, value, validationErrors);
                break;
            case EnumType enumType:
                ValidateEumType(VariablePath(name, variable.Name), enumType, value, optional, validationErrors);
                break;
            case ValueType valueType:
                ValidateType(VariablePath(name, variable.Name), valueType, value, optional, validationErrors);
                break;
            case GeneratedType generatedType:
                ValidateObjectType(VariablePath(name, variable.Name), generatedType, value, validationErrors);
                break;
            default:
                throw new InvalidOperationException($"Unexpected variable type: '{variable.Type?.GetType().Name}'");
        }
    }

    private void ValidateCustomType(string name, DeclaredType declaredType, object value, List<string> validationErrors)
    {
        if (value != null && value is not Dictionary<string, object>)
        {
            validationErrors.Add($"{name}' should have a custom type '{declaredType.Name}'. Invalid type: '{value.GetType().Name}'");
            return;
        }

        var dictionary = value != null ? (Dictionary<string, object>)value : new Dictionary<string, object>();

        foreach (var parameter in declaredType.TypeDefinition.Variables)
        {
            ValidateParameter(name, dictionary, validationErrors, parameter);
        }
    }

    private void ValidateObjectType(string name, ObjectType objectType, object value, List<string> validationErrors)
    {
        if (value != null && value is not Dictionary<string, object>)
        {
            validationErrors.Add($"{name}' should have a object type '{objectType.Name}'. Invalid type: '{value.GetType().Name}'");
            return;
        }

        var dictionary = value != null ? (Dictionary<string, object>)value : new Dictionary<string, object>();

        foreach (var member in objectType.Members)
        {
            ValidateMember(name, dictionary, validationErrors, member);
        }
    }

    private void ValidateEumType(string name, EnumType enumType, object value, bool optional, List<string>validationErrors)
    {
        if (RunTime.Validate.IsMissing(name, value, optional, enumType.Name, validationErrors)) return;

        if (value is not string stringValue)
        {
            validationErrors.Add(
                $"'{name}' should have a '{enumType.Name}' value. Invalid type: '{value.GetType().Name}'");
            return;
        }

        var parts = stringValue.Split(TokenValues.MemberAccess);
        if (parts.Length != 2 || parts[0] != enumType.Name || !enumType.ContainsMember(parts[1]))
        {
            validationErrors.Add(
                $"'{name}' should have a '{enumType.Name}' value. Invalid value: '{stringValue}'");
        }
    }

    private void ValidateType(string name, ValueType valueType, object value, bool optional,
        List<string> validationErrors)
    {
        if (RunTime.Validate.IsMissing(name, value, optional, valueType.Name, validationErrors)) return;

        switch (valueType.Name)
        {
            case TypeNames.String:
                RunTime.Validate.String(name, value, optional, validationErrors);
                return;

            case TypeNames.Number:
                RunTime.Validate.Number(name, value, optional, validationErrors);
                return;

            case TypeNames.Boolean:
                RunTime.Validate.Boolean(name, value, optional, validationErrors);
                return;

            case TypeNames.Date:
                RunTime.Validate.Date(name, value, optional, validationErrors);
                return;

            default:
                throw new InvalidOperationException($"Invalid value type: '{valueType.Name}'");
        }
    }

    private object CreateParameters(IDictionary<string, object> values)
    {
        var parameters = CreateParameters();
        SetParameters(parameters, values, null);
        return parameters;
    }

    private void SetParameters(object parameters, IDictionary<string, object> values, string parent)
    {
        foreach (var (key, value) in values)
        {
            var variablePath = VariablePath(parent, key);
            var field = GetParameterSetter(parameters, variablePath);
            if (field.Type is not DeclaredType && field.Type is not GeneratedType)
            {
                var convertedValue = GetValue(value, field.Type);
                field.SetValue(convertedValue);
            }
            else
            {
                SetParameters(parameters, values[key] as IDictionary<string, object>, variablePath);
            }
        }
    }

    private static string VariablePath(string parent, string name)
    {
        return parent != null ? $"{parent}.{name}" : name;
    }

    private object GetValue(object value, Type type)
    {
        return TypeConverter.Convert(compilationEnvironment, value, type);
    }

    private object CreateParameters() => Activator.CreateInstance(parametersType);

    private ParameterSetter GetParameterSetter(object parameters, string name)
    {
        var currentReference = IdentifierPath.Parse(name);
        var currentValue = parameters;
        var field = GetField(currentReference.RootIdentifier, parameters);
        var parameterType = GetFunctionParameterType(currentReference);

        while (currentReference.HasChildIdentifiers)
        {
            currentReference = currentReference.ChildrenPath();
            currentValue = field.GetValue(currentValue);
            field = GetField(currentReference.RootIdentifier, currentValue);
            parameterType = GetParameterType(parameterType, currentReference);
        }

        return new ParameterSetter(parameterType, (value) => field.SetValue(currentValue, value));
    }

    private Type GetFunctionParameterType(IdentifierPath currentPath)
    {
        return function.Parameters.Variables
            .FirstOrDefault(parameter => parameter.Name == currentPath.RootIdentifier)
            .State.Type;
    }

    private static Type GetParameterType(Type parameterType, IdentifierPath currentPath)
    {
        if (parameterType is not ObjectType objectType)
        {
            throw new InvalidOperationException("Unexpected type: " + parameterType);
        }

        return objectType.MemberType(currentPath.RootIdentifier);
    }

    private static FieldInfo GetField(string name, object valueObject)
    {
        Assert.NotNull(valueObject, nameof(valueObject));

        var type = valueObject.GetType();
        var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public);
        if (field == null)
        {
            throw new InvalidOperationException($"Couldn't find parameter field: '{name}' on type: '{type.Name}'");
        }

        return field;
    }
}
