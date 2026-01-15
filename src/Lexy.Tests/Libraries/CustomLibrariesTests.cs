using System.Threading.Tasks;
using Lexy.Compiler;
using Lexy.Tests.Compiler;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Libraries;

public class CustomLibrariesTests : ScopedServicesTestFixture
{
    protected override void Initialize(IServiceCollection serviceCollection)
    {
        serviceCollection.AddLexy(typeof(CustomLibrary));
    }

    [Test]
    public async Task LibraryFunctionString()
    {
        using var script = await ServiceProvider.CompileFunction($@"
function SimpleFunction
  results
    string Result
  Result = CustomLibrary.FunctionString(""b"")");
        var result = script.Run();
        var value = (string)result.GetValue("Result");
        value.ShouldBe("abc");
    }
}
