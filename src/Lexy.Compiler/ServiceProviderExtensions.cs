using Lexy.Compiler.Compiler;
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
    public static IServiceCollection AddLexy(this IServiceCollection services)
    {
        return services.Singleton<ILexyParser, LexyParser>()

            .Singleton<ISourceCodeDocument, SourceCodeDocument>()
            .Singleton<ITokenizer, Tokenizer>()
            .Singleton<IExpressionFactory, ExpressionFactory>()

            .Singleton<IFileSystem, FileSystem>()
            .Singleton<ICompilationEnvironment, CompilationEnvironment>()
            .Singleton<IExecutionContext, ExecutionContext>()

            .Singleton<ILexyCompiler, LexyCompiler>()

            .Transient<ISpecificationsRunner, SpecificationsRunner>();
    }

    private static IServiceCollection Singleton<TInterface, IImplementation>(this IServiceCollection services)
        where TInterface : class
        where IImplementation : class, TInterface
    {
        services.TryAdd(ServiceDescriptor.Singleton<TInterface, IImplementation>());
        services.TryAdd(ServiceDescriptor.Transient<ISpecificationsRunner, SpecificationsRunner>());

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
