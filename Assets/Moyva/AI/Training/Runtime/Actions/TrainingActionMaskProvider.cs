using System;
using Kruty1918.Moyva.AI.Bot;
using Unity.MLAgents.Actuators;
namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingActionMaskProvider : ITrainingActionProvider
    {
        public const int BranchSize = BotDecisionContract.MaxCandidateSlots;
        public const int SafeActionIndex = 0;
        public TrainingBotBridge Bridge { get; }
        public TrainingActionMaskProvider(ITrainingSimulation simulation) : this(new TrainingBotBridge(simulation)) { }
        public TrainingActionMaskProvider(TrainingBotBridge bridge) { Bridge = bridge; }
        public int ActionCount => BranchSize;
        public TrainingAction Decode(int index)
        {
            var candidate = Bridge.Frame?.Candidates[index];
            if (candidate == null) throw new ArgumentOutOfRangeException(nameof(index));
            return FromCandidate(candidate);
        }
        public static TrainingAction FromCandidate(BotCandidateAction candidate)
        {
            var type = candidate.Intent switch
            {
                BotIntentType.EndTurn => TrainingActionType.EndTurn,
                BotIntentType.Move => TrainingActionType.MoveUnit,
                BotIntentType.Attack => TrainingActionType.Attack,
                BotIntentType.Recruit => TrainingActionType.Recruit,
                BotIntentType.Build => TrainingActionType.Build,
                BotIntentType.Capture => TrainingActionType.Capture,
                _ => TrainingActionType.NoOp
            };
            return new TrainingAction(type, candidate.ActorKey, candidate.TargetKey);
        }
        public bool IsLegal(int index) => Bridge.Frame?.Candidates.IsLegal(index) ?? false;
        public int FirstLegalAction() => 0;
        public void WriteMask(IDiscreteActionMask mask) => BotMlFrameWriter.Mask(Bridge.Frame, mask);
    }
}
