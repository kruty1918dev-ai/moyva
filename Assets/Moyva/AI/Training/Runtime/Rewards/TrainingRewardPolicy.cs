namespace Kruty1918.Moyva.AI.Training
{
    public interface ITrainingRewardPolicy
    {
        float Evaluate(TrainingRewardEvent rewardEvent, string playerId);
        float Terminal(TrainingEpisodeResult result);
    }

    public sealed class TrainingRewardPolicy : ITrainingRewardPolicy
    {
        private readonly TrainingRewardConfig _config;
        public TrainingRewardPolicy(TrainingRewardConfig config) { config.Validate(); _config = config; }

        public float Evaluate(TrainingRewardEvent e, string playerId)
        {
            if (e.Type == TrainingRewardEventType.InvalidAction) return _config.invalidAction;
            if (!e.Validated) return 0;
            switch (e.Type)
            {
                case TrainingRewardEventType.Decision:
                    return _config.penalizePerDecision ? _config.perDecision : 0;
                case TrainingRewardEventType.TurnCompleted:
                    return _config.penalizePerDecision ? 0 : _config.perTurn;
                case TrainingRewardEventType.StagnantDecision:
                    return _config.stagnantDecision;
                case TrainingRewardEventType.IsolatedSettlement:
                    return _config.isolatedSettlement;
                case TrainingRewardEventType.ResourcePotential:
                    return _config.resourcePotential;
            }
            if (!e.Meaningful || string.IsNullOrEmpty(e.SubjectId) || string.IsNullOrEmpty(playerId))
                return 0;
            switch (e.Type)
            {
                case TrainingRewardEventType.ObjectiveCaptured:
                    return e.ActorOwnerId == playerId && e.TargetOwnerId != playerId ? _config.objectiveCaptured : 0;
                case TrainingRewardEventType.ObjectiveLost:
                    return e.TargetOwnerId == playerId ? _config.objectiveLost : 0;
                case TrainingRewardEventType.EnemyUnitDestroyed:
                    return e.ActorOwnerId == playerId && !string.IsNullOrEmpty(e.TargetOwnerId)
                        && e.TargetOwnerId != playerId ? _config.enemyUnitDestroyed : 0;
                case TrainingRewardEventType.OwnUnitLost:
                    return e.TargetOwnerId == playerId ? _config.ownUnitLost : 0;
                case TrainingRewardEventType.ResourceMilestone:
                    return e.ActorOwnerId == playerId ? _config.resourceMilestone : 0;
                case TrainingRewardEventType.BuildingCreated:
                case TrainingRewardEventType.UnitCreated:
                    return e.ActorOwnerId == playerId ? _config.buildOrRecruit : 0;
                default: return 0;
            }
        }

        public float Terminal(TrainingEpisodeResult result)
        {
            switch (result)
            {
                case TrainingEpisodeResult.Victory: return _config.victory;
                case TrainingEpisodeResult.ScenarioSuccess: return _config.victory;
                case TrainingEpisodeResult.Defeat: return _config.defeat;
                case TrainingEpisodeResult.Draw: return _config.draw;
                case TrainingEpisodeResult.Timeout: return _config.timeout;
                default: return 0;
            }
        }
    }
}
