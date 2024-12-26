using System.Collections.Generic;
using Lexy.Poc.Core.RunTime;

namespace Lexy.Runtime
{
    public class FunctionTestSimpleReturn
    {
        public decimal Input = 5m;
        public decimal Result = default(decimal);
        public Lexy.Poc.Core.RunTime.FunctionResult __Result()
        {
            var result = new Lexy.Poc.Core.RunTime.FunctionResult();
            result["Result"] = Result;
            return result;
        }

        public void __Run(IExecutionContext context)
        {
            context.LogDebug("8:     Result = Input");
            var Results = 789 - -456;

            var a = BuiltInNumberFunctions.Int(8);
        }
    }
}