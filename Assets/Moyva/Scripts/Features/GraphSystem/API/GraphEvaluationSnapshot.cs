using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.API
{
    /// <summary>
    /// Immutable identity of one deterministic graph evaluation. Editor previews
    /// and runtime consumers can compare Revision/Seed/MapSize before applying it.
    /// </summary>
    public sealed class GraphEvaluationSnapshot
    {
        private readonly Dictionary<string, bool[,]> _compiledLayerMatrices;
        private readonly Dictionary<string, object> _layerOutputs;
        private readonly Dictionary<string, NodeEvaluationRecord> _nodeRecords;

        public GraphEvaluationSnapshot(
            GraphExecutionResult executionResult,
            int seed,
            Vector2Int mapSize,
            long revision,
            IReadOnlyDictionary<string, bool[,]> compiledLayerMatrices = null,
            string diagnostics = null,
            IReadOnlyDictionary<string, object> layerOutputs = null,
            GraphAsset sourceGraph = null)
        {
            ExecutionResult = executionResult;
            SourceGraph = sourceGraph;
            Seed = GlobalSeed.Normalize(seed);
            MapSize = new Vector2Int(Mathf.Max(1, mapSize.x), Mathf.Max(1, mapSize.y));
            Revision = revision;
            Diagnostics = diagnostics;
            _compiledLayerMatrices = new Dictionary<string, bool[,]>();
            if (compiledLayerMatrices != null)
            {
                foreach (var pair in compiledLayerMatrices)
                {
                    if (!string.IsNullOrEmpty(pair.Key) && pair.Value != null)
                        _compiledLayerMatrices[pair.Key] = Clone(pair.Value);
                }
            }

            _layerOutputs = new Dictionary<string, object>();
            if (layerOutputs != null)
            {
                foreach (var pair in layerOutputs)
                {
                    if (!string.IsNullOrEmpty(pair.Key) && pair.Value != null)
                        _layerOutputs[pair.Key] = pair.Value;
                }
            }

            _nodeRecords = BuildNodeRecords(executionResult);
        }

        public GraphExecutionResult ExecutionResult { get; }
        public GraphAsset SourceGraph { get; }
        public int Seed { get; }
        public Vector2Int MapSize { get; }
        public long Revision { get; }
        public string Diagnostics { get; }
        public bool Success => ExecutionResult != null && ExecutionResult.Success;
        public IReadOnlyDictionary<string, bool[,]> CompiledLayerMatrices => _compiledLayerMatrices;
        public IReadOnlyDictionary<string, object> LayerOutputs => _layerOutputs;
        public IReadOnlyDictionary<string, NodeEvaluationRecord> NodeRecords => _nodeRecords;

        public object[] GetNodeOutputs(string nodeId) =>
            !string.IsNullOrEmpty(nodeId)
            && _nodeRecords.TryGetValue(nodeId, out var record)
                ? record.Outputs
                : null;
        public T GetNodeArtifact<T>(string nodeId) where T : class =>
            !string.IsNullOrEmpty(nodeId)
            && _nodeRecords.TryGetValue(nodeId, out var record)
                ? record.Artifact as T
                : null;
        public T GetLayerOutput<T>(string layerId) where T : class =>
            !string.IsNullOrEmpty(layerId)
            && _layerOutputs.TryGetValue(layerId, out var output)
                ? output as T
                : null;

        public bool IsCompatibleWith(
            GraphAsset graph,
            int seed,
            Vector2Int mapSize)
        {
            return Success
                   && SourceGraph == graph
                   && Seed == GlobalSeed.Normalize(seed)
                   && MapSize == new Vector2Int(
                       Mathf.Max(1, mapSize.x),
                       Mathf.Max(1, mapSize.y));
        }

        private static bool[,] Clone(bool[,] source)
        {
            int width = source.GetLength(0);
            int height = source.GetLength(1);
            var clone = new bool[width, height];
            System.Array.Copy(source, clone, source.Length);
            return clone;
        }

        private static Dictionary<string, NodeEvaluationRecord> BuildNodeRecords(
            GraphExecutionResult result)
        {
            var records = new Dictionary<string, NodeEvaluationRecord>();
            if (result == null)
                return records;

            var logs = result.Logs?
                .Where(log => log != null && !string.IsNullOrEmpty(log.NodeId))
                .GroupBy(log => log.NodeId)
                .ToDictionary(group => group.Key, group => group.Last())
                ?? new Dictionary<string, NodeExecutionLog>();
            var nodeIds = new HashSet<string>(
                result.ExecutionOrderNodeIds
                ?? Array.Empty<string>());
            foreach (string nodeId in logs.Keys)
                nodeIds.Add(nodeId);

            foreach (string nodeId in nodeIds)
            {
                if (string.IsNullOrEmpty(nodeId))
                    continue;

                logs.TryGetValue(nodeId, out var log);
                records[nodeId] = new NodeEvaluationRecord(
                    nodeId,
                    result.GetOutputs(nodeId),
                    result.GetArtifact(nodeId),
                    log);
            }

            return records;
        }
    }

    /// <summary>
    /// Фактичний результат одного вузла в межах конкретної revision.
    /// Масив портів копіюється, тому presentation-код не може змінити його склад.
    /// </summary>
    public sealed class NodeEvaluationRecord
    {
        private readonly object[] _outputs;

        internal NodeEvaluationRecord(
            string nodeId,
            object[] outputs,
            object artifact,
            NodeExecutionLog log)
        {
            NodeId = nodeId;
            _outputs = outputs != null
                ? (object[])outputs.Clone()
                : null;
            Artifact = artifact;
            Log = log;
        }

        public string NodeId { get; }
        public object[] Outputs => _outputs != null
            ? (object[])_outputs.Clone()
            : null;
        public object Artifact { get; }
        public NodeExecutionLog Log { get; }
        public bool IsConnectedToOutput => Log?.IsConnectedToOutput ?? false;
    }
}
