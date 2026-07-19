using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes.ObjectPlacement
{
    [NodeInfo(
        "Object Layer",
        "Objects",
        "Перетворює кандидатів, варіанти prefab та правила появи на згенерований TWC object layer.",
        StableId = "moyva.objects.layer",
        Order = 40,
        PreviewOutput = "out.object_layer")]
    public sealed class ObjectLayerNode : NodeBase
    {
        [SerializeField, InlineEditable("шар")]
        [Tooltip("Назва згенерованого TWC blueprint/build object layer.")]
        private string _layerName = "Props";

        [SerializeField]
        [Tooltip("Опціональний id графічного шару, до якого логічно належить цей шар об'єктів.")]
        private string _targetGraphLayerId;

        [SerializeField]
        [Tooltip("Варіанти префабів, використовуються у згенерованому TWC Object Build Layer.")]
        private List<ObjectPrefabEntry> _prefabs = new();

        [SerializeField]
        [Tooltip("Налаштування появи, які мапляться у згенерований TWC Object Build Layer.")]
        private ObjectPlacementRule _rule = new();

        [SerializeField]
        [Tooltip("Зберігається разом з вихідним шаром для редактора/дебагу. Генерація кластерів зазвичай відбувається в Cluster Scatter.")]
        private ClusterSettings _cluster = new();

        public override string Title => "Object Layer";
        public override string Category => "Objects";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<List<ScatterCandidate>>("Candidates", "in.candidates"),
            PortDefinition.OptionalInput<bool[,]>("Exclusion Mask", "in.exclusion_mask"),
            PortDefinition.OptionalInput<GrassCardSettings>("Grass", "in.grass")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<ObjectPlacementLayer>("Object Layer", "out.object_layer")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            if (inputs == null || inputs.Length == 0 || inputs[0] is not List<ScatterCandidate> candidates)
                return NodeOutput.Error("Вхідні кандидати є обов'язковими.");

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

            if (layer.Prefabs.Count == 0)
            {
                string grassHint = grassConnectedWithoutPrefab
                    ? " Налаштування трави підключені, але не встановлено згенерований/призначений префаб трави."
                    : string.Empty;
                return NodeOutput.Warning($"Шар об'єктів не має префабів.{grassHint} Шар TWC буде пропущений.", layer);
            }

            if (layer.Candidates.Count == 0)
                return NodeOutput.Warning(
                    $"Шар об'єктів не має кандидатів після фільтрації ({candidates.Count} вхідних, {candidates.Count - filtered.Count} виключених).",
                    layer);

            return NodeOutput.Success(layer);
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

    }
}
