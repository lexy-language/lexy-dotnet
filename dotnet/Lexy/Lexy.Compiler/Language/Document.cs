using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class Document : RootNode
    {
        private readonly IList<IRootNode> nodes = new List<IRootNode>();

        public override string NodeName => "Document";

        public Comments Comments { get; }

        public Document(SourceReference reference) : base(reference)
        {
            Comments = new Comments(Reference);
        }

        public override IParsableNode Parse(IParserContext context)
        {
            var line = context.CurrentLine;

            if (line.IsEmpty()) return this;
            if (line.Tokens.IsComment()) return Comments;

            var indent = line.Indent(context);
            if (indent == null || indent > 0)
            {
                context.Logger.Fail(context.DocumentReference(),$"Unexpected line: {line}");
                return this;
            }

            var tokenName = Parser.NodeName.Parse(line, context);
            if (tokenName == null) return this;

            var reference = context.LineStartReference();
            var rootNode = tokenName.Keyword switch
            {
                null => null,
                Keywords.FunctionKeyword => Function.Create(tokenName.Name, reference),
                Keywords.EnumKeyword => EnumDefinition.Parse(tokenName, reference),
                Keywords.ScenarioKeyword => Scenario.Parse(tokenName, reference),
                Keywords.TableKeyword => Table.Parse(tokenName, reference),
                _ => InvalidNode(tokenName, context, reference)
            };

            if (rootNode == null) return this;

            nodes.Add(rootNode);
            context.ProcessNode(rootNode);

            return rootNode;
        }

        private IRootNode InvalidNode(NodeName tokenName, IParserContext context, SourceReference reference)
        {
            context.Logger.Fail(reference, $"Unknown keyword: {tokenName.Keyword}");
            return this;
        }

        public override IEnumerable<INode> GetChildren() => nodes;

        protected override void Validate(IValidationContext context)
        {
            DuplicateChecker.ValidateNode(
                context,
                node => node.Reference,
                node => node.NodeName,
                node => $"Duplicated node name: '{node.NodeName}'",
                nodes);
        }
    }
}