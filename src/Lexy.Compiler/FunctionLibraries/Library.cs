using System;
using System.Collections.Generic;
using System.Reflection;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Functions;

namespace Lexy.Compiler.FunctionLibraries;

public class Library : ILibrary
{
    private readonly Type type;
    private readonly IDictionary<string, LibraryFunction> functions;

    private Library(Type type, IDictionary<string, LibraryFunction> functions)
    {
        this.type = type;
        this.functions = functions;
    }

    public IInstanceFunction GetFunction(string identifier)
    {
        return functions.TryGetValue(identifier, out var libraryFunction) ? libraryFunction : null;
    }

    public static Library Build(Type libraryType)
    {
        var staticMethods = libraryType.GetMethods(BindingFlags.Public | BindingFlags.Static);
        var functions = BuildFunctions(libraryType, staticMethods);
        return new Library(libraryType, functions);
    }

    private static IDictionary<string, LibraryFunction> BuildFunctions(Type libraryType, MethodInfo[] staticMethods)
    {
        var result = new Dictionary<string, LibraryFunction>();
        foreach (var staticMethod in staticMethods)
        {
            BuildFunction(libraryType, result, staticMethod);
        }
        return result;
    }

    private static void BuildFunction(Type libraryType, Dictionary<string, LibraryFunction> result, MethodInfo staticMethod)
    {
        if (result.ContainsKey(staticMethod.Name))
        {
            throw new InvalidOperationException(
                $"Duplicated function '{staticMethod.Name}' in library type '{libraryType.Name}'. Overloads are not supported.");
        }
        result.Add(staticMethod.Name, LibraryFunction.Build(staticMethod));
    }
}