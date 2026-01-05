using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser.Tokens;
using Lexy.Compiler.Specifications;
using Lexy.RunTime;
using Microsoft.Extensions.Logging;

namespace Lexy.Compiler.Compiler;

public class ExecutableFunction
{
    private record ParameterSetter(VariableType VariableType, Action<object> SetValue);

    private readonly Function function;
    private readonly ICompilationEnvironment compilationEnvironment;
    private readonly Type parametersType;

    private readonly MethodInfo runMethod;
    private readonly ILogger<ExecutionContext> executionLogger;

    public ExecutableFunction(Function function,
        Type functionType,
        ICompilationEnvironment compilationEnvironment,
        ILogger<ExecutionContext> executionLogger)
    {
        this.executionLogger = executionLogger;
        this.function = function;
        this.compilationEnvironment = compilationEnvironment;
        runMethod = functionType.GetMethod(LexyCodeConstants.RunMethod, BindingFlags.Static | BindingFlags.Public);
        parametersType = functionType.GetNestedType(LexyCodeConstants.ParametersType);
    }

    public FunctionResult Run(IDictionary<string, object> values = null)
    {
        values ??= new Dictionary<string, object>();
        ValidateValues(values);

        var parameters = CreateParameters(values);

        var context = new ExecutionContext(this.executionLogger);
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
        switch (parameter.VariableType)
        {
            case DeclaredType declaredType:
                ValidateCustomType(VariablePath(name, parameter.Name), declaredType, value, validationErrors);
                break;
            case EnumType enumType:
                ValidateEumType(VariablePath(name, parameter.Name), enumType, value, optional, validationErrors);
                break;
            case PrimitiveType primitiveType:
                ValidateType(VariablePath(name, parameter.Name), primitiveType, value, optional, validationErrors);
                break;
            case GeneratedType generatedType:
                ValidateComplexType(VariablePath(name, parameter.Name), generatedType, value, validationErrors);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unexpected variable type: '{parameter.VariableType?.GetType().Name}'");
        }
    }

    private void ValidateMember(string name, IDictionary<string, object> values, List<string> validationErrors, GeneratedTypeMember member)
    {
        var optional = false;
        var value = values.TryGetValue(member.Name, out var objectValue) ? objectValue : null;
        switch (member.Type)
        {
            case DeclaredType declaredType:
                ValidateCustomType(VariablePath(name, member.Name), declaredType, value, validationErrors);
                break;
            case EnumType enumType:
                ValidateEumType(VariablePath(name, member.Name), enumType, value, optional, validationErrors);
                break;
            case PrimitiveType primitiveType:
                ValidateType(VariablePath(name, member.Name), primitiveType, value, optional, validationErrors);
                break;
            case GeneratedType generatedType:
                ValidateComplexType(VariablePath(name, member.Name), generatedType, value, validationErrors);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unexpected variable type: '{member.Type?.GetType().Name}'");
        }
    }

    private void ValidateCustomType(string name, DeclaredType declaredType, object value, List<string> validationErrors)
    {
        if (value != null && value is not Dictionary<string, object>)
        {
            validationErrors.Add($"{name}' should have a custom type '{declaredType.Type}'. Invalid type: '{value.GetType().Name}'");
            return;
        }

        var dictionary = value != null ? (Dictionary<string, object>)value : new Dictionary<string, object>();

        foreach (var parameter in declaredType.TypeDefinition.Variables)
        {
            ValidateParameter(name, dictionary, validationErrors, parameter);
        }
    }

    private void ValidateComplexType(string name, GeneratedType generatedType, object value, List<string> validationErrors)
    {
        if (value != null && value is not Dictionary<string, object>)
        {
            validationErrors.Add($"{name}' should have a complex type '{generatedType.Name}'. Invalid type: '{value.GetType().Name}'");
            return;
        }

        var dictionary = value != null ? (Dictionary<string, object>)value : new Dictionary<string, object>();

        foreach (var member in generatedType.Members)
        {
            ValidateMember(name, dictionary, validationErrors, member);
        }
    }

    private void ValidateEumType(string name, EnumType enumType, object value, bool optional, List<string>validationErrors)
    {
        if (RunTime.Validate.IsMissing(name, value, optional, enumType.Type, validationErrors)) return;

        if (value is not string stringValue)
        {
            validationErrors.Add(
                $"'{name}' should have a '{enumType.Enum.Name.Value}' value. Invalid type: '{value.GetType().Name}'");
            return;
        }

        var parts = stringValue.Split(TokenValues.MemberAccess);
        if (parts.Length != 2 || parts[0] != enumType.Type || !enumType.Enum.ContainsMember(parts[1]))
        {
            validationErrors.Add(
                $"'{name}' should have a '{enumType.Enum.Name.Value}' value. Invalid value: '{stringValue}'");
        }
    }

    private void ValidateType(string name, PrimitiveType primitiveType, object value, bool optional,
        List<string> validationErrors)
    {
        if (RunTime.Validate.IsMissing(name, value, optional, primitiveType.Type, validationErrors)) return;

        switch (primitiveType.Type)
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
                throw new InvalidOperationException($"Invalid primitive type: '{primitiveType.Type}'");
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
            if (field.VariableType is not DeclaredType && field.VariableType is not GeneratedType)
            {
                var convertedValue = GetValue(value, field.VariableType);
                field.SetValue(convertedValue);
            }
            else
            {
                SetParameters(parameters, values[key] as IDictionary<string, object>, variablePath);
            }
        }
    }

    private static string VariablePath(string parent, string name) {
        return parent != null ? $"{parent}.{name}" : name;
    }

    private object GetValue(object value, VariableType type)
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
            currentReference = currentReference.ChildrenReference();
            currentValue = field.GetValue(currentValue);
            field = GetField(currentReference.RootIdentifier, currentValue);
            parameterType = GetTypeVariableType(parameterType, currentReference);
        }

        return new ParameterSetter(parameterType, (value) => field.SetValue(currentValue, value));
    }

    private VariableType GetFunctionParameterType(IdentifierPath currentPath)
    {
        return function.Parameters.Variables.FirstOrDefault(parameter =>
            parameter.Name == currentPath.RootIdentifier).VariableType;
    }

    private static VariableType GetTypeVariableType(VariableType parameterType, IdentifierPath currentPath)
    {
        switch (parameterType)
        {
            case DeclaredType declaredType:
                return declaredType.TypeDefinition.Variables
                    .FirstOrDefault(variable => variable.Name == currentPath.RootIdentifier).VariableType;
            case GeneratedType generatedType:
                return generatedType.Members
                    .FirstOrDefault(variable => variable.Name == currentPath.RootIdentifier)
                    .Type;
            default:
                throw new InvalidOperationException("Unexpected type: " + parameterType);
        }
    }

    private static FieldInfo? GetField(string name, object valueObject)
    {
        if (valueObject == null) throw new ArgumentNullException(nameof(valueObject));

        var type = valueObject.GetType();
        var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public);
        if (field == null)
        {
            throw new InvalidOperationException($"Couldn't find parameter field: '{name}' on type: '{type.Name}'");
        }

        return field;
    }
}