using System.Collections.Generic;

namespace Kruty1918.Telemetry.Validation
{
    public enum QualityDecision
    {
        /// <summary>Data passes all gates; eligible for validated/normalized layers.</summary>
        Accept = 0,
        /// <summary>Accepted with warnings; flagged but stored.</summary>
        AcceptWithWarnings = 1,
        /// <summary>Failed hard gates; stored in quarantine, never training-ready.</summary>
        Quarantine = 2,
        /// <summary>Unparseable/corrupt; rejected outright.</summary>
        Reject = 3,
    }

    /// <summary>
    /// Per-batch (or per-event) quality verdict. Attached to batch metadata and
    /// returned by validators; the backend mirrors it in the dataset manifest.
    /// </summary>
    public sealed class DataQualityReport
    {
        public const int ValidatorVersion = 1;

        public int TotalEvents;
        public int ValidEvents;
        public int InvalidEvents;
        public int DuplicateEvents;
        public int MissingRequiredFields;
        public readonly List<ValidationViolation> Violations = new List<ValidationViolation>();
        /// <summary>Anomaly indicators, e.g. "constant-field:score", "rate-exceeded".</summary>
        public readonly List<string> Anomalies = new List<string>();
        public QualityDecision Decision = QualityDecision.Accept;

        /// <summary>Completeness 0..1 = structurally valid events / total.</summary>
        public float Completeness => TotalEvents == 0 ? 1f : ValidEvents / (float)TotalEvents;

        /// <summary>Quality score 0..1 combining completeness and violation density.</summary>
        public float Score
        {
            get
            {
                float s = Completeness;
                if (TotalEvents > 0)
                    s -= System.Math.Min(0.5f, Violations.Count / (float)TotalEvents * 0.5f);
                s -= System.Math.Min(0.2f, Anomalies.Count * 0.05f);
                return s < 0 ? 0 : s > 1 ? 1 : s;
            }
        }

        public void Add(ValidationViolation v)
        {
            Violations.Add(v);
            if (v.Kind == ValidationViolationKind.Integrity || Decision == QualityDecision.Reject) return;
            if (Violations.Count >= 1 && Decision == QualityDecision.Accept)
                Decision = QualityDecision.AcceptWithWarnings;
        }

        /// <summary>Hard-fail the batch.</summary>
        public void Fail(ValidationViolation v)
        {
            Violations.Add(v);
            Decision = v.Kind == ValidationViolationKind.Integrity
                ? QualityDecision.Reject
                : QualityDecision.Quarantine;
        }
    }
}
