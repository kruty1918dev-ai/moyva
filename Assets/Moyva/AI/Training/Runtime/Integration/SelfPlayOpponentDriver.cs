using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.AI.Bot;
using Unity.InferenceEngine;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Training
{
    /// <summary>
    /// Self-play opponent: a verified frozen ONNX checkpoint driving the enemy
    /// faction through the same observation/action contract as the learner.
    /// The model is loaded from a runtime path (training artifacts never become
    /// imported assets), and any decode that lands on an illegal slot degrades
    /// to the first legal candidate so the episode can never stall on a bad
    /// checkpoint.
    /// </summary>
    internal sealed class SelfPlayOpponentDriver : IBotPolicyDriver, IDisposable
    {
        private readonly Worker _worker;
        private readonly string _observationInput;
        private readonly string _maskInput;
        private readonly string _actionOutput;
        private readonly int _observationSize;
        private readonly int _slotCount;

        public BotPolicyMode Mode => BotPolicyMode.MLAgentsInference;
        public string ModelPath { get; }

        private SelfPlayOpponentDriver(Model model, string modelPath,
            string observationInput, string maskInput, string actionOutput,
            int observationSize, int slotCount)
        {
            _worker = new Worker(model, BackendType.CPU);
            ModelPath = modelPath;
            _observationInput = observationInput;
            _maskInput = maskInput;
            _actionOutput = actionOutput;
            _observationSize = observationSize;
            _slotCount = slotCount;
        }

        public static SelfPlayOpponentDriver TryLoad(string modelPath, out string reason)
        {
            reason = null;
            if (string.IsNullOrWhiteSpace(modelPath)) { reason = "no opponent model path"; return null; }
            string fullPath;
            try { fullPath = Path.GetFullPath(modelPath); }
            catch (Exception) { reason = "invalid opponent model path"; return null; }
            if (!File.Exists(fullPath)) { reason = "opponent model not found: " + fullPath; return null; }
            if (fullPath.EndsWith(".onnx", StringComparison.OrdinalIgnoreCase))
            { reason = "opponent model must be .sentis — ONNX conversion is editor-only: " + fullPath; return null; }

            Model model;
            try { model = ModelLoader.Load(fullPath); }
            catch (Exception e) { reason = "opponent model failed to load: " + e.Message; return null; }

            string observationInput = null, maskInput = null;
            foreach (var input in model.inputs)
            {
                string name = input.name ?? string.Empty;
                if (name.IndexOf("obs", StringComparison.OrdinalIgnoreCase) >= 0) observationInput = name;
                else if (name.IndexOf("mask", StringComparison.OrdinalIgnoreCase) >= 0) maskInput = name;
            }
            if (observationInput == null && model.inputs.Count > 0) observationInput = model.inputs[0].name;
            if (maskInput == null)
                foreach (var input in model.inputs)
                    if (input.name != observationInput) { maskInput = input.name; break; }
            if (observationInput == null) { reason = "opponent model has no observation input"; return null; }

            string actionOutput = null;
            foreach (var output in model.outputs)
            {
                string name = output.name ?? string.Empty;
                if (name.IndexOf("deterministic", StringComparison.OrdinalIgnoreCase) >= 0) { actionOutput = name; break; }
            }
            if (actionOutput == null)
                foreach (var output in model.outputs)
                {
                    string name = output.name ?? string.Empty;
                    if (name.IndexOf("discrete_actions", StringComparison.OrdinalIgnoreCase) >= 0) { actionOutput = name; break; }
                }
            if (actionOutput == null && model.outputs.Count > 0) actionOutput = model.outputs[0].name;
            if (actionOutput == null) { reason = "opponent model has no action output"; return null; }

            return new SelfPlayOpponentDriver(model, fullPath, observationInput, maskInput,
                actionOutput, BotObservationSchema.Size, BotDecisionContract.MaxCandidateSlots);
        }

        public Task<BotPolicyDecision> Decide(BotDecisionFrame frame, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            int slot = Decode(frame);
            if (!frame.Candidates.IsLegal(slot))
            {
                for (int i = 0; i < _slotCount && i < frame.Candidates.Count; i++)
                    if (frame.Candidates.IsLegal(i)) { slot = i; break; }
            }
            return Task.FromResult(new BotPolicyDecision(slot, frame.Sequence, Mode));
        }

        private int Decode(BotDecisionFrame frame)
        {
            var observations = new float[_observationSize];
            for (int i = 0; i < observations.Length && i < frame.Observations.Count; i++)
                observations[i] = frame.Observations[i];
            var mask = new float[_slotCount];
            if (_maskInput != null)
                for (int i = 0; i < mask.Length; i++)
                    mask[i] = frame.Candidates.IsLegal(i) ? 1f : 0f;

            using var observationTensor = new Tensor<float>(new TensorShape(1, _observationSize), observations);
            _worker.SetInput(_observationInput, observationTensor);
            if (_maskInput != null)
            {
                using var maskTensor = new Tensor<float>(new TensorShape(1, _slotCount), mask);
                _worker.SetInput(_maskInput, maskTensor);
            }
            _worker.Schedule();

            var output = _worker.PeekOutput(_actionOutput) as Tensor<int>;
            if (output == null) return -1;
            using var result = output.ReadbackAndClone();
            return result[0];
        }

        public void Dispose() => _worker?.Dispose();
    }
}
