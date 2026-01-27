using System;
using System.Collections.Generic;
using Lexy.RunTime;

namespace Lexy.Runtime
{
    public static class Function__Calling
    {
        public class __Parameters
        {
            public decimal Value = default(decimal);
        }

        public class __Result
        {
            public decimal Result = default(decimal);
            public string Message = "";
        }

        public static __Result __Run(__Parameters __parameters, IExecutionContext __context)
        {
            if (__parameters == null)
                throw new ArgumentNullException(nameof(__parameters));
            if (__context == null)
                throw new ArgumentNullException(nameof(__context));
            __context.SetFileName("tests.lexy");
            __context.OpenScope("Execute: Calling", 2);
            __context.LogVariables("Parameters", 3, __parameters);
            var __result = new __Result();
            var __logLine7 = __context.LogLine("Result = Value + 7", 7, LogVariablesBuilder.New().AddVariable("Value", __parameters.Value).Build());
            __result.Result = __parameters.Value + 7m;
            __logLine7.AddWriteVariables(LogVariablesBuilder.New().AddVariable("Result", __result.Result).Build());
            __context.LogVariables("Results", 5, __result);
            __context.CloseScope();
            return __result;
        }

        public static __Result __Run(decimal Value, IExecutionContext __context)
        {
            var __parameters = new __Parameters();
            __parameters.Value = Value;
            return __Run(__parameters, __context);
        }
    }

    public static class Function__Caller
    {
        public class __Parameters
        {
            public decimal Value = default(decimal);
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
            __context.SetFileName("tests.lexy");
            __context.OpenScope("Execute: Caller", 10);
            __context.LogVariables("Parameters", 11, __parameters);
            var __result = new __Result();
            var __logLine14 = __context.LogLine("Calling.Parameters params", 14, LogVariablesBuilder.New().Build());
            Function__Calling.__Parameters @params = new Function__Calling.__Parameters();
            __logLine14.AddWriteVariables(LogVariablesBuilder.New().AddVariable("params", @params).Build());
            var __logLine15 = __context.LogLine("params.Value = Value", 15, LogVariablesBuilder.New().AddVariable("Value", __parameters.Value).Build());
            @params.Value = __parameters.Value;
            __logLine15.AddWriteVariables(LogVariablesBuilder.New().AddVariable("params.Value", @params.Value).Build());
            var __logLine16 = __context.LogLine("... = Calling(params)", 16, LogVariablesBuilder.New().AddVariable("params", @params).Build());
            Function__Calling.__Result __result_17 = Function__Calling.__Run(@params, __context);
            __result.Result = __result_17.Result;
            __logLine16.AddWriteVariables(LogVariablesBuilder.New().Build());
            __context.LogVariables("Results", 13, __result);
            __context.CloseScope();
            return __result;
        }

        public static __Result __Run(decimal Value, IExecutionContext __context)
        {
            var __parameters = new __Parameters();
            __parameters.Value = Value;
            return __Run(__parameters, __context);
        }
    }
}
