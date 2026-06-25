using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes.ObjectPlacement
{
    [NodeInfo("Object Layer", "Object Placement", "Packages scatter candidates, prefab variants, and spawn settings into a generated TWC object layer.")]
    public sealed class ObjectLayerNode : NodeBase, IPreviewableNode
    {
        [SerializeField, InlineEditable("Layer")]
        [Tooltip("Name of the generated TWC blueprint/build object layer.")]
        private string _layerName = "Props";

        [SerializeField]
        [Tooltip("Optional graph terrain layer id this object layer conceptually belongs to.")]
        private string _targetGraphLayerId;

        [SerializeField]
        [Tooltip("Prefab variants used by the generated TWC Object Build Layer.")]
        private List<ObjectPrefabEntry> _prefabs = new();

        [SerializeField]
        [Tooltip("Spawn settings that are mapped to the generated TWC Object Build Layer.")]
        private ObjectPlacementRule _rule = new();

        [SerializeField]
        [Tooltip("Stored with the output layer for editor/debug context. Cluster generation usually happens in Cluster Scatter.")]
        private ClusterSettings _cluster = new();

        [NonSerialized] private ScatterMask _lastMask;
        [NonSerialized] private List<ScatterCandidate> _lastCandidates;

        public override string Title => "Object Layer";
        public override string Category => "Object Placement";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<List<ScatterCandidate>>("Candidates"),
            PortDefinition.Input<bool[,]>("Exclude"),
            PortDefinition.Input<GrassCardSettings>("Grass")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<ObjectPlacementLayer>("Object Layer")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            if (inputs == null || inputs.Length == 0 || inputs[0] is not List<ScatterCandidate> candidates)
                return NodeOutput.Error("Candidates input is required.");

            var exclude = inputs.Length > 1 ? inputs[1] as bool[,] : null;
            var grass = inputs.Length > 2 ? inputs[2] as GrassCardSettings : null;
            var layer = new ObjectPlacementLayer(_layerName)
            {
                TargetGraphLayerId = ResolveTargetGraphLayerId(),
                Rule = _rule ?? new ObjectPlacementRule(),
                Cluster = _cluster ?? new ClusterSettings()
            };

            AddConfiguredPrefabs(layer.Prefabs);
            bool grassConnectedWithoutPrefab = grass != null && grass.Prefab == null;
            AddGrassPrefab(layer.Prefabs, grass);

            var filtered = FilterCandidates(candidates, exclude);
            AssignPrefabIndices(filtered, layer.Prefabs, context?.Seed ?? 1);
            layer.Candidates.AddRange(filtered);

            _lastCandidates = filtered;
            _lastMask = BuildPreviewMask(context, filtered);

            if (layer.Prefabs.Count == 0)
            {
                string grassHint = grassConnectedWithoutPrefab
                    ? " Grass settings are connected, but no generated/assigned grass prefab is set."
                    : string.Empty;
                return NodeOutput.Warning($"Object layer has no prefabs.{grassHint} TWC layer will be skipped.", layer);
            }

            if (layer.Candidates.Count == 0)
                return NodeOutput.Warning(
                    $"Object layer has no candidates after filtering ({candidates.Count} input, {candidates.Count - filtered.Count} excluded).",
                    layer);

            return NodeOutput.Success(layer);
        }

        public Texture2D GeneratePreview(int width, int height)
        {
            return _lastMask == null
                ? null
                : ObjectPlacementPreviewUtility.BuildScatterTexture(_lastMask, _lastCandidates);
        }

        private string ResolveTargetGraphLayerId()
        {
            if (!string.IsNullOrWhiteSpace(_targetGraphLayerId))
                return _targetGraphLayerId.Trim();

            return string.IsNullOrWhiteSpace(LayerId)
                ? null
                : LayerId;
        }

        private void AddConfiguredPrefabs(List<ObjectPrefabEntry> target)
        {
            if (_prefabs == null)
                return;

            for (int i = 0; i < _prefabs.Count; i++)
            {
                var entry = _prefabs[i];
                if (entry?.Prefab == null)
                    continue;

                target.Add(entry);
            }
        }

        private static void AddGrassPrefab(List<ObjectPrefabEntry> target, GrassCardSettings grass)
        {
            if (grass?.Prefab == null)
                return;

            for (int i = 0; i < target.Count; i++)
            {
                if (target[i]?.Prefab == grass.Prefab)
                    return;
            }

            target.Add(new ObjectPrefabEntry
            {
                Prefab = grass.Prefab,
                Weight = 1f,
                MinScale = 0.85f,
                MaxScale = 1.15f,
                RandomYaw = true,
                AlignToSurface = true,
                ClusterAffinity = 1f,
                ColorVariation = grass.Tint
            });
        }

        private static List<ScatterCandidate> FilterCandidates(
            List<ScatterCandidate> candidates,
            bool[,] exclude)
        {
            var result = new List<ScatterCandidate>();
            if (candidates == null)
                return result;

            for (int i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (exclude != null
                    && candidate.Cell.x >= 0
                    && candidate.Cell.y >= 0
                    && candidate.Cell.x < exclude.GetLength(0)
                    && candidate.Cell.y < exclude.GetLength(1)
                    && exclude[candidate.Cell.x, candidate.Cell.y])
                {
                    continue;
                }

                result.Add(candidate);
            }

            return result;
        }

        private static void AssignPrefabIndices(
            List<ScatterCandidate> candidates,
            List<ObjectPrefabEntry> prefabs,
            int seed)
        {
            if (candidates == null || prefabs == null || prefabs.Count == 0)
                return;

            float totalWeight = 0f;
            for (int i = 0; i < prefabs.Count; i++)
                totalWeight += Mathf.Max(0f, prefabs[i]?.Weight ?? 0f);
            if (totalWeight <= 0f)
                totalWeight = prefabs.Count;

            for (int i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (candidate.PrefabIndex >= 0)
                    continue;

                var random = new System.Random(unchecked(seed + candidate.Cell.x * 73856093 ^ candidate.Cell.y * 19349663));
                float roll = (float)random.NextDouble() * totalWeight;
                float cumulative = 0f;
                int selected = 0;
                for (int p = 0; p < prefabs.Count; p++)
                {
                    cumulative += Mathf.Max(0f, prefabs[p]?.Weight ?? 0f);
                    if (roll <= cumulative)
                    {
                        selected = p;
                        break;
                    }
                }

                candidates[i] = candidate.WithPrefabIndex(selected);
            }
        }

        private static ScatterMask BuildPreviewMask(NodeContext context, List<ScatterCandidate> candidates)
        {
            int w = Mathf.Max(1, context?.MapSize.x ?? 0);
            int h = Mathf.Max(1, context?.MapSize.y ?? 0);
            if (candidates != null)
            {
                for (int i = 0; i < candidates.Count; i++)
                {
                    w = Mathf.Max(w, candidates[i].Cell.x + 1);
                    h = Mathf.Max(h, candidates[i].Cell.y + 1);
                }
            }

            var placement = new bool[w, h];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                    placement[x, y] = true;
            }

            return new ScatterMask(placement);
        }
    }
}
