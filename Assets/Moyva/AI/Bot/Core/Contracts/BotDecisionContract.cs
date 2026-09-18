using System;
using System.Security.Cryptography;
using System.Text;

namespace Kruty1918.Moyva.AI.Bot
{
    public enum BotIntentType { None = 0, EndTurn = 1, Move = 2, Attack = 3, Capture = 4, Recruit = 5, Build = 6, Explore = 7, Defend = 8, Economy = 9, Reposition = 10, Wait = 11 }
    public enum BotCapabilityId { Turn = 0, Movement = 1, Combat = 2, Recruitment = 3, Construction = 4, Capture = 5, Economy = 6, Exploration = 7 }
    public static class BotDecisionContract
    {
        public const int ContractVersion = 2, ObservationSchemaVersion = 2, CandidateSchemaVersion = 2, ActionSchemaVersion = 1;
        public const int MaxCandidateSlots = 128, GlobalFeatureCount = 96, CandidateFeatureCount = 32;
        public const int SpatialSize = 8, SpatialChannels = 10, SpatialFeatureCount = 640;
        public const int ObservationCount = GlobalFeatureCount + SpatialFeatureCount + MaxCandidateSlots * CandidateFeatureCount;
        public static string Hash { get; } = ComputeHash();
        private static string ComputeHash()
        {
                using var sha = SHA256.Create();
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(
                    "MoyvaBot:v2:o2:c2:a1:slots128:global96:spatial8x8x10:candidate32:"
                    + "global=0active,1round,2phase,3ownUnits,4visibleOtherUnits,5unitsAvailable,6economyAvailable,7spatialAvailable,8visibilityAvailable,"
                    + "9scenarioGoal,10scenarioStep,11scenarioProgress,12ownSettlements,13visibleEnemySettlements,16capabilities8,"
                    + "24ownResourcesTotal,25poolResourcesTotal,26resourceKinds,27food,28wood,29stone,30iron,31gold,32productionEstimate,33populationAvailable,"
                    + "34goalCastle,35goalProduction,36goalEconomy,37goalRecruit,38goalMove,39goalScout,40goalCombat,41goalCapture,42goalWin:"
                    + "candidate=present,intentOneHot12,distance,cost,ownHp,targetHp,visible,path,complete,buildingType,unitType,purpose,x,y,duration,reserved:"
                    + "intents=None,EndTurn,Move,Attack,Capture,Recruit,Build,Explore,Defend,Economy,Reposition,Wait")))
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
        public const int SpatialOffset = 96, CandidateOffset = 736;
        public const int Size = BotDecisionContract.ObservationCount;
    }
}
