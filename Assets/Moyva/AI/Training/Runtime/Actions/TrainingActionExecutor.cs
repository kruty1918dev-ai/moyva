namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingActionExecutor : ITrainingActionExecutor
    {
        private readonly ITrainingSimulation _simulation;
        private readonly TrainingBotBridge _bridge;
        public TrainingActionExecutor(ITrainingSimulation simulation, ITrainingActionProvider actions)
        { _simulation = simulation; _bridge = ((TrainingActionMaskProvider)actions).Bridge; }
        public bool TryExecute(int actionIndex, out string reason)
        {
            if (_simulation?.Turns == null && actionIndex == TrainingActionMaskProvider.SafeActionIndex)
            {
                reason = null;
                return true;
            }
            if (!_bridge.CanRequestDecision) _bridge.Tick(0);
            var candidate = _bridge.Frame?.Candidates[actionIndex];
            if (_simulation?.Turns != null && candidate != null
                && actionIndex >= 0 && actionIndex <= (int)TrainingActionType.Capture
                && TrainingActionMaskProvider.FromCandidate(candidate).ActionType != (TrainingActionType)actionIndex)
            {
                reason = "Requested legacy action is not legal in the current candidate frame.";
                return false;
            }
            bool submitted = _bridge.Submit(actionIndex);
            if (!submitted && _simulation?.Turns == null && candidate?.Intent == Kruty1918.Moyva.AI.Bot.BotIntentType.Wait)
            {
                reason = null;
                return true;
            }
            reason = submitted ? null : "No shared decision is currently pending.";
            return submitted;
        }
    }
}
