using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Parser;
using Microsoft.Extensions.DependencyInjection;

namespace Lexy.Poc.Core.Specifications
{
    public interface IScenarioRunner
    {
        bool Failed { get; }

        void Initialize(string fileName, Components components, Scenario scenario,
            ISpecificationRunnerContext context, IServiceScope serviceScope, IParserLogger parserLogger);

        void Run();
        string ParserLogging();
    }
}