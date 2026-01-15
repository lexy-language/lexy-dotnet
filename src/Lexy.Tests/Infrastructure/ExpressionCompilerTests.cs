using System.Threading.Tasks;
using Lexy.Compiler;
using Lexy.Tests.Compiler;
using Lexy.Tests.Libraries;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Infrastructure;

public class ExpressionCompilerTests : ScopedServicesTestFixture
{
    public class Sub2Model
    {
        public int Sub2Property { get; set; }
    }

    public class SubModel
    {
        public int SubProperty { get; set; }
        public Sub2Model Inner2 { get; set; }
    }

    public class Model
    {
        public int Property { get; set; }
        public SubModel Inner { get; set; }
    }

    [Test]
    public void CompilePropertyOfModel()
    {
        var model = new Model { Property = 9 };
        var (value, message) = ExpressionCompilerExtensions.CompileExpression(model => model.Property, model);

        message.ShouldBe("model.Property");

        value.ShouldBe(9);
    }

    [Test]
    public void CompilePropertyOfInnerModel()
    {
        var model = new Model { Inner = new SubModel { SubProperty = 77} };
        var (value, message) = ExpressionCompilerExtensions.CompileExpression(model => model.Inner.SubProperty, model);

        message.ShouldBe("model.Inner.SubProperty");

        value.ShouldBe(77);
    }

    [Test]
    public void CompilePropertyOfInnerInnerModel()
    {
        var model = new Model { Inner = new SubModel { Inner2 = new Sub2Model { Sub2Property = 9} } };
        var (value, message) = ExpressionCompilerExtensions.CompileExpression(model => model.Inner.Inner2.Sub2Property, model);

        message.ShouldBe("model.Inner.Inner2.Sub2Property");

        value.ShouldBe(9);
    }
}
