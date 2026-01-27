using System;
using System.Threading.Tasks;
using Lexy.Compiler.Parser.Logging;
using Lexy.Compiler.Parser.Symbols;
using NUnit.Framework;

namespace Lexy.Tests.Symbols;

public class GetSymbolsTests : ScopedServicesTestFixture
{
    [Test]
    public async Task FunctionWithEnumAndTableDependency()
    {
        var (symbols, nodes) = await ServiceProvider.GetSymbols("test.lexy",
@"scenario TestSymbols                                                                               //  1
  function                                                                                         //  2
    parameters                                                                                     //  3
      TypeExample Example                                                                          //  4
    results                                                                                        //  5
      number Result                                                                                //  6
    ... = FunctionWithFunctionDependency(...)                                                      //  7
    ... = FunctionWithFunctionTypeDependency(...)                                                  //  8
  parameters                                                                                       //  9
    Example.EnumValue = EnumExample.Single                                                         // 10
    Example.Nested.EnumValue = EnumExample.Married                                                 // 11
  results                                                                                          // 12
    Result = 777                                                                                   // 13
                                                                                                   // 14
function FunctionWithFunctionDependency                                                            // 15
  parameters                                                                                       // 16
    TypeExample Example                                                                            // 17
  results                                                                                          // 18
    number Result                                                                                  // 19
  ... = FunctionWithTypeDependency(...)                                                            // 20
  ... = FunctionWithTableDependency(...)                                                           // 21
  ... = FunctionWithEnumDependency(...)                                                            // 22
                                                                                                   // 23
function FunctionWithFunctionTypeDependency                                                        // 24
  parameters                                                                                       // 25
    TypeExample Example                                                                            // 26
  results                                                                                          // 27
    number Result                                                                                  // 28
  var functionParametersFill = fill(FunctionWithTypeDependency.Parameters)                         // 29
  var functionParametersNew = new(FunctionWithTypeDependency.Parameters)                           // 30
  var tableParameters = new(TableExample.Row)                                                      // 31
  Result = 777                                                                                     // 32
                                                                                                   // 33
function FunctionWithTypeDependency                                                                // 34
  parameters                                                                                       // 35
    TypeExample Example                                                                            // 36
  results                                                                                          // 37
    number Result                                                                                  // 38 
  Result = Example.Nested.Result                                                                   // 39
                                                                                                   // 40
function FunctionWithTableDependency                                                               // 41
  parameters                                                                                       // 42
    TypeExample Example                                                                            // 43
  results                                                                                          // 44
    number Result                                                                                  // 45
  Result = TableExample.LookUp(EnumExample.Single, TableExample.Example, TableExample.Value)       // 46
                                                                                                   // 47
function FunctionWithEnumDependency                                                                // 48
  parameters                                                                                       // 49
    EnumExample EnumValue                                                                          // 50
    TypeExample Example                                                                            // 51
  results                                                                                          // 52
    number Result                                                                                  // 53
  Result = 666                                                                                     // 54
                                                                                                   // 55
type NestedType                                                                                    // 56
  EnumExample EnumValue                                                                            // 57
  number Result = 888                                                                              // 58
                                                                                                   // 59
type TypeExample                                                                                   // 60
  EnumExample EnumValue                                                                            // 61
  NestedType Nested                                                                                // 62
                                                                                                   // 63
table TableExample                                                                                 // 64
  | EnumExample Example | number Value |                                                           // 65
  | EnumExample.Single  | 123          |                                                           // 66
                                                                                                   // 67
enum EnumExample                                                                                   // 68
  Single                                                                                           // 69
  Married                                                                                          // 70
  CivilPartnership                                                                                 // 71");

        var nodesLog = NodesLogger.Log(nodes);
        Console.WriteLine(nodesLog);

        Verify.Model(symbols, _ => _
            .VerifyDescription(1, 1, "scenario: TestSymbols", SymbolKind.Scenario, "Test scenario")
            .VerifyDescription(1, 10, "scenario: TestSymbols", SymbolKind.Scenario)
            .VerifyDescriptionNull(1, 21, 25, 100, 104)
            .VerifyDescriptionNull(2, 1, 2)
            .VerifyDescription(2, 10, "function: TestSymbolsFunction", SymbolKind.Function)
            .VerifyDescription(3, 7, "parameters", SymbolKind.Keyword)
            .VerifyDescription(4, 8, "type: TypeExample", SymbolKind.Type)
            .VerifyDescription(4, 22, "parameter: TypeExample Example", SymbolKind.ParameterVariable)
            .VerifyDescription(5, 10, "results", SymbolKind.Keyword)
            .VerifyDescription(6, 8, "value type: number", SymbolKind.ValueType)
            .VerifyDescription(6, 16, "result: number Result", SymbolKind.ResultVariable)
            .VerifyDescription(7, 5, "spread operator", SymbolKind.Operator)
            .VerifyDescription(7, 6, "spread operator", SymbolKind.Operator)
            .VerifyDescription(7, 7, "spread operator", SymbolKind.Operator)
            .VerifyDescription(7, 11, "function: FunctionWithFunctionDependency", SymbolKind.Function)
            .VerifyDescription(7, 41, "function: FunctionWithFunctionDependency", SymbolKind.Function)
            .VerifyDescription(7, 42, "spread operator", SymbolKind.Operator) // todo add mapping
            .VerifyDescription(8, 6, "spread operator", SymbolKind.Operator) // todo add mapping
            .VerifyDescription(8, 14, "function: FunctionWithFunctionTypeDependency", SymbolKind.Function)
            .VerifyDescription(9, 5, "parameters", SymbolKind.Keyword)
            .VerifyDescription(10, 8, "parameter: EnumExample Example.EnumValue", SymbolKind.ParameterVariable)
            .VerifyDescription(10, 13, "parameter: EnumExample Example.EnumValue", SymbolKind.ParameterVariable)
            .VerifyDescription(10, 18, "parameter: EnumExample Example.EnumValue", SymbolKind.ParameterVariable)
            .VerifyDescription(10, 21, "parameter: EnumExample Example.EnumValue", SymbolKind.ParameterVariable)
            .VerifyDescription(10, 25, "enum member: EnumExample.Single", SymbolKind.EnumMember)
            .VerifyDescription(10, 35, "enum member: EnumExample.Single", SymbolKind.EnumMember)
            .VerifyDescription(10, 37, "enum member: EnumExample.Single", SymbolKind.EnumMember)
            .VerifyDescription(10, 42, "enum member: EnumExample.Single", SymbolKind.EnumMember)
            .VerifyDescription(11, 5, "parameter: EnumExample Example.Nested.EnumValue", SymbolKind.ParameterVariable)
            .VerifyDescription(11, 13, "parameter: EnumExample Example.Nested.EnumValue", SymbolKind.ParameterVariable)
            .VerifyDescription(11, 20, "parameter: EnumExample Example.Nested.EnumValue", SymbolKind.ParameterVariable)
            .VerifyDescription(11, 32, "enum member: EnumExample.Married", SymbolKind.EnumMember)
            .VerifyDescription(11, 44, "enum member: EnumExample.Married", SymbolKind.EnumMember)
            .VerifyDescription(12, 9, "results", SymbolKind.Keyword)
            .VerifyDescription(13, 8, "result: number Result", SymbolKind.ResultVariable)
            .VerifyDescription(13, 15, "777", SymbolKind.Constant)

            .VerifyDescription(15, 6, "function: FunctionWithFunctionDependency", SymbolKind.Function)
            .VerifyDescription(15, 21, "function: FunctionWithFunctionDependency", SymbolKind.Function)
            .VerifyDescription(16, 8, "parameters", SymbolKind.Keyword)
            .VerifyDescription(17, 8, "type: TypeExample", SymbolKind.Type)
            .VerifyDescription(17, 20, "parameter: TypeExample Example", SymbolKind.ParameterVariable)
            .VerifyDescription(18, 4, "results", SymbolKind.Keyword)
            .VerifyDescription(19, 8, "value type: number", SymbolKind.ValueType)
            .VerifyDescription(19, 16, "result: number Result", SymbolKind.ResultVariable)

            .VerifyDescription(20, 5, "spread operator", SymbolKind.Operator)
            .VerifyDescription(20, 22, "function: FunctionWithTypeDependency", SymbolKind.Function)
            .VerifyDescription(20, 37, "spread operator", SymbolKind.Operator) // todo add mapping

            .VerifyDescription(21, 5, "spread operator", SymbolKind.Operator)
            .VerifyDescription(21, 28, "function: FunctionWithTableDependency", SymbolKind.Function)
            .VerifyDescription(21, 38, "spread operator", SymbolKind.Operator) // todo add mapping

            .VerifyDescription(22, 3, "spread operator", SymbolKind.Operator)
            .VerifyDescription(22, 14, "function: FunctionWithEnumDependency", SymbolKind.Function)
            .VerifyDescription(22, 36, "spread operator", SymbolKind.Operator) // todo add mapping

            .VerifyDescription(24, 3, "function: FunctionWithFunctionTypeDependency", SymbolKind.Function)
            .VerifyDescription(24, 18, "function: FunctionWithFunctionTypeDependency", SymbolKind.Function)
            .VerifyDescription(25, 12, "parameters", SymbolKind.Keyword)
            .VerifyDescription(26, 11, "type: TypeExample", SymbolKind.Type)
            .VerifyDescription(26, 23, "parameter: TypeExample Example", SymbolKind.ParameterVariable)
            .VerifyDescription(27, 8, "results", SymbolKind.Keyword)
            .VerifyDescription(28, 7, "value type: number", SymbolKind.ValueType)
            .VerifyDescription(28, 14, "result: number Result", SymbolKind.ResultVariable)

            .VerifyDescription(29, 4, "type: FunctionWithTypeDependency.Parameters", SymbolKind.GeneratedType)
            .VerifyDescription(29, 17, "functionParametersFill", SymbolKind.Variable)
            .VerifyDescription(29, 34, "fill", SymbolKind.SystemFunction)
            .VerifyDescription(29, 53, "type: FunctionWithTypeDependency.Parameters", SymbolKind.GeneratedType)
            .VerifyDescription(29, 66, "type: FunctionWithTypeDependency.Parameters", SymbolKind.GeneratedType)

            .VerifyDescription(30, 5, "type: FunctionWithTypeDependency.Parameters", SymbolKind.GeneratedType)
            .VerifyDescription(30, 17, "functionParametersNew", SymbolKind.Variable)
            .VerifyDescription(30, 31, "new", SymbolKind.SystemFunction)
            .VerifyDescription(30, 44, "type: FunctionWithTypeDependency.Parameters", SymbolKind.GeneratedType)
            .VerifyDescription(30, 63, "type: FunctionWithTypeDependency.Parameters", SymbolKind.GeneratedType)

            .VerifyDescription(31, 5, "type: TableExample.Row", SymbolKind.GeneratedType)
            .VerifyDescription(31, 13, "tableParameters", SymbolKind.Variable)
            .VerifyDescription(31, 27, "new", SymbolKind.SystemFunction)
            .VerifyDescription(31, 30, "type: TableExample.Row", SymbolKind.GeneratedType)
            .VerifyDescription(31, 41, "type: TableExample.Row", SymbolKind.GeneratedType)

            .VerifyDescription(32, 7, "result: number Result", SymbolKind.ResultVariable)
            .VerifyDescription(32, 14, "777", SymbolKind.Constant)

            .VerifyDescription(34, 7, "function: FunctionWithTypeDependency", SymbolKind.Function)
            .VerifyDescription(34, 23, "function: FunctionWithTypeDependency", SymbolKind.Function)
            .VerifyDescription(35, 12, "parameters", SymbolKind.Keyword)
            .VerifyDescription(36, 11, "type: TypeExample", SymbolKind.Type)
            .VerifyDescription(36, 23, "parameter: TypeExample Example", SymbolKind.ParameterVariable)
            .VerifyDescription(37, 8, "results", SymbolKind.Keyword)
            .VerifyDescription(38, 7, "value type: number", SymbolKind.ValueType)
            .VerifyDescription(38, 14, "result: number Result", SymbolKind.ResultVariable)

            .VerifyDescription(41, 7, "function: FunctionWithTableDependency", SymbolKind.Function)
            .VerifyDescription(41, 28, "function: FunctionWithTableDependency", SymbolKind.Function)
            .VerifyDescription(42, 12, "parameters", SymbolKind.Keyword)
            .VerifyDescription(43, 15, "type: TypeExample", SymbolKind.Type)
            .VerifyDescription(43, 23, "parameter: TypeExample Example", SymbolKind.ParameterVariable)
            .VerifyDescription(44, 6, "results", SymbolKind.Keyword)

            .VerifyDescription(45, 8, "value type: number", SymbolKind.ValueType)
            .VerifyDescription(45, 15, "result: number Result", SymbolKind.ResultVariable)

            .VerifyDescription(46, 6, "result: number Result", SymbolKind.ResultVariable)
            .VerifyDescription(46, 27, "table function: TableExample.LookUp", SymbolKind.TableFunction)
            .VerifyDescription(46, 35, "enum member: EnumExample.Single", SymbolKind.EnumMember)
            .VerifyDescription(46, 46, "enum member: EnumExample.Single", SymbolKind.EnumMember)
            .VerifyDescription(46, 75, "type: TableExample.Value", SymbolKind.GeneratedType)
            .VerifyDescription(46, 88, "type: TableExample.Value", SymbolKind.GeneratedType)

            .VerifyDescription(48, 7, "function: FunctionWithEnumDependency", SymbolKind.Function)
            .VerifyDescription(48, 18, "function: FunctionWithEnumDependency", SymbolKind.Function)
            .VerifyDescription(49, 8, "parameters", SymbolKind.Keyword)
            .VerifyDescription(50, 15, "enum: EnumExample", SymbolKind.Enum)
            .VerifyDescription(50, 23, "parameter: EnumExample EnumValue", SymbolKind.ParameterVariable)
            .VerifyDescription(51, 13, "type: TypeExample", SymbolKind.Type)
            .VerifyDescription(51, 22, "parameter: TypeExample Example", SymbolKind.ParameterVariable)
            .VerifyDescription(52, 3, "results", SymbolKind.Keyword)
            .VerifyDescription(53, 5, "value type: number", SymbolKind.ValueType)
            .VerifyDescription(53, 16, "result: number Result", SymbolKind.ResultVariable)
            .VerifyDescription(54, 5, "result: number Result", SymbolKind.ResultVariable)
            .VerifyDescription(54, 14, "666", SymbolKind.Constant)

            .VerifyDescription(56, 3, "type: NestedType", SymbolKind.Type)
            .VerifyDescription(56, 14, "type: NestedType", SymbolKind.Type)
            .VerifyDescription(57, 18, "parameter: EnumExample EnumValue", SymbolKind.ParameterVariable)
            .VerifyDescription(58, 18, "parameter: number Result", SymbolKind.ParameterVariable)
            .VerifyDescription(58, 12, "parameter: number Result", SymbolKind.ParameterVariable)
            .VerifyDescription(58, 20, "888", SymbolKind.Constant)

            .VerifyDescription(60, 3, "type: TypeExample", SymbolKind.Type)
            .VerifyDescription(60, 15, "type: TypeExample", SymbolKind.Type)
            .VerifyDescription(61, 7, "enum: EnumExample", SymbolKind.Enum)
            .VerifyDescription(61, 18, "parameter: EnumExample EnumValue", SymbolKind.ParameterVariable)
            .VerifyDescription(62, 4, "type: NestedType", SymbolKind.Type)
            .VerifyDescription(62, 18, "parameter: NestedType Nested", SymbolKind.ParameterVariable)

            .VerifyDescription(64, 4, "table: TableExample", SymbolKind.Table)
            .VerifyDescription(64, 14, "table: TableExample", SymbolKind.Table)

            .VerifyDescription(65, 5, "enum: EnumExample", SymbolKind.Enum)
            .VerifyDescription(65, 21, "Example", SymbolKind.TableColumn)
            .VerifyDescription(65, 28, "value type: number", SymbolKind.ValueType)
            .VerifyDescription(65, 35, "Value", SymbolKind.TableColumn)

            .VerifyDescription(66, 12, "enum member: EnumExample.Single", SymbolKind.EnumMember)
            .VerifyDescription(66, 19, "enum member: EnumExample.Single", SymbolKind.EnumMember)
            .VerifyDescription(66, 28, "123", SymbolKind.Constant)

            .VerifyDescription(68, 2, "enum: EnumExample", SymbolKind.Enum)
            .VerifyDescription(68, 16, "enum: EnumExample", SymbolKind.Enum)
            .VerifyDescription(69, 7, "enum member: EnumExample.Single", SymbolKind.EnumMember)
            .VerifyDescription(70, 8, "enum member: EnumExample.Married", SymbolKind.EnumMember)
            .VerifyDescription(71, 15, "enum member: EnumExample.CivilPartnership", SymbolKind.EnumMember)

          /*
           test.lexy (65:3-40)   TableHeader (Lexy.Compiler.Language.Tables.TableHeader)
          test.lexy (65:17-25)     ColumnHeader (Lexy.Compiler.Language.Tables.ColumnHeader)
          test.lexy (65:17-25)       ObjectTypeDeclaration (EnumExample)
          test.lexy (65:34-40)     ColumnHeader (Lexy.Compiler.Language.Tables.ColumnHeader)
          test.lexy (65:34-40)       ValueTypeDeclaration (number)


table TableExample                                                                                 // 64
  | EnumExample Example | number Value |                                                           // 65
  | EnumExample.Single  | 123          |                                                           // 66
                                                                                                   // 67
enum EnumExample                                                                                   // 68
  Single                                                                                           // 69
  Married                                                                                          // 70
  CivilPartnership                                                                                 // 71");
 */
        );
    }
}
