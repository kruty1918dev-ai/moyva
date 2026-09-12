using System;
using System.Security.Cryptography;
using System.Text;

namespace Kruty1918.Moyva.AI.Bot
{
    public enum BotIntentType { None = 0, EndTurn = 1, Move = 2, Attack = 3, Capture = 4, Recruit = 5, Build = 6, Explore = 7, Defend = 8, Economy = 9, Reposition = 10, Wait = 11 }
    public enum BotCapabilityId { Turn = 0, Movement = 1, Combat = 2, Recruitment = 3, Construction = 4, Capture = 5, Economy = 6, Exploration = 7 }
    public static class BotDecisionContract
    {
        public const int ContractVersion = 1, ObservationSchemaVersion = 1, CandidateSchemaVersion = 1, ActionSchemaVersion = 1;
        public const int MaxCandidateSlots = 128, GlobalFeatureCount = 64, CandidateFeatureCount = 24;
        public const int SpatialSize = 8, SpatialChannels = 8, SpatialFeatureCount = 512;
        public const int ObservationCount = GlobalFeatureCount + SpatialFeatureCount + MaxCandidateSlots * CandidateFeatureCount;
        public static string Hash { get; } = ComputeHash();
        private static string ComputeHash()
        {
                using var sha = SHA256.Create();
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(
                    "MoyvaBot:v1:o1:c1:a1:slots128:global64:spatial8x8x8:candidate24:"
                    + "global=0active,1round,2phase,3ownUnits,4visibleOtherUnits,5unitsAvailable,6economyAvailable,7spatialAvailable,8visibilityAvailable,16capabilities8:"
                    + "candidate=present,intentOneHot12,distance,cost,ownHp,targetHp,visible,path,complete,reserved4:"
                    + "intents=None,EndTurn,Move,Attack,Capture,Recruit,Build,Explore,Defend,Economy,Reposition,Wait")))
                    .Replace("-", "").ToLowerInvariant();
        }
    }
    public static class BotObservationSchema
    {
        public const int Active = 0, Round = 1, Phase = 2, OwnUnits = 3, VisibleOtherUnits = 4;
        public const int UnitsAvailable = 5, EconomyAvailable = 6, SpatialAvailable = 7, VisibilityAvailable = 8;
        public const int Capabilities = 16, SpatialOffset = 64, CandidateOffset = 576;
        public const int Size = BotDecisionContract.ObservationCount;
    }
}
