using System.Collections.Generic;

namespace Kruty1918.Moyva.AI.Training
{
    public enum TrainingMechanicStatus { Ready, Blocked, NotApplicableByGameDesign }

    public sealed class TrainingReadinessReport
    {
        private readonly List<string> _blockers = new List<string>();
        private readonly Dictionary<string, TrainingMechanicStatus> _mechanics = new Dictionary<string, TrainingMechanicStatus>();
        public IReadOnlyDictionary<string, TrainingMechanicStatus> Mechanics => new System.Collections.ObjectModel.ReadOnlyDictionary<string, TrainingMechanicStatus>(_mechanics);
        private readonly List<string> _warnings = new List<string>();
        public IReadOnlyList<string> Blockers => _blockers.AsReadOnly();
        public IReadOnlyList<string> Warnings => _warnings.AsReadOnly();
        public bool IsReady => _blockers.Count == 0;
        public string Verdict => IsReady ? "READY_FOR_REAL_TRAINING" : "NOT_READY_FOR_REAL_TRAINING";
        public void Block(string reason) { if (!_blockers.Contains(reason)) _blockers.Add(reason); }
        public void SetMechanic(string name, TrainingMechanicStatus status, string reason = null)
        {
            _mechanics[name] = status;
            if (status == TrainingMechanicStatus.Blocked) Block(name + ": BLOCKED: " + reason);
        }
        private string DescribeMechanics()
        {
            var lines = new List<string>();
            foreach (var pair in _mechanics)
                lines.Add(pair.Key + ": " + (pair.Value == TrainingMechanicStatus.Ready ? "READY"
                    : pair.Value == TrainingMechanicStatus.Blocked ? "BLOCKED" : "NOT_APPLICABLE_BY_GAME_DESIGN"));
            return string.Join("\n", lines);
        }
        public void Warn(string reason) { if (!_warnings.Contains(reason)) _warnings.Add(reason); }
        public override string ToString() => Verdict + "\n" + DescribeMechanics() + "\n" + string.Join("\n", _blockers) + "\n" + string.Join("\n", _warnings);
    }
}
