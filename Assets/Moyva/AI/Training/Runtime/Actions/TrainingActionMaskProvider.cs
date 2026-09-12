using System;
using Unity.MLAgents.Actuators;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingActionMaskProvider : ITrainingActionProvider
    {
        public const int BranchSize = 7;
        public const int SafeActionIndex = (int)TrainingActionType.NoOp;
        private readonly ITrainingSimulation _simulation;

        public TrainingActionMaskProvider(ITrainingSimulation simulation) { _simulation = simulation; }
        public int ActionCount => BranchSize;
        public TrainingAction Decode(int index)
        {
            if (index < 0 || index >= ActionCount) throw new ArgumentOutOfRangeException(nameof(index));
            return new TrainingAction((TrainingActionType)index, _simulation.PlayerId);
        }

        public bool IsLegal(int index)
        {
            if (index == SafeActionIndex) return true;
            return index == (int)TrainingActionType.EndTurn && _simulation.IsReady
                && _simulation.Turns != null
                && _simulation.Turns.CanOwnerAct(_simulation.PlayerId, out _)
                && _simulation.CanEndTurn();
        }

        public int FirstLegalAction()
        {
            for (int i = 0; i < ActionCount; i++)
                if (IsLegal(i)) return i;
            return SafeActionIndex;
        }

        public void WriteMask(IDiscreteActionMask mask)
        {
            for (int i = 0; i < ActionCount; i++)
                mask.SetActionEnabled(0, i, IsLegal(i));
        }
    }
}
