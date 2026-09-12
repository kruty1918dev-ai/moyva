using System.Collections.Generic;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingReadinessReport
    {
        private readonly List<string> _blockers = new List<string>();
        private readonly List<string> _warnings = new List<string>();
        public IReadOnlyList<string> Blockers => _blockers.AsReadOnly();
        public IReadOnlyList<string> Warnings => _warnings.AsReadOnly();
        public bool IsReady => _blockers.Count == 0;
        public string Verdict => IsReady ? "READY_FOR_REAL_TRAINING" : "NOT_READY_FOR_REAL_TRAINING";
        public void Block(string reason) { if (!_blockers.Contains(reason)) _blockers.Add(reason); }
        public void Warn(string reason) { if (!_warnings.Contains(reason)) _warnings.Add(reason); }
        public override string ToString() => Verdict + "\n" + string.Join("\n", _blockers) + "\n" + string.Join("\n", _warnings);
    }
}
