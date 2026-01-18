using System.Threading.Tasks;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Tests.Parser.ExpressionParser;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Parser;

public class ParseTableTests : ScopedServicesTestFixture
{
    [Test]
    public async Task TestInAndStringColumns()
    {
        const string code = @"table TestTable
  | number Value | string Result |
  | 7 | ""Test quoted"" |
  | 8 | ""Test"" |";

        var (table, _) = await ServiceProvider.ParseTable(code);

        table.Name.ShouldBe("TestTable");
        table.Header.Columns.Count.ShouldBe(2);
        table.Header.Columns[0].Name.ShouldBe("Value");
        table.Header.Columns[0].TypeDeclaration.ShouldBePrimitiveType(TypeNames.Number);
        table.Header.Columns[1].Name.ShouldBe("Result");
        table.Header.Columns[1].TypeDeclaration.ShouldBePrimitiveType(TypeNames.String);
        table.Rows.Count.ShouldBe(2);
        table.Rows[0].Values[0].Expression.ValidateNumericLiteralExpression(7);
        table.Rows[0].Values[1].Expression.ValidateQuotedLiteralExpression("Test quoted");
        table.Rows[1].Values[0].Expression.ValidateNumericLiteralExpression(8);
        table.Rows[1].Values[1].Expression.ValidateQuotedLiteralExpression("Test");
    }

    [Test]
    public async Task TestDateTimeAndBoolean()
    {
        const string code = @"table TestTable
  | date Value | boolean Result |
  | d""2024-12-18T17:07:45"" | false |
  | d""2024-12-18T17:08:12"" | true |";

        var (table, _) = await ServiceProvider.ParseTable(code);

        table.Name.ShouldBe("TestTable");
        table.Header.Columns.Count.ShouldBe(2);
        table.Header.Columns[0].Name.ShouldBe("Value");
        table.Header.Columns[0].TypeDeclaration.ShouldBePrimitiveType(TypeNames.Date);
        table.Header.Columns[1].Name.ShouldBe("Result");
        table.Header.Columns[1].TypeDeclaration.ShouldBePrimitiveType(TypeNames.Boolean);
        table.Rows.Count.ShouldBe(2);
        table.Rows[0].Values[0].Expression.ValidateDateTimeLiteralExpression("2024-12-18T17:07:45");
        table.Rows[0].Values[1].Expression.ValidateBooleanLiteralExpression(false);
        table.Rows[1].Values[0].Expression.ValidateDateTimeLiteralExpression("2024-12-18T17:08:12");
        table.Rows[1].Values[1].Expression.ValidateBooleanLiteralExpression(true);
    }
}
