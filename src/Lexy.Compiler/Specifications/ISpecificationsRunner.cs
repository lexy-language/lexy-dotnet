using System.Threading.Tasks;

namespace Lexy.Compiler.Specifications;

public interface ISpecificationsRunner
{
    Task Run(string folder);
    Task RunAll(string file);
}
