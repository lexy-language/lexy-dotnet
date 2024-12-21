using Lexy.Poc.Core;
using Lexy.Poc.Core.Parser;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Poc.Parser
{
    public class ParseExpressionsTests : ScopedServicesTestFixture
    {
        [Test]
        public void TestInAndStringColumns()
        {
            var code = @"Table: TestTable
  | number Value | string Result |
  | 7 | ""Test quoted"" |
  | 8 | Test |";

            var parser = ServiceProvider.GetRequiredService<ILexyParser>();
            var script = parser.ParseTable(code);

            script.Name.Value.ShouldBe("TestTable");
            script.Headers.Values.Count.ShouldBe(2);
            script.Headers.Values[0].Name.ShouldBe("Value");
            script.Headers.Values[0].Type.ShouldBe(TypeNames.Number);
            script.Headers.Values[1].Name.ShouldBe("Result");
            script.Headers.Values[1].Type.ShouldBe(TypeNames.String);
            script.Rows.Count.ShouldBe(2);
            script.Rows[0].Values[0].ShouldBeOfType<NumberLiteralToken>();
            script.Rows[0].Values[0].Value.ShouldBe("7");
            script.Rows[0].Values[1].ShouldBeOfType<QuotedLiteralToken>();
            script.Rows[0].Values[1].Value.ShouldBe("Test quoted");
            script.Rows[1].Values[0].ShouldBeOfType<NumberLiteralToken>();
            script.Rows[1].Values[0].Value.ShouldBe("8");
            script.Rows[1].Values[1].ShouldBeOfType<StringLiteralToken>();
            script.Rows[1].Values[1].Value.ShouldBe("Test");
        }

        [Test]
        public void TestDateTimeAndBoolean()
        {
            var code = @"Table: TestTable
  | datetime Value | boolean Result |
  | d""2024/12/18 17:07:45"" | false |
  | d""2024/12/18 17:08:12"" | true |";

            var parser = ServiceProvider.GetRequiredService<ILexyParser>();
            var script = parser.ParseTable(code);

            script.Name.Value.ShouldBe("TestTable");
            script.Headers.Values.Count.ShouldBe(2);
            script.Headers.Values[0].Name.ShouldBe("Value");
            script.Headers.Values[0].Type.ShouldBe(TypeNames.DateTime);
            script.Headers.Values[1].Name.ShouldBe("Result");
            script.Headers.Values[1].Type.ShouldBe(TypeNames.Boolean);
            script.Rows.Count.ShouldBe(2);
            script.Rows[0].Values[0].ShouldBeOfType<DateTimeLiteral>();
            script.Rows[0].Values[0].Value.ShouldBe("2024/12/18 17:07:45");
            script.Rows[0].Values[1].ShouldBeOfType<BooleanLiteral>();
            script.Rows[0].Values[1].Value.ShouldBe("false");
            script.Rows[1].Values[0].ShouldBeOfType<DateTimeLiteral>();
            script.Rows[1].Values[0].Value.ShouldBe("2024/12/18 17:08:12");
            script.Rows[1].Values[1].ShouldBeOfType<BooleanLiteral>();
            script.Rows[1].Values[1].Value.ShouldBe("true");
        }
    }
}