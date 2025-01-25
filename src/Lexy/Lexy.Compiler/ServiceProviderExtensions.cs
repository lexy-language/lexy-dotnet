using Lexy.Compiler.Compiler;
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
        services.TryAdd(ServiceDescriptor.Singleton<ILexyParser, LexyParser>());
        services.TryAdd(ServiceDescriptor.Singleton<ISourceCodeDocument, SourceCodeDocument>());
        services.TryAdd(ServiceDescriptor.Singleton<ITokenizer, Tokenizer>());
        services.TryAdd(ServiceDescriptor.Singleton<IExpressionFactory, ExpressionFactory>());

        services.TryAdd(ServiceDescriptor.Singleton<ICompilationEnvironment, CompilationEnvironment>());
        services.TryAdd(ServiceDescriptor.Singleton<IExecutionContext, ExecutionContext>());

        services.TryAdd(ServiceDescriptor.Singleton<ILexyCompiler, LexyCompiler>());

        services.TryAdd(ServiceDescriptor.Transient<ISpecificationsRunner, SpecificationsRunner>());

        return services;
    }
}