using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Parser;

public class ParseEnumTests : ScopedServicesTestFixture
{
    [Test]
    public async Task SimpleEnum()
    {
        const string code = @"enum Enum1
  First
  Second";

        var (enumValue, _) = await ServiceProvider.ParseEnum(code);

        enumValue.Name.ShouldBe("Enum1");
        enumValue.Members.Count.ShouldBe(2);
        enumValue.Members[0].Name.ShouldBe("First");
        enumValue.Members[0].NumberValue.ShouldBe(0);
        enumValue.Members[0].ValueLiteral.ShouldBeNull();
        enumValue.Members[1].Name.ShouldBe("Second");
        enumValue.Members[1].NumberValue.ShouldBe(1);
        enumValue.Members[1].ValueLiteral.ShouldBeNull();
    }

    [Test]
    public async Task EnumWithValues()
    {
        const string code = @"enum Enum2
  First = 5
  Second = 6";

        var (enumValue, _) = await ServiceProvider.ParseEnum(code);

        enumValue.Name.ShouldBe("Enum2");
        enumValue.Members.Count.ShouldBe(2);
        enumValue.Members[0].Name.ShouldBe("First");
        enumValue.Members[0].NumberValue.ShouldBe(5);
        enumValue.Members[0].ValueLiteral.NumberValue.ShouldBe(5);
        enumValue.Members[1].Name.ShouldBe("Second");
        enumValue.Members[1].NumberValue.ShouldBe(6);
        enumValue.Members[1].ValueLiteral.NumberValue.ShouldBe(6m);
    }
}
