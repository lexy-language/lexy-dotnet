/*using System;
using System.Collections.Generic;
using Lexy.RunTime;

namespace Lexy.Runtime
{
    public static class Function__LookUpDefaultColumnNumberFunction
    {
        public class __Parameters
        {
            public System.DateTime Value = new System.DateTime(1, 1, 1, 0, 0, 0);
        }

        public class __Result
        {
            public decimal Result = default(decimal);
        }

        public static __Result __Run(__Parameters __parameters, IExecutionContext __context)
        {
            if (__parameters == null)
                throw new ArgumentNullException(nameof(__parameters));
            if (__context == null)
                throw new ArgumentNullException(nameof(__context));
            __context.SetFileName("LookupRowDate.lexy");
            __context.OpenScope("Execute: LookUpDefaultColumnNumberFunction", 94);
            __context.LogVariables("Parameters", 94, __parameters);
            var __result = new __Result();
            var __logLine98 = __context.LogLine("var row = DateTable.LookUpRow(Value)", 98, LogVariablesBuilder.New().AddVariable("Value", __parameters.Value).Build());
            Table__DateTable.__Row row = Lexy.RunTime.Libraries.Table.LookUpRow("SearchValue", __parameters.Value, row => row.SearchValue, "DateTable", Table__DateTable.Values, __context);
            __logLine98.AddWriteVariables(LogVariablesBuilder.New().AddVariable("row", row).Build());
            var __logLine99 = __context.LogLine("Result = row.ResultNumber", 99, LogVariablesBuilder.New().AddVariable("row.ResultNumber", row.ResultNumber).Build());
            __result.Result = row.ResultNumber;
            __logLine99.AddWriteVariables(LogVariablesBuilder.New().AddVariable("Result", __result.Result).Build());
            __context.LogVariables("Results", 94, __result);
            __context.CloseScope();
            return __result;
        }
    }
}*/