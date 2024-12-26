using System;
using Lexy.RunTime.RunTime;

namespace Lexy.Compiler
{
    public static class Function__CallSimpleFunctionExplicitParametersFunction
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
            var __result = new __Result();
            __context.LogDebug("32:       var parameters = fill(SimpleFunction.Parameters)");
            Function__SimpleFunction.__Parameters parameters = new Function__SimpleFunction.__Parameters();
            parameters.Value = __parameters.Value;
            __context.LogDebug("33:       var results = SimpleFunction(parameters)");
            Function__SimpleFunction.__Result results = Function__SimpleFunction.__Run(parameters, __context);
            __context.LogDebug("34:       extract(results)");
            new System.DateTime();
            return __result;
        }
    }
}
