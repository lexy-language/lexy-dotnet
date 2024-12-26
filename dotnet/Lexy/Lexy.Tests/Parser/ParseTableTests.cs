using Lexy.Poc.Core;
using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Poc.Parser
{
    public class ParseTableTests : ScopedServicesTestFixture
    {
        [Test]
        public void TestInAndStringColumns()
        {
            var code = @"Table: TestTable
  | number Value | string Result |
  | 7 | ""Test quoted"" |
  | 8 | Test |";

            var parser = ServiceProvider.GetRequiredService<ILexyParser>();
            var table = parser.ParseTable(code);

            table.Name.Value.ShouldBe("TestTable");
            table.Header.Values.Count.ShouldBe(2);
            table.Header.Values[0].Name.ShouldBe("Value");
            table.Header.Values[0].Type.ShouldBePrimitiveType(TypeNames.Number);
            table.Header.Values[1].Name.ShouldBe("Result");
            table.Header.Values[1].Type.ShouldBePrimitiveType(TypeNames.String);
            table.Rows.Count.ShouldBe(2);
            table.Rows[0].Values[0].ShouldBeOfType<NumberLiteralToken>();
            table.Rows[0].Values[0].Value.ShouldBe("7");
            table.Rows[0].Values[1].ShouldBeOfType<QuotedLiteralToken>();
            table.Rows[0].Values[1].Value.ShouldBe("Test quoted");
            table.Rows[1].Values[0].ShouldBeOfType<NumberLiteralToken>();
            table.Rows[1].Values[0].Value.ShouldBe("8");
            table.Rows[1].Values[1].ShouldBeOfType<StringLiteralToken>();
            table.Rows[1].Values[1].Value.ShouldBe("Test");
        }

        [Test]
        public void TestDateTimeAndBoolean()
        {
            var code = @"Table: TestTable
  | date Value | boolean Result |
  | d""2024/12/18 17:07:45"" | false |
  | d""2024/12/18 17:08:12"" | true |";

            var parser = ServiceProvider.GetService<ILexyParser>();
            var table = parser.ParseTable(code);

            table.Name.Value.ShouldBe("TestTable");
            table.Header.Values.Count.ShouldBe(2);
            table.Header.Values[0].Name.ShouldBe("Value");
            table.Header.Values[0].Type.ShouldBePrimitiveType(TypeNames.Date);
            table.Header.Values[1].Name.ShouldBe("Result");
            table.Header.Values[1].Type.ShouldBePrimitiveType(TypeNames.Boolean);
            table.Rows.Count.ShouldBe(2);
            table.Rows[0].Values[0].ShouldBeOfType<DateTimeLiteral>();
            table.Rows[0].Values[0].Value.ShouldBe("2024/12/18 17:07:45");
            table.Rows[0].Values[1].ShouldBeOfType<BooleanLiteral>();
            table.Rows[0].Values[1].Value.ShouldBe("false");
            table.Rows[1].Values[0].ShouldBeOfType<DateTimeLiteral>();
            table.Rows[1].Values[0].Value.ShouldBe("2024/12/18 17:08:12");
            table.Rows[1].Values[1].ShouldBeOfType<BooleanLiteral>();
            table.Rows[1].Values[1].Value.ShouldBe("true");
        }
    }
}