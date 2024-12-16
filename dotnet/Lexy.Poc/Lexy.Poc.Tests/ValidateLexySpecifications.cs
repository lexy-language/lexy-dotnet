using Lexy.Poc.Core.Specifications;
using NUnit.Framework;

namespace Lexy.Poc
{
    public class ValidateLexySpecificationsTest
    {
        [Test]
        public void AllSpecifications()
        {
            var runner = new SpecificationsRunner();
            runner.RunAll("../../../../../../laws/Specifications");
        }
    }
}