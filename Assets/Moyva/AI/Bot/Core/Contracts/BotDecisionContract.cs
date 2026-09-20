using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Bot
{
    public enum BotIntentType { None = 0, EndTurn = 1, Move = 2, Attack = 3, Capture = 4, Recruit = 5, Build = 6, Explore = 7, Defend = 8, Economy = 9, Reposition = 10, Wait = 11 }
    public enum BotCapabilityId { Turn = 0, Movement = 1, Combat = 2, Recruitment = 3, Construction = 4, Capture = 5, Economy = 6, Exploration = 7 }
    public static class BotDecisionContract
    {
        public const int ContractVersion = 3, ObservationSchemaVersion = 4, CandidateSchemaVersion = 2, ActionSchemaVersion = 1;
        public const int MaxCandidateSlots = 128, GlobalFeatureCount = 96, CandidateFeatureCount = 32;
        public const int SpatialSize = 8, SpatialChannels = 10, SpatialFeatureCount = 640;
        public const int ObservationCount = GlobalFeatureCount + SpatialFeatureCount + MaxCandidateSlots * CandidateFeatureCount;
        public const string SpecResourceId = "MoyvaBotContract";
        public static BotContractSpec Spec { get; } = LoadSpec();
        public static string Hash { get; } = ComputeHash(Spec);

        private static BotContractSpec LoadSpec()
        {
            var asset = Resources.Load<TextAsset>(SpecResourceId);
            var spec = asset != null ? JsonUtility.FromJson<BotContractSpec>(asset.text) : null;
            if (spec == null)
                throw new InvalidOperationException("AI contract preset is missing or invalid: Resources/" + SpecResourceId + ".json");
            spec.Validate();
            if (spec.contractVersion != ContractVersion || spec.observationSchemaVersion != ObservationSchemaVersion
                || spec.candidateSchemaVersion != CandidateSchemaVersion || spec.actionSchemaVersion != ActionSchemaVersion
                || spec.maxCandidateSlots != MaxCandidateSlots || spec.globalFeatureCount != GlobalFeatureCount
                || spec.candidateFeatureCount != CandidateFeatureCount || spec.spatialSize != SpatialSize
                || spec.spatialChannels != SpatialChannels || spec.ObservationCount != ObservationCount)
                throw new InvalidOperationException(
                    SpecResourceId + ".json drifted from BotDecisionContract constants; update both together.");
            return spec;
        }

        private static string ComputeHash(BotContractSpec spec)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(spec.Signature())))
                .Replace("-", "").ToLowerInvariant();
        }
    }
    public static class BotObservationSchema
    {
        public const int Active = 0, Round = 1, Phase = 2, OwnUnits = 3, VisibleOtherUnits = 4;
        public const int UnitsAvailable = 5, EconomyAvailable = 6, SpatialAvailable = 7, VisibilityAvailable = 8;
        public const int ScenarioGoal = 9, ScenarioStep = 10, ScenarioProgress = 11;
        public const int OwnSettlements = 12, VisibleEnemySettlements = 13;
        /// <summary>Last-known hostile units still remembered through fog.</summary>
        public const int RememberedEnemyUnits = 14;
        /// <summary>Last-known hostile buildings still remembered through fog.</summary>
        public const int RememberedEnemyBuildings = 15;
        public const int Capabilities = 16;
        public const int OwnResourcesTotal = 24, PoolResourcesTotal = 25, ResourceKinds = 26;
        public const int ResourceFood = 27, ResourceWood = 28, ResourceStone = 29, ResourceIron = 30, ResourceGold = 31;
        public const int ProductionEstimate = 32, PopulationAvailable = 33;
        // Semantic scenario goal vector. Replaces reliance on the opaque
        // scenario id hash at slot 9 as the primary goal signal.
        public const int GoalCastle = 34, GoalProduction = 35, GoalEconomy = 36;
        public const int GoalRecruit = 37, GoalMove = 38, GoalScout = 39;
        public const int GoalCombat = 40, GoalCapture = 41, GoalWin = 42;
        // Aggregate tactical metrics for own forces. Distinct from the goal
        // vector: these carry continuous values, goals carry 0/1 presence flags.
        public const int TacticalVision = 43, TacticalAttack = 44, TacticalHeight = 45;
        public const int SpatialOffset = 96, CandidateOffset = 736;
        public const int Size = BotDecisionContract.ObservationCount;
    }
}
