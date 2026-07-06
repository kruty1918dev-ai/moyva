using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    [DisallowMultipleComponent]
    internal sealed class TileWorldCreatorChunkAuditReporter : MonoBehaviour
    {
        private const string LogTag = "[MoyvaTWCChunks]";
        private const string WorldGenDiagTag = "[MoyvaWorldGenDiag]";
        private readonly List<string> _samples = new List<string>(8);

        public void Report(TileWorldCreatorManager manager, Configuration configuration, string phase)
        {
            if (manager == null || configuration == null)
                return;

            ClusterIdentifier[] clusters = manager.GetComponentsInChildren<ClusterIdentifier>(true);
            int expected = EstimateExpectedChunks(configuration);
            _samples.Clear();
            for (int i = 0; i < clusters.Length && _samples.Count < 8; i++)
            {
                if (clusters[i] == null)
                    continue;

                Bounds bounds = CollectBounds(clusters[i].transform);
                _samples.Add($"{clusters[i].name}:center={Format(bounds.center)},size={Format(bounds.size)}");
            }

            string message = $"phase={phase}, expectedChunks={expected}, actualClusters={clusters.Length}, " +
                $"map={configuration.width}x{configuration.height}, clusterCellSize={configuration.clusterCellSize}, " +
                $"mergeTiles={configuration.mergeTiles}, layers={TileWorldCreatorChunkBatchingUtility.DescribeActiveTileLayers(configuration)}, samples=[{string.Join(" | ", _samples)}]";
            Debug.Log($"{LogTag} Audit {message}");
            Debug.Log($"{WorldGenDiagTag} TWCChunkAudit {message}");
        }

        public void RequestDelayedReport(TileWorldCreatorManager manager, Configuration configuration, string phase)
        {
            StopAllCoroutines();
            StartCoroutine(ReportDelayed(manager, configuration, phase));
        }

        private System.Collections.IEnumerator ReportDelayed(TileWorldCreatorManager manager, Configuration configuration, string phase)
        {
            yield return null;
            yield return null;
            Report(manager, configuration, phase);
        }

        private static int EstimateExpectedChunks(Configuration configuration)
        {
            int chunkSize = Mathf.Max(1, configuration.clusterCellSize);
            return Mathf.CeilToInt(Mathf.Max(1, configuration.width) / (float)chunkSize)
                * Mathf.CeilToInt(Mathf.Max(1, configuration.height) / (float)chunkSize);
        }

        private static Bounds CollectBounds(Transform root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            Bounds bounds = default;
            bool hasBounds = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                    continue;

                if (hasBounds)
                    bounds.Encapsulate(renderers[i].bounds);
                else
                    bounds = renderers[i].bounds;

                hasBounds = true;
            }

            return bounds;
        }

        private static string Format(Vector3 value)
            => $"({value.x:0.##},{value.y:0.##},{value.z:0.##})";
    }
}
