using System.Collections;

namespace Lexy.Poc.Core.RunTime
{
    public class FunctionResult
    {
        private readonly IDictionary values = new Hashtable();

        public object this[string name]
        {
            get => values[name];
            set => values[name] = value;
        }

        public decimal Number(string name)
        {
            return (decimal)values[name];
        }
    }
}