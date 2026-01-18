namespace Lexy.Compiler.Language.TypeSystem.Functions;

internal interface IOverloadArguments
{
    public int? Discriminator { get; }
    public int? DiscriminatorColumnArgument { get; }
    public int? DefaultDiscriminatorColumn { get; }
}