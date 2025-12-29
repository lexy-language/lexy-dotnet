using System;
using Lexy.Compiler.Compiler;
using Lexy.Compiler.FunctionLibraries;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;
using Lexy.Compiler.Specifications;
using Lexy.RunTime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lexy.Compiler;

public static class ServiceProviderExtensions
{
    public static IServiceCollection AddLexy(this IServiceCollection services, params Type[] libraries)
    {
        return services.Singleton<ILexyParser, LexyParser>()

            .Singleton<ISourceCodeDocument, SourceCodeDocument>()
            .Singleton<ITokenizer, Tokenizer>()
            .Singleton<IExpressionFactory, ExpressionFactory>()

            .Singleton<IFileSystem, FileSystem>()
            .Singleton<IExecutionContext, ExecutionContext>()
            .SingletonFactory<ILibraries, Libraries>(_ => new Libraries(libraries))

            .Singleton<ILexyCompiler, LexyCompiler>()

            .AddScoped<ICompilationEnvironment, CompilationEnvironment>()
            .Transient<ISpecificationsRunner, SpecificationsRunner>();
    }

    private static IServiceCollection Singleton<TInterface, IImplementation>(this IServiceCollection services)
        where TInterface : class
        where IImplementation : class, TInterface
    {
        services.TryAdd(ServiceDescriptor.Singleton<TInterface, IImplementation>());

        return services;
    }

    private static IServiceCollection SingletonFactory<TInterface, IImplementation>(this IServiceCollection services,
        Func<IServiceProvider,TInterface> factory)
        where TInterface : class
        where IImplementation : class, TInterface
    {
        services.TryAdd(ServiceDescriptor.Singleton(factory));

        return services;
    }

    private  static IServiceCollection Transient<TInterface, IImplementation>(this IServiceCollection services)
        where TInterface : class
        where IImplementation : class, TInterface
    {
        services.TryAdd(ServiceDescriptor.Transient<TInterface, IImplementation>());

        return services;
    }
}