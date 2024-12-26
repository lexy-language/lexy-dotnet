using System;
using System.Collections.Generic;

namespace Lexy.Poc.Core.Language
{
    internal static class NodesWalker
    {
        public static IEnumerable<T> Walk<T>(IEnumerable<INode> nodes, Func<INode, T> action)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            if (action == null) throw new ArgumentNullException(nameof(action));

            var result = new List<T>();
            Walk(nodes, action, result);

            return result;
        }

        private static void Walk<T>(INode node, Func<INode, T> action, IList<T> result)
        {
            var actionResult = action(node);
            if (actionResult != null)
            {
                result.Add(actionResult);
            }

            var children = node.GetChildren();

            Walk(children, action, result);
        }

        private static void Walk<T>(IEnumerable<INode> nodes, Func<INode, T> action, IList<T> result)
        {
            foreach (var node in nodes)
            {
                Walk(node, action, result);
            }
        }
    }
}