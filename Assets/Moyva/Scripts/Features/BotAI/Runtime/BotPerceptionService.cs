using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotPerceptionService : IBotPerceptionService
    {
        internal const int StartVisionRadius = 4;
        internal const int UnitVisionRadius = 5;
        internal const int BuildingVisionRadius = 3;

        private sealed class OwnerVision
        {
            public readonly HashSet<Vector2Int> Visible = new();
            public readonly HashSet<Vector2Int> Explored = new();
        }

        private readonly Dictionary<string, OwnerVision> _owners =
            new(StringComparer.Ordinal);

        private readonly IBotTerrainKnowledge _terrain;

        [Inject]
        public BotPerceptionService(
            [InjectOptional] IBotTerrainKnowledge terrain = null)
        {
            _terrain = terrain;
        }

        public void Refresh(
            string ownerId,
            Vector2Int startPosition,
            IReadOnlyList<BotUnitSnapshot> ownUnits,
            IReadOnlyList<BotBuildingSnapshot> ownBuildings)
        {
            string owner = Normalize(ownerId);
            OwnerVision vision = GetOrCreate(owner);
            vision.Visible.Clear();

            RevealFrom(vision.Visible, startPosition, StartVisionRadius);

            if (ownUnits != null)
            {
                for (int i = 0; i < ownUnits.Count; i++)
                    RevealFrom(vision.Visible, ownUnits[i].Position, UnitVisionRadius);
            }

            if (ownBuildings != null)
            {
                for (int i = 0; i < ownBuildings.Count; i++)
                    RevealFrom(vision.Visible, ownBuildings[i].Position, BuildingVisionRadius);
            }

            vision.Explored.UnionWith(vision.Visible);
        }

        public bool IsVisible(string ownerId, Vector2Int cell)
            => TryGet(ownerId, out OwnerVision vision) &&
               vision.Visible.Contains(cell);

        public bool IsExplored(string ownerId, Vector2Int cell)
            => TryGet(ownerId, out OwnerVision vision) &&
               vision.Explored.Contains(cell);

        public IReadOnlyCollection<Vector2Int> GetVisibleCells(string ownerId)
        {
            if (!TryGet(ownerId, out OwnerVision vision))
                return Array.Empty<Vector2Int>();

            var result = new List<Vector2Int>(vision.Visible);
            result.Sort(CompareCell);
            return result;
        }

        public IReadOnlyCollection<Vector2Int> GetExploredCells(string ownerId)
        {
            if (!TryGet(ownerId, out OwnerVision vision))
                return Array.Empty<Vector2Int>();

            var result = new List<Vector2Int>();
            foreach (Vector2Int cell in vision.Explored)
            {
                if (!vision.Visible.Contains(cell))
                    result.Add(cell);
            }

            result.Sort(CompareCell);
            return result;
        }

        private void RevealFrom(
            HashSet<Vector2Int> visible,
            Vector2Int source,
            int radius)
        {
            radius = Math.Max(1, radius);

            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    Vector2Int target = new(source.x + dx, source.y + dy);
                    int distance = Math.Max(Math.Abs(dx), Math.Abs(dy));
                    if (distance > radius)
                        continue;

                    if (_terrain != null &&
                        _terrain.IsReady &&
                        !_terrain.Contains(target))
                    {
                        continue;
                    }

                    if (HasTerrainLineOfSight(source, target))
                        visible.Add(target);
                }
            }
        }

        private bool HasTerrainLineOfSight(
            Vector2Int source,
            Vector2Int target)
        {
            if (_terrain == null ||
                !_terrain.IsReady ||
                !_terrain.TryGetCell(source, out BotTerrainCellSnapshot from) ||
                !_terrain.TryGetCell(target, out BotTerrainCellSnapshot to))
            {
                return true;
            }

            int dx = target.x - source.x;
            int dy = target.y - source.y;
            int steps = Math.Max(Math.Abs(dx), Math.Abs(dy));
            if (steps <= 1)
                return true;

            float maxEndLevel = Math.Max(from.TerrainLevel, to.TerrainLevel) + 1f;
            float maxEndHeight = Math.Max(from.Height, to.Height) + 1.25f;

            for (int step = 1; step < steps; step++)
            {
                float t = step / (float)steps;
                int x = Mathf.RoundToInt(Mathf.Lerp(source.x, target.x, t));
                int y = Mathf.RoundToInt(Mathf.Lerp(source.y, target.y, t));
                var sampleCell = new Vector2Int(x, y);

                if (!_terrain.TryGetCell(sampleCell, out BotTerrainCellSnapshot sample))
                    continue;

                if (sample.TerrainLevel > maxEndLevel ||
                    sample.Height > maxEndHeight)
                {
                    return false;
                }

                if (ContainsAny(sample.ObjectId, "mountain", "cliff"))
                    return false;
            }

            return true;
        }

        private OwnerVision GetOrCreate(string owner)
        {
            if (!_owners.TryGetValue(owner, out OwnerVision vision))
            {
                vision = new OwnerVision();
                _owners.Add(owner, vision);
            }
            return vision;
        }

        private bool TryGet(string ownerId, out OwnerVision vision)
            => _owners.TryGetValue(Normalize(ownerId), out vision);

        private static bool ContainsAny(string value, params string[] tokens)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            for (int i = 0; i < tokens.Length; i++)
            {
                if (value.IndexOf(tokens[i], StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        private static string Normalize(string value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

        private static int CompareCell(Vector2Int left, Vector2Int right)
        {
            int x = left.x.CompareTo(right.x);
            return x != 0 ? x : left.y.CompareTo(right.y);
        }
    }
}
