using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using Unity.InferenceEngine;

namespace Kruty1918.Moyva.AI.Bot
{
    public static class BotMlFrameWriter
    {
        public static void Observe(BotDecisionFrame frame, VectorSensor sensor)
        { foreach (float value in frame.Observations) sensor.AddObservation(value); }
        public static void Mask(BotDecisionFrame frame, IDiscreteActionMask mask)
        {
            for (int i = 0; i < BotDecisionContract.MaxCandidateSlots; i++)
                mask.SetActionEnabled(0, i, frame.Candidates.IsLegal(i));
        }
        public static void Configure(BehaviorParameters behavior, BehaviorType type, ModelAsset model = null)
        {
            behavior.BehaviorName = "MoyvaStrategy";
            behavior.BehaviorType = type;
            behavior.BrainParameters.VectorObservationSize = BotObservationSchema.Size;
            behavior.BrainParameters.NumStackedVectorObservations = 1;
            behavior.BrainParameters.ActionSpec = ActionSpec.MakeDiscrete(BotDecisionContract.MaxCandidateSlots);
            behavior.Model = model;
        }
    }

    public sealed class MoyvaBotPolicyAgent : Agent, IBotPolicyDriver
    {
        private TaskCompletionSource<BotPolicyDecision> _pending;
        private BotDecisionFrame _frame;
        private CancellationTokenRegistration _registration;
        private bool _inFlight;
        public BotPolicyMode Mode { get; private set; } = BotPolicyMode.MLAgentsInference;
        public void Configure(BotPolicyMode mode) { Mode = mode; MaxStep = 0; }
        public Task<BotPolicyDecision> Decide(BotDecisionFrame frame, CancellationToken token)
        {
            if (_inFlight) throw new InvalidOperationException("Previous ML callback has not drained.");
            _registration.Dispose();
            _frame = frame;
            _inFlight = true;
            _pending = new TaskCompletionSource<BotPolicyDecision>();
            var pending = _pending;
            _registration = token.Register(() => pending.TrySetCanceled());
            RequestDecision();
            return _pending.Task;
        }
        public override void CollectObservations(VectorSensor sensor) => BotMlFrameWriter.Observe(_frame, sensor);
        public override void WriteDiscreteActionMask(IDiscreteActionMask mask) => BotMlFrameWriter.Mask(_frame, mask);
        public override void OnActionReceived(ActionBuffers actions)
        {
            _inFlight = false;
            _pending?.TrySetResult(new BotPolicyDecision(actions.DiscreteActions[0], _frame.Sequence, Mode));
        }
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var action = actionsOut.DiscreteActions;
            action[0] = new HeuristicBotPolicyDriver().Decide(_frame, CancellationToken.None).Result.Slot;
        }
        protected override void OnDisable()
        {
            _registration.Dispose();
            _pending?.TrySetCanceled();
            base.OnDisable();
        }
    }
}
