using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Expressions.Functions;

namespace Lexy.Compiler.Parser
{
    public class LexyParser : ILexyParser
    {
        private readonly IParserContext context;
        private readonly ISourceCodeDocument sourceCodeDocument;
        private readonly IParserLogger logger;

        public LexyParser(IParserContext parserContext, ISourceCodeDocument sourceCodeDocument, IParserLogger logger)
        {
            context = parserContext ?? throw new ArgumentNullException(nameof(parserContext));
            this.sourceCodeDocument = sourceCodeDocument ?? throw new ArgumentNullException(nameof(sourceCodeDocument));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ParserResult ParseFile(string fileName, bool throwException = true)
        {
            logger.LogInfo("Parse file: " + fileName);

            var code = File.ReadAllLines(fileName);
            return Parse(code, fileName, throwException);
        }

        public ParserResult Parse(string[] code, string fullFileName, bool throwException = true)
        {
            if (code == null) throw new ArgumentNullException(nameof(code));

            var sourceCodeNode = new SourceCodeNode();
            context.AddFileIncluded(fullFileName);

            ParseDocument(sourceCodeNode, code, fullFileName);

            logger.LogNodes(context.Nodes);

            ValidateNodesTree(sourceCodeNode);
            DetectCircularDependencies();

            if (throwException)
            {
                logger.AssertNoErrors();
            }

            return new ParserResult(context.Nodes);
        }

        private void ParseDocument(SourceCodeNode sourceCodeNode, string[] code, string fullFileName)
        {
            sourceCodeDocument.SetCode(code, Path.GetFileName(fullFileName));

            var currentIndent = 0;
            var nodes = new ParsableNodeArray(sourceCodeNode);

            while (sourceCodeDocument.HasMoreLines())
            {
                if (!context.ProcessLine())
                {
                    currentIndent = sourceCodeDocument.CurrentLine?.Indent(context) ?? currentIndent;
                    continue;
                }

                var line = sourceCodeDocument.CurrentLine;
                if (line.IsComment() || line.IsEmpty())
                {
                    continue;
                }

                var indentResult = line.Indent(context);
                if (!indentResult.HasValue)
                {
                    continue;
                }

                var indent = indentResult.Value;
                if (indent > currentIndent)
                {
                    context.Logger.Fail(context.LineStartReference(), $"Invalid indent: {indent}");
                    continue;
                }

                var node = nodes.Get(indent);
                node = ParseLine(node);

                currentIndent = indent + 1;

                nodes.Set(currentIndent, node);
            }

            Finalize();

            LoadIncludedFiles(fullFileName, sourceCodeNode);
        }

        private void LoadIncludedFiles(string parentFullFileName, SourceCodeNode sourceCodeNode)
        {
            var includes = sourceCodeNode.GetDueIncludes();
            foreach (var include in includes)
            {
                IncludeFiles(parentFullFileName, sourceCodeNode, include);
            }
        }

        private void IncludeFiles(string parentFullFileName, SourceCodeNode sourceCodeNode, Include include)
        {
            var fileName = include.Process(parentFullFileName, context);
            if (fileName == null) return;

            if (context.IsFileIncluded(fileName)) return;

            logger.LogInfo("Parse file: " + fileName);

            var code = File.ReadAllLines(fileName);

            context.AddFileIncluded(fileName);

            ParseDocument(sourceCodeNode, code, fileName);
        }

        private void ValidateNodesTree(SourceCodeNode sourceCodeNode)
        {
            var validationContext = new ValidationContext(context);
            sourceCodeNode.ValidateTree(validationContext);
        }

        private void DetectCircularDependencies()
        {
            var visitedNodes = new List<IRootNode>();
            var node = DetectCircularDependencies(context.Nodes.OfType<IRootNode>(), visitedNodes);
            if (node != null)
            {
                context.Logger.SetCurrentNode(node as IRootNode);
                context.Logger.Fail(node.Reference, $"Circular reference detected in: '{node.NodeName}'");
            }
        }

        private IRootNode DetectCircularDependencies(IEnumerable<IRootNode> nodes, List<IRootNode> visitedNodes)
        {
            foreach (var node in nodes)
            {
                if (visitedNodes.Contains(node))
                {
                    return node;
                }
                visitedNodes.Add(node);

                var circularNode = DetectCircularDependencies(node, visitedNodes);
                if (circularNode != null)
                {
                    return circularNode;
                }
            }

            return null;
        }

        private IRootNode DetectCircularDependencies(IRootNode node, List<IRootNode> visitedNodes)
        {
            var dependencies = GetDependenciesNodes(node);

            if (dependencies == null) return null;

            return DetectCircularDependencies(dependencies, visitedNodes);
        }

        private IEnumerable<IRootNode> GetDependenciesNodes(IRootNode node)
        {
            var resultDependencies = new List<IRootNode>();
            var nodeDependencies = (node as IHasNodeDependencies)?.GetDependencies(context.Nodes);
            if (nodeDependencies != null)
            {
                resultDependencies.AddRange(nodeDependencies);
            }

            var children = node.GetChildren();
            NodesWalker.Walk(children, childNode =>
            {
                var nodeDependencies = (childNode as IHasNodeDependencies)?.GetDependencies(context.Nodes);
                if (nodeDependencies == null) return;

                foreach (var dependency in nodeDependencies)
                {
                    if (dependency == null)
                    {
                        throw new NotImplementedException("node.GetNodes() should never return null");
                    }

                    resultDependencies.Add(dependency);
                }
            });
            return resultDependencies;
        }

        private void Finalize()
        {
            sourceCodeDocument.Reset();
            logger.Reset();
        }

        private IParsableNode ParseLine(IParsableNode currentNode)
        {
            var node = currentNode.Parse(context);
            if (node == null)
            {
                throw new InvalidOperationException(
                    $"({currentNode}) Parse should return child node or itself.");
            }
            return node;
        }
    }
}