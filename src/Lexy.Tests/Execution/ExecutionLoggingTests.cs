using System;
using System.Collections.Generic;
using System.Text.Json;
using Lexy.Compiler.Compiler;
using Lexy.Compiler.Infrastructure;
using Lexy.RunTime;
using Lexy.Tests.Compiler;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Execution;

public class ExecutionLoggingTests : ScopedServicesTestFixture
{
    [Test]
    public void SimpleFunction()
    {
        using var script = ServiceProvider.CompileFunction($@"
function SimpleFunction
# Validate table keywords
  Parameters
    number Value
  Results
    number Result
  Code
    Result = Value * 5");
        var result = script.Run(new Dictionary<string, object> { { "Value", 23} });
        result.Logging.Count.ShouldBe(1, result.Logging.Format(0));
    }

    [Test]
    public void TableVariablesShouldNotStoreFullTableInLogging()
    {
        using var script = ServiceProvider.CompileFunction($@"Table: SimpleTable
# Validate table keywords
  | number Search | number Value |
  | 0 | 0 |
  | 1 | 1 |
function ValidateTableKeywordFunction
# Validate table keywords
  Parameters
  Results
    number Result
  Code
    Result = LOOKUP(SimpleTable, 2, SimpleTable.Search, SimpleTable.Value)");
        ExpectNoTableValuesProperty(script);
    }

    [Test]
    public void TableVariablesShouldNotStoreFullTableInLoggingRow()
    {
        using var script = ServiceProvider.CompileFunction(@"Table: SimpleTable
  | number Search | number Value | string Extra |
  | 0 | 0 | ""ext"" |
  | 1 | 1 | ""ra""  |
 
function ValidateTableKeywordFunction
  Parameters
  Results
    SimpleTable.Row Result
  Code
    Result = LOOKUPROW(SimpleTable, 2, SimpleTable.Search)");

        ExpectNoTableValuesProperty(script);
    }

    private void WalkLogging(IReadOnlyList<ExecutionLogEntry> logging, Action<ExecutionLogEntry> handler)
    {
        foreach (var log in logging)
        {
            handler(log);
            WalkLogging(log.Entries, handler);
        }
    }

    private void ExpectNoTableValuesProperty(CompilerExtensions.CompileFunctionResult script)
    {
        var result = script.Run();
        WalkLogging(result.Logging, log =>
        {
            Console.Write(JsonSerializer.Serialize(log));
            var table = log.ReadVariables["SimpleTable"];
            if (table == null) return;

            var values = table.LogVariables?[LexyCodeConstants.ValuesVariable];
            if (values != null)
            {
                throw new InvalidOperationException("Table values should not be stored:" +
                                                    JsonSerializer.Serialize(values));
            }
        });
    }
}