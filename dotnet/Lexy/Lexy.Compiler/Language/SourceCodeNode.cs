using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;

namespace Lexy.Poc.Core.Language
{
    public class SourceCodeNode : RootNode
    {
        private readonly IList<IRootNode> nodes = new List<IRootNode>();
        private readonly IList<Include> includes = new List<Include>();

        public override string NodeName => "SourceCodeNode";

        public Comments Comments { get; }

        public SourceCodeNode() : base(new SourceReference(new SourceFile("SourceCodeNode"), 1, 1))
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
                context.Logger.Fail(context.LineReference(0),$"Unexpected line: {line}");
                return this;
            }

            var rootNode = ParseRootNode(context, line);
            if (rootNode == null) return this;

            nodes.Add(rootNode);
            context.ProcessNode(rootNode);

            return rootNode;
        }

        private IRootNode ParseRootNode(IParserContext context, Line line)
        {
            if (Include.IsValid(line))
            {
                var include = Include.Parse(line, context);
                if (include != null)
                {
                    includes.Add(include);
                    return null;
                }
            }

            var tokenName = Parser.NodeName.Parse(line, context);
            if (tokenName == null) return null;

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

            return rootNode;
        }

        private IRootNode InvalidNode(NodeName tokenName, IParserContext context, SourceReference reference)
        {
            context.Logger.Fail(reference, $"Unknown keyword: {tokenName.Keyword}");
            return null;
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

        public IEnumerable<Include> GetDueIncludes() => includes.Where(include => !include.IsProcessed).ToList();
    }

    public class Include
    {
        private readonly SourceReference reference;

        public bool IsProcessed { get; private set; }
        public string FileName { get; }

        private Include(string fileName, SourceReference reference)
        {
            this.reference = reference;
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        }

        public static bool IsValid(Line line)
        {
            return line.Tokens.IsKeyword(0, Keywords.Include);
        }

        public static Include Parse(Line line, IParserContext parserContext)
        {
            var lineTokens = line.Tokens;
            if (lineTokens.Length != 2 || !lineTokens.IsQuotedString(1))
            {
                parserContext.Logger.Fail(parserContext.LineStartReference(), "Invalid syntax. Expected: 'Include \"FileName\"");
                return null;
            }

            var quotedString = lineTokens.Token<QuotedLiteralToken>(1);

            return new Include(quotedString.Value, parserContext.LineStartReference());
        }

        public string Process(string parentFullFileName, IParserContext context)
        {
            IsProcessed = true;
            if (string.IsNullOrEmpty(FileName))
            {
                context.Logger.Fail(reference, "No include file name specified.");
                return null;
            }

            var directName = Path.GetDirectoryName(parentFullFileName);
            var fullPath = Path.GetFullPath(directName);
            var fullFinName = $"{Path.Combine(fullPath, FileName)}.{LexySourceDocument.FileExtension}";

            if (!File.Exists(fullFinName))
            {
                context.Logger.Fail(reference, $"Invalid include file name '{FileName}'");
                return null;
            }

            return fullFinName;
        }
    }
}