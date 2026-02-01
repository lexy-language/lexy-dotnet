using System;
using System.Threading.Tasks;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser.Logging;
using NUnit.Framework;

namespace Lexy.Tests.Symbols;

public class GetSymbolsTests : ScopedServicesTestFixture
{
    [Test]
    public async Task AllKeywords()
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
            .Description(1, 1, "scenario: TestSymbols", SymbolKind.Scenario, "Test scenario")
            .Description(1, 10, "scenario: TestSymbols", SymbolKind.Scenario)
            .VerifyDescriptionNull(1, 21, 25, 100, 104)
            .VerifyDescriptionNull(2, 1, 2)
            .Description(2, 10, "function: TestSymbolsFunction", SymbolKind.Function)
            .Description(3, 7, "parameters", SymbolKind.Keyword)
            .Description(4, 8, "type: TypeExample", SymbolKind.Type)
            .Description(4, 22, "parameter: TypeExample Example", SymbolKind.ParameterVariable)
            .Description(5, 10, "results", SymbolKind.Keyword)
            .Description(6, 8, "value type: number", SymbolKind.ValueType)
            .Description(6, 16, "result: number Result", SymbolKind.ResultVariable)
            .Description(7, 5, "spread operator", SymbolKind.Operator)
            .Description(7, 6, "spread operator", SymbolKind.Operator)
            .Description(7, 7, "spread operator", SymbolKind.Operator)
            .Description(7, 11, "function: FunctionWithFunctionDependency", SymbolKind.Function)
            .Description(7, 41, "function: FunctionWithFunctionDependency", SymbolKind.Function)
            .Description(7, 42, "spread operator", SymbolKind.Operator) // todo add mapping
            .Description(8, 6, "spread operator", SymbolKind.Operator) // todo add mapping
            .Description(8, 14, "function: FunctionWithFunctionTypeDependency", SymbolKind.Function)
            .Description(9, 5, "parameters", SymbolKind.Keyword)
            .Description(10, 8, "parameter: EnumExample Example.EnumValue", SymbolKind.ParameterVariable)
            .Description(10, 13, "parameter: EnumExample Example.EnumValue", SymbolKind.ParameterVariable)
            .Description(10, 18, "parameter: EnumExample Example.EnumValue", SymbolKind.ParameterVariable)
            .Description(10, 21, "parameter: EnumExample Example.EnumValue", SymbolKind.ParameterVariable)
            .Description(10, 25, "enum member: EnumExample.Single", SymbolKind.EnumMember)
            .Description(10, 35, "enum member: EnumExample.Single", SymbolKind.EnumMember)
            .Description(10, 37, "enum member: EnumExample.Single", SymbolKind.EnumMember)
            .Description(10, 42, "enum member: EnumExample.Single", SymbolKind.EnumMember)
            .Description(11, 5, "parameter: EnumExample Example.Nested.EnumValue", SymbolKind.ParameterVariable)
            .Description(11, 13, "parameter: EnumExample Example.Nested.EnumValue", SymbolKind.ParameterVariable)
            .Description(11, 20, "parameter: EnumExample Example.Nested.EnumValue", SymbolKind.ParameterVariable)
            .Description(11, 32, "enum member: EnumExample.Married", SymbolKind.EnumMember)
            .Description(11, 44, "enum member: EnumExample.Married", SymbolKind.EnumMember)
            .Description(12, 9, "results", SymbolKind.Keyword)
            .Description(13, 8, "result: number Result", SymbolKind.ResultVariable)
            .Description(13, 15, "777", SymbolKind.Constant)

            .Description(15, 6, "function: FunctionWithFunctionDependency", SymbolKind.Function)
            .Description(15, 21, "function: FunctionWithFunctionDependency", SymbolKind.Function)
            .Description(16, 8, "parameters", SymbolKind.Keyword)
            .Description(17, 8, "type: TypeExample", SymbolKind.Type)
            .Description(17, 20, "parameter: TypeExample Example", SymbolKind.ParameterVariable)
            .Description(18, 4, "results", SymbolKind.Keyword)
            .Description(19, 8, "value type: number", SymbolKind.ValueType)
            .Description(19, 16, "result: number Result", SymbolKind.ResultVariable)

            .Description(20, 5, "spread operator", SymbolKind.Operator)
            .Description(20, 22, "function: FunctionWithTypeDependency", SymbolKind.Function)
            .Description(20, 37, "spread operator", SymbolKind.Operator) // todo add mapping

            .Description(21, 5, "spread operator", SymbolKind.Operator)
            .Description(21, 28, "function: FunctionWithTableDependency", SymbolKind.Function)
            .Description(21, 38, "spread operator", SymbolKind.Operator) // todo add mapping

            .Description(22, 3, "spread operator", SymbolKind.Operator)
            .Description(22, 14, "function: FunctionWithEnumDependency", SymbolKind.Function)
            .Description(22, 36, "spread operator", SymbolKind.Operator) // todo add mapping

            .Description(24, 3, "function: FunctionWithFunctionTypeDependency", SymbolKind.Function)
            .Description(24, 18, "function: FunctionWithFunctionTypeDependency", SymbolKind.Function)
            .Description(25, 12, "parameters", SymbolKind.Keyword)
            .Description(26, 11, "type: TypeExample", SymbolKind.Type)
            .Description(26, 23, "parameter: TypeExample Example", SymbolKind.ParameterVariable)
            .Description(27, 8, "results", SymbolKind.Keyword)
            .Description(28, 7, "value type: number", SymbolKind.ValueType)
            .Description(28, 14, "result: number Result", SymbolKind.ResultVariable)

            .Description(29, 4, "type: FunctionWithTypeDependency.Parameters", SymbolKind.GeneratedType)
            .Description(29, 17, "functionParametersFill", SymbolKind.Variable)
            .Description(29, 34, "fill", SymbolKind.SystemFunction)
            .Description(29, 53, "type: FunctionWithTypeDependency.Parameters", SymbolKind.GeneratedType)
            .Description(29, 66, "type: FunctionWithTypeDependency.Parameters", SymbolKind.GeneratedType)

            .Description(30, 5, "type: FunctionWithTypeDependency.Parameters", SymbolKind.GeneratedType)
            .Description(30, 17, "functionParametersNew", SymbolKind.Variable)
            .Description(30, 31, "new", SymbolKind.SystemFunction)
            .Description(30, 44, "type: FunctionWithTypeDependency.Parameters", SymbolKind.GeneratedType)
            .Description(30, 63, "type: FunctionWithTypeDependency.Parameters", SymbolKind.GeneratedType)

            .Description(31, 5, "type: TableExample.Row", SymbolKind.GeneratedType)
            .Description(31, 13, "tableParameters", SymbolKind.Variable)
            .Description(31, 27, "new", SymbolKind.SystemFunction)
            .Description(31, 30, "type: TableExample.Row", SymbolKind.GeneratedType)
            .Description(31, 41, "type: TableExample.Row", SymbolKind.GeneratedType)

            .Description(32, 7, "result: number Result", SymbolKind.ResultVariable)
            .Description(32, 14, "777", SymbolKind.Constant)

            .Description(34, 7, "function: FunctionWithTypeDependency", SymbolKind.Function)
            .Description(34, 23, "function: FunctionWithTypeDependency", SymbolKind.Function)
            .Description(35, 12, "parameters", SymbolKind.Keyword)
            .Description(36, 11, "type: TypeExample", SymbolKind.Type)
            .Description(36, 23, "parameter: TypeExample Example", SymbolKind.ParameterVariable)
            .Description(37, 8, "results", SymbolKind.Keyword)
            .Description(38, 7, "value type: number", SymbolKind.ValueType)
            .Description(38, 14, "result: number Result", SymbolKind.ResultVariable)

            .Description(41, 7, "function: FunctionWithTableDependency", SymbolKind.Function)
            .Description(41, 28, "function: FunctionWithTableDependency", SymbolKind.Function)
            .Description(42, 12, "parameters", SymbolKind.Keyword)
            .Description(43, 15, "type: TypeExample", SymbolKind.Type)
            .Description(43, 23, "parameter: TypeExample Example", SymbolKind.ParameterVariable)
            .Description(44, 6, "results", SymbolKind.Keyword)

            .Description(45, 8, "value type: number", SymbolKind.ValueType)
            .Description(45, 15, "result: number Result", SymbolKind.ResultVariable)

            .Description(46, 6, "result: number Result", SymbolKind.ResultVariable)
            .Description(46, 27, "table function: TableExample.LookUp", SymbolKind.TableFunction)
            .Description(46, 35, "enum member: EnumExample.Single", SymbolKind.EnumMember)
            .Description(46, 46, "enum member: EnumExample.Single", SymbolKind.EnumMember)
            .Description(46, 75, "type: TableExample.Value", SymbolKind.GeneratedType)
            .Description(46, 88, "type: TableExample.Value", SymbolKind.GeneratedType)

            .Description(48, 7, "function: FunctionWithEnumDependency", SymbolKind.Function)
            .Description(48, 18, "function: FunctionWithEnumDependency", SymbolKind.Function)
            .Description(49, 8, "parameters", SymbolKind.Keyword)
            .Description(50, 15, "enum: EnumExample", SymbolKind.Enum)
            .Description(50, 23, "parameter: EnumExample EnumValue", SymbolKind.ParameterVariable)
            .Description(51, 13, "type: TypeExample", SymbolKind.Type)
            .Description(51, 22, "parameter: TypeExample Example", SymbolKind.ParameterVariable)
            .Description(52, 3, "results", SymbolKind.Keyword)
            .Description(53, 5, "value type: number", SymbolKind.ValueType)
            .Description(53, 16, "result: number Result", SymbolKind.ResultVariable)
            .Description(54, 5, "result: number Result", SymbolKind.ResultVariable)
            .Description(54, 14, "666", SymbolKind.Constant)

            .Description(56, 3, "type: NestedType", SymbolKind.Type)
            .Description(56, 14, "type: NestedType", SymbolKind.Type)
            .Description(57, 18, "parameter: EnumExample EnumValue", SymbolKind.ParameterVariable)
            .Description(58, 18, "parameter: number Result", SymbolKind.ParameterVariable)
            .Description(58, 12, "parameter: number Result", SymbolKind.ParameterVariable)
            .Description(58, 20, "888", SymbolKind.Constant)

            .Description(60, 3, "type: TypeExample", SymbolKind.Type)
            .Description(60, 15, "type: TypeExample", SymbolKind.Type)
            .Description(61, 7, "enum: EnumExample", SymbolKind.Enum)
            .Description(61, 18, "parameter: EnumExample EnumValue", SymbolKind.ParameterVariable)
            .Description(62, 4, "type: NestedType", SymbolKind.Type)
            .Description(62, 18, "parameter: NestedType Nested", SymbolKind.ParameterVariable)

            .Description(64, 4, "table: TableExample", SymbolKind.Table)
            .Description(64, 14, "table: TableExample", SymbolKind.Table)

            .Description(65, 5, "enum: EnumExample", SymbolKind.Enum)
            .Description(65, 21, "Example", SymbolKind.TableColumn)
            .Description(65, 28, "value type: number", SymbolKind.ValueType)
            .Description(65, 35, "Value", SymbolKind.TableColumn)

            .Description(66, 12, "enum member: EnumExample.Single", SymbolKind.EnumMember)
            .Description(66, 19, "enum member: EnumExample.Single", SymbolKind.EnumMember)
            .Description(66, 28, "123", SymbolKind.Constant)

            .Description(68, 2, "enum: EnumExample", SymbolKind.Enum)
            .Description(68, 16, "enum: EnumExample", SymbolKind.Enum)
            .Description(69, 7, "enum member: EnumExample.Single", SymbolKind.EnumMember)
            .Description(70, 8, "enum member: EnumExample.Married", SymbolKind.EnumMember)
            .Description(71, 15, "enum member: EnumExample.CivilPartnership", SymbolKind.EnumMember)
        );
    }
}
