using Lexy.Compiler.Language.Tables;
using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;
using Lexy.Tests.Parser.ExpressionParser;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Parser;

public class ParseTableTests : ScopedServicesTestFixture
{
    [Test]
    public void TestInAndStringColumns()
    {
        const string code = @"Table: TestTable
  | number Value | string Result |
  | 7 | ""Test quoted"" |
  | 8 | ""Test"" |";

        var (table, _) = ServiceProvider.ParseTable(code);

        table.Name.Value.ShouldBe("TestTable");
        table.Header.Columns.Count.ShouldBe(2);
        table.Header.Columns[0].Name.ShouldBe("Value");
        table.Header.Columns[0].Type.ShouldBePrimitiveType(TypeNames.Number);
        table.Header.Columns[1].Name.ShouldBe("Result");
        table.Header.Columns[1].Type.ShouldBePrimitiveType(TypeNames.String);
        table.Rows.Count.ShouldBe(2);
        table.Rows[0].Values[0].ValidateNumericLiteralExpression(7);
        table.Rows[0].Values[1].ValidateQuotedLiteralExpression("Test quoted");
        table.Rows[1].Values[0].ValidateNumericLiteralExpression(8);
        table.Rows[1].Values[1].ValidateQuotedLiteralExpression("Test");
    }

    [Test]
    public void TestDateTimeAndBoolean()
    {
        const string code = @"Table: TestTable
  | date Value | boolean Result |
  | d""2024-12-18T17:07:45"" | false |
  | d""2024-12-18T17:08:12"" | true |";

        var (table, _) = ServiceProvider.ParseTable(code);

        table.Name.Value.ShouldBe("TestTable");
        table.Header.Columns.Count.ShouldBe(2);
        table.Header.Columns[0].Name.ShouldBe("Value");
        table.Header.Columns[0].Type.ShouldBePrimitiveType(TypeNames.Date);
        table.Header.Columns[1].Name.ShouldBe("Result");
        table.Header.Columns[1].Type.ShouldBePrimitiveType(TypeNames.Boolean);
        table.Rows.Count.ShouldBe(2);
        table.Rows[0].Values[0].ValidateDateTimeLiteralExpression("2024-12-18T17:07:45");
        table.Rows[0].Values[1].ValidateBooleanLiteralExpression(false);
        table.Rows[1].Values[0].ValidateDateTimeLiteralExpression("2024-12-18T17:08:12");
        table.Rows[1].Values[1].ValidateBooleanLiteralExpression(true);
    }
}