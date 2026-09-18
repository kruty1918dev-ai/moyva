namespace Kruty1918.Moyva.AI.Training
{
    public enum TrainingActionType { EndTurn, MoveUnit, Attack, Recruit, Build, Capture, NoOp }

    public readonly struct TrainingAction
    {
        public readonly TrainingActionType ActionType;
        public readonly string ActorId;
        public readonly string TargetId;
        public readonly int TargetTileIndex;
        public readonly int SecondaryParameter;

        public TrainingAction(TrainingActionType type, string actorId = "", string targetId = "",
            int targetTileIndex = -1, int secondaryParameter = 0)
        {
            ActionType = type;
            ActorId = actorId;
            TargetId = targetId;
            TargetTileIndex = targetTileIndex;
            SecondaryParameter = secondaryParameter;
        }
    }

    public interface ITrainingActionProvider
    {
        int ActionCount { get; }
        TrainingAction Decode(int index);
        bool IsLegal(int index);
    }

    public interface ITrainingActionExecutor
    {
        bool TryExecute(int actionIndex, out string reason);
    }
}
