using Lexy.Poc.Core;
using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;
using Lexy.Poc.Parser.ExpressionParser;
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

            var context = GetService<IParserContext>();
            context.Logger.HasErrors().ShouldBeFalse(context.Logger.FormatMessages());

            script.Name.Value.ShouldBe("TestTable");
            script.Header.Values.Count.ShouldBe(2);
            script.Header.Values[0].Name.ShouldBe("Value");
            script.Header.Values[0].Type.ShouldBePrimitiveType(TypeNames.Number);
            script.Header.Values[1].Name.ShouldBe("Result");
            script.Header.Values[1].Type.ShouldBePrimitiveType(TypeNames.String);
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
  | date Value | boolean Result |
  | d""2024/12/18 17:07:45"" | false |
  | d""2024/12/18 17:08:12"" | true |";

            var parser = ServiceProvider.GetRequiredService<ILexyParser>();
            var script = parser.ParseTable(code);

            script.Name.Value.ShouldBe("TestTable");
            script.Header.Values.Count.ShouldBe(2);
            script.Header.Values[0].Name.ShouldBe("Value");
            script.Header.Values[0].Type.ShouldBePrimitiveType(TypeNames.Date);
            script.Header.Values[1].Name.ShouldBe("Result");
            script.Header.Values[1].Type.ShouldBePrimitiveType(TypeNames.Boolean);
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

    internal static class VariableTypeExtensions
    {
        public static void ShouldBePrimitiveType(this VariableDeclarationType type, string name)
        {
            type.ShouldBeOfType<PrimitiveVariableDeclarationType>()
                .Type.ShouldBe(name);
        }
    }
}