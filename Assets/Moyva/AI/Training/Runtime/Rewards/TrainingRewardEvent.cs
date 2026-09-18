namespace Kruty1918.Moyva.AI.Training
{
    public enum TrainingRewardEventType
    {
        EpisodeStarted, EpisodeWon, EpisodeLost, EpisodeDraw, EpisodeTimeout,
        Decision, TurnCompleted, ValidAction, InvalidAction, UnitCreated,
        EnemyUnitDestroyed, OwnUnitLost, BuildingCreated, OwnBuildingLost,
        ObjectiveCaptured, ObjectiveLost, ResourceMilestone, StagnantDecision,
        IsolatedSettlement, ResourcePotential
    }

    public readonly struct TrainingRewardEvent
    {
        public readonly long EpisodeId;
        public readonly string EventId;
        public readonly TrainingRewardEventType Type;
        public readonly string SubjectId;
        public readonly string ActorOwnerId;
        public readonly string TargetOwnerId;
        public readonly bool Validated;
        public readonly bool Meaningful;

        public TrainingRewardEvent(long episodeId, string eventId, TrainingRewardEventType type,
            string subjectId = "", string actorOwnerId = "", string targetOwnerId = "",
            bool validated = false, bool meaningful = false)
        {
            EpisodeId = episodeId;
            EventId = eventId;
            Type = type;
            SubjectId = subjectId;
            ActorOwnerId = actorOwnerId;
            TargetOwnerId = targetOwnerId;
            Validated = validated;
            Meaningful = meaningful;
        }
    }
}
