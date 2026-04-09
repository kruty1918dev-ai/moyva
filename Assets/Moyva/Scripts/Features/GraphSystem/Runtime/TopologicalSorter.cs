using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.GraphSystem.Runtime
{
    /// <summary>
    /// Утиліта для топологічного сортування графів (алгоритм Кана).
    /// Використовується GraphRunner для визначення порядку виконання
    /// та GraphValidator для перевірки наявності циклів.
    /// </summary>
    public static class TopologicalSorter
    {
        /// <summary>
        /// Виконує топологічне сортування вузлів графа алгоритмом Кана.
        /// Повертає відсортований список вузлів або null, якщо граф містить цикли.
        /// </summary>
        public static List<NodeBase> Sort(GraphAsset graph)
        {
            var nodeMap = new Dictionary<string, NodeBase>();
            var inDegree = new Dictionary<string, int>();
            var adjacency = new Dictionary<string, List<string>>();

            foreach (var node in graph.Nodes)
            {
                if (node == null) continue;
                nodeMap[node.NodeId] = node;
                inDegree[node.NodeId] = 0;
                adjacency[node.NodeId] = new List<string>();
            }

            foreach (var conn in graph.Connections)
            {
                if (!nodeMap.ContainsKey(conn.SourceNodeId)
                    || !nodeMap.ContainsKey(conn.TargetNodeId))
                    continue;

                adjacency[conn.SourceNodeId].Add(conn.TargetNodeId);
                inDegree[conn.TargetNodeId]++;
            }

            var queue = new Queue<string>();
            foreach (var kvp in inDegree)
                if (kvp.Value == 0)
                    queue.Enqueue(kvp.Key);

            var sorted = new List<NodeBase>();
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                sorted.Add(nodeMap[current]);

                foreach (var neighbor in adjacency[current])
                {
                    inDegree[neighbor]--;
                    if (inDegree[neighbor] == 0)
                        queue.Enqueue(neighbor);
                }
            }

            if (sorted.Count != nodeMap.Count)
                return null;

            return sorted;
        }
    }
}
