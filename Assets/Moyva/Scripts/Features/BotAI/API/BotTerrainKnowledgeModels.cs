using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.API
{
    public readonly struct BotTerrainCellSnapshot
    {
        public BotTerrainCellSnapshot(
            Vector2Int cell,
            string tileId,
            string objectId,
            float height,
            int terrainLevel)
        {
            Cell = cell;
            TileId = Normalize(tileId);
            ObjectId = Normalize(objectId);
            Height = height;
            TerrainLevel = terrainLevel;
        }

        public Vector2Int Cell { get; }
        public string TileId { get; }
        public string ObjectId { get; }
        public float Height { get; }
        public int TerrainLevel { get; }

        public bool HasObject => !string.IsNullOrEmpty(ObjectId);

        private static string Normalize(string value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    public readonly struct BotSiteScoreFactor
    {
        public BotSiteScoreFactor(
            string key,
            string label,
            float rawValue,
            int weight,
            int contribution,
            string detail)
        {
            Key = Normalize(key);
            Label = Normalize(label);
            RawValue = rawValue;
            Weight = weight;
            Contribution = contribution;
            Detail = Normalize(detail);
        }

        public string Key { get; }
        public string Label { get; }
        public float RawValue { get; }
        public int Weight { get; }
        public int Contribution { get; }
        public string Detail { get; }

        private static string Normalize(string value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    public sealed class BotSiteEvaluation
    {
        public BotSiteEvaluation(
            Vector2Int cell,
            bool placementAllowed,
            int totalScore,
            string summary,
            IReadOnlyList<BotSiteScoreFactor> factors)
        {
            Cell = cell;
            PlacementAllowed = placementAllowed;
            TotalScore = totalScore;
            Summary = string.IsNullOrWhiteSpace(summary) ? string.Empty : summary.Trim();
            Factors = factors ?? Array.Empty<BotSiteScoreFactor>();
        }

        public Vector2Int Cell { get; }
        public bool PlacementAllowed { get; }
        public int TotalScore { get; }
        public string Summary { get; }
        public IReadOnlyList<BotSiteScoreFactor> Factors { get; }
    }

    public interface IBotTerrainKnowledge
    {
        bool IsReady { get; }
        int Width { get; }
        int Height { get; }
        long StartupSequence { get; }

        bool Contains(Vector2Int cell);
        bool TryGetCell(Vector2Int cell, out BotTerrainCellSnapshot snapshot);
        IReadOnlyList<Vector2Int> GetNeighbors(Vector2Int cell, int radius);
    }

    public interface IBotCastleSiteEvaluator
    {
        BotSiteEvaluation Evaluate(
            BotWorldSnapshot snapshot,
            string castleBuildingId,
            Vector2Int cell,
            bool placementAllowed);
    }
}
