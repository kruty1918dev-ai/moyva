using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.AI.Bot;

namespace Kruty1918.Moyva.AI.Training
{
    /// <summary>
    /// Scripted opponent policies for scenario training. The opponent always
    /// obeys the real gameplay rules — these drivers only choose between the
    /// legal candidates the frame exposes.
    /// </summary>
    internal static class TrainingOpponentPolicy
    {
        public static IBotPolicyDriver Create(string archetype, string opponentModelPath = null)
        {
            switch ((archetype ?? "heuristic").Trim().ToLowerInvariant())
            {
                case "passive":
                case "none":
                case "static":
                    return new PassiveOpponentDriver();
                case "self-play":
                case "onnx":
                case "learned":
                    var driver = SelfPlayOpponentDriver.TryLoad(opponentModelPath, out string reason);
                    if (driver != null) return driver;
                    UnityEngine.Debug.LogWarning($"Self-play opponent unavailable ({reason}); falling back to heuristic.");
                    return new BoundedHeuristicOpponentDriver();
                default:
                    return new BoundedHeuristicOpponentDriver();
            }
        }
    }

    /// <summary>
    /// Static-target opponent: ends its turn immediately and never initiates
    /// attacks. Used by combat/capture scenarios where the learner is the
    /// aggressor and the enemy is a controlled objective.
    /// </summary>
    internal sealed class PassiveOpponentDriver : IBotPolicyDriver
    {
        public BotPolicyMode Mode => BotPolicyMode.Heuristic;

        public Task<BotPolicyDecision> Decide(BotDecisionFrame frame, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            for (int i = 0; i < frame.Candidates.Count; i++)
                if (frame.Candidates.IsLegal(i) && frame.Candidates[i].Intent == BotIntentType.EndTurn)
                    return Task.FromResult(new BotPolicyDecision(i, frame.Sequence, Mode));
            for (int i = 0; i < frame.Candidates.Count; i++)
                if (frame.Candidates.IsLegal(i))
                    return Task.FromResult(new BotPolicyDecision(i, frame.Sequence, Mode));
            return Task.FromResult(new BotPolicyDecision(0, frame.Sequence, Mode));
        }
    }

    /// <summary>
    /// Heuristic opponent with a per-turn action budget. Productive intents
    /// keep the shared priority order, but once the budget is spent the driver
    /// only takes EndTurn so an operational economy cannot keep it acting
    /// forever. Without the budget an endless recruit/move supply would hold
    /// the turn hostage — a training opponent must terminate deterministically.
    /// </summary>
    internal sealed class BoundedHeuristicOpponentDriver : IBotPolicyDriver
    {
        private const int MaxProductiveActionsPerTurn = 6;
        private long _turn = -1;
        private int _productiveActions;
        public BotPolicyMode Mode => BotPolicyMode.Heuristic;

        public Task<BotPolicyDecision> Decide(BotDecisionFrame frame, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (frame.Stamp.Turn != _turn)
            {
                _turn = frame.Stamp.Turn;
                _productiveActions = 0;
            }
            bool budgetLeft = _productiveActions < MaxProductiveActionsPerTurn;
            int fallback = -1;
            int endTurn = -1;
            if (budgetLeft)
            {
                foreach (var intent in new[] { BotIntentType.Attack, BotIntentType.Capture, BotIntentType.Recruit,
                    BotIntentType.Build, BotIntentType.Move, BotIntentType.EndTurn })
                    for (int i = 0; i < frame.Candidates.Count; i++)
                        if (frame.Candidates.IsLegal(i) && frame.Candidates[i].Intent == intent)
                        {
                            if (intent != BotIntentType.EndTurn) _productiveActions++;
                            return Task.FromResult(new BotPolicyDecision(i, frame.Sequence, Mode));
                        }
            }
            for (int i = 0; i < frame.Candidates.Count; i++)
            {
                if (!frame.Candidates.IsLegal(i)) continue;
                if (frame.Candidates[i].Intent == BotIntentType.EndTurn) { endTurn = i; break; }
                // EndTurn is only absent while a move is still animating — avoid
                // starting another one or the blocker never clears.
                if (fallback < 0
                    || frame.Candidates[fallback].Intent == BotIntentType.Move
                       && frame.Candidates[i].Intent != BotIntentType.Move)
                    fallback = i;
            }
            return Task.FromResult(new BotPolicyDecision(endTurn >= 0 ? endTurn : fallback >= 0 ? fallback : 0,
                frame.Sequence, Mode));
        }
    }
}
