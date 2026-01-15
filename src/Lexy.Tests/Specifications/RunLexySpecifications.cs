using System.Threading.Tasks;
using Lexy.Compiler.Specifications;
using NUnit.Framework;

namespace Lexy.Tests.Specifications;

public class RunLexySpecifications : ScopedServicesTestFixture
{
    [Test]
    public async Task RunAll()
    {
        LoggingConfiguration.LogFileNames();

        var runner = GetService<ISpecificationsRunner>();
        await runner.RunAll("../../../lexy-language/Specifications");
    }
}
