using System.Collections.Generic;

namespace Lexy.Poc.Core.Compiler.CSharp
{
    public class ValidateTableKeyword
    {
        public class ValidateTableKeywordRow
        {
            public int Value { get; set; }
            public int Result { get; set; }
        }
        private static IList<ValidateTableKeywordRow> _value = new List<ValidateTableKeywordRow>();
        static ValidateTableKeyword()
        {
            _value.Add(new ValidateTableKeywordRow
            {
                Value = 0,
                Result = 0,
            });
            _value.Add(new ValidateTableKeywordRow
            {
                Value = 1,
                Result = 1,
            });
        }

        public ValidateTableKeyword()
        {
        }

        public int Count => _value.Count;
        public IEnumerable<ValidateTableKeywordRow> Value => _value;
    }
}