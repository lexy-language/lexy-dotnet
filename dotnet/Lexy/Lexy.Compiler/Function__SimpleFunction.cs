using System;
using Lexy.RunTime.RunTime;

namespace Lexy.Compiler
{
    public static class Function__SimpleFunction
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
            __context.LogDebug("7:     Result = Value");
            __result.Result = __parameters.Value;
            return __result;
        }
    }
}