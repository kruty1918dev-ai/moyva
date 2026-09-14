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
        public TrainingActionMaskProvider(ITrainingSimulation simulation) : this(new TrainingBotBridge(simulation))
        {
            Bridge.Reset((int)TrainingCurriculumStage.FullGame);
            Bridge.Tick(0);
        }
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
        public bool IsLegal(int index)
        {
            EnsureFrame();
            return Bridge.Frame?.Candidates.IsLegal(index) ?? false;
        }
        public int FirstLegalAction()
        {
            EnsureFrame();
            var candidates = Bridge.Frame?.Candidates;
            if (candidates == null) return SafeActionIndex;
            for (int i = 0; i < candidates.Count; i++)
                if (candidates.IsLegal(i)) return i;
            return SafeActionIndex;
        }
        public void WriteMask(IDiscreteActionMask mask) => BotMlFrameWriter.Mask(Bridge.Frame, mask);
        private void EnsureFrame()
        {
            if (Bridge.Frame == null) Bridge.Tick(0);
        }
    }
}
