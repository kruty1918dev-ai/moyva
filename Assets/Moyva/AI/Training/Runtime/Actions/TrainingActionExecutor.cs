namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingActionExecutor : ITrainingActionExecutor
    {
        private readonly ITrainingSimulation _simulation;
        private readonly ITrainingActionProvider _actions;

        public TrainingActionExecutor(ITrainingSimulation simulation, ITrainingActionProvider actions)
        {
            _simulation = simulation;
            _actions = actions;
        }

        public bool TryExecute(int actionIndex, out string reason)
        {
            if (!_actions.IsLegal(actionIndex))
            {
                reason = "Action is masked or no longer legal.";
                return false;
            }
            var action = _actions.Decode(actionIndex);
            if (action.ActionType == TrainingActionType.NoOp)
            {
                reason = null;
                return true;
            }
            if (action.ActionType == TrainingActionType.EndTurn)
                return _simulation.Turns.TryEndTurn(_simulation.PlayerId, out reason);
            reason = "Action adapter is not implemented.";
            return false;
        }
    }
}
