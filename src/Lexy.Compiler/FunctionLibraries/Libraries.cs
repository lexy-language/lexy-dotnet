using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.FunctionLibraries;

public class Libraries : ILibraries
{
    private readonly IDictionary<string, ILibrary> libraries = new Dictionary<string, ILibrary>();

    public Libraries(params Type[] libraryTypes)
    {
        AddSystemLibraries();

        libraryTypes.ForEach(AddLibrary);
    }

    private void AddSystemLibraries()
    {
        AddLibrary(typeof(Lexy.RunTime.Libraries.Date));
        AddLibrary(typeof(Lexy.RunTime.Libraries.Math));
        AddLibrary(typeof(Lexy.RunTime.Libraries.Number));
    }

    private void AddLibrary(Type libraryType)
    {
        var library = Library.Build(libraryType);
        libraries.Add(libraryType.Name, library);
    }

    public ILibrary GetLibrary(IdentifierPath identifier)
    {
        return libraries.TryGetValue(identifier.ToString(), out var library) ? library : null;
    }

    public IEnumerable<Type> AllTypes()
    {
        return libraries.Select(library => library.Value.Type);
    }
}