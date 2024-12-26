using Lexy.Poc.Core;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Poc.Parser
{
    public class ParseEnumTests : ScopedServicesTestFixture
    {
        [Test]
        public void SimpleEnum()
        {
            var code = @"Enum: Enum1
  First
  Second";

            var parser = ServiceProvider.GetRequiredService<ILexyParser>();
            var enumValue = parser.ParseEnum(code);

            enumValue.Name.Value.ShouldBe("Enum1");
            enumValue.Members.Count.ShouldBe(2);
            enumValue.Members[0].Name.ShouldBe("First");
            enumValue.Members[0].Value.ShouldBeNull();
            enumValue.Members[1].Name.ShouldBe("Second");
            enumValue.Members[1].Value.ShouldBeNull();
        }

        [Test]
        public void EnumWithValues()
        {
            var code = @"Enum: Enum2
  First = 5
  Second = 6";

            var parser = ServiceProvider.GetRequiredService<ILexyParser>();
            var enumValue = parser.ParseEnum(code);

            enumValue.Name.Value.ShouldBe("Enum2");
            enumValue.Members.Count.ShouldBe(2);
            enumValue.Members[0].Name.ShouldBe("First");
            enumValue.Members[0].Value.NumberValue.ShouldBe(5);
            enumValue.Members[1].Name.ShouldBe("Second");
            enumValue.Members[1].Value.NumberValue.ShouldBe(6m);
        }
    }
}