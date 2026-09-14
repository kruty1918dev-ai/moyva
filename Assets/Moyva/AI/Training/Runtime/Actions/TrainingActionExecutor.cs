namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingActionExecutor : ITrainingActionExecutor
    {
        private readonly TrainingBotBridge _bridge;
        public TrainingActionExecutor(ITrainingSimulation simulation, ITrainingActionProvider actions)
        { _bridge = ((TrainingActionMaskProvider)actions).Bridge; }
        public bool TryExecute(int actionIndex, out string reason)
        {
            if (!_bridge.CanRequestDecision) _bridge.Tick(0);
            bool submitted = _bridge.Submit(actionIndex);
            reason = submitted ? null : "No shared decision is currently pending.";
            return submitted;
        }
    }
}
