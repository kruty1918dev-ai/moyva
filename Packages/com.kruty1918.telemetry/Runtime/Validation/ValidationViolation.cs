namespace Kruty1918.Telemetry.Validation
{
    public enum ValidationViolationKind
    {
        Structural = 0,
        Semantic = 1,
        Sequence = 2,
        Statistical = 3,
        Integrity = 4,
    }

    /// <summary>One machine-readable violation; <c>Code</c> is stable for aggregation.</summary>
    public readonly struct ValidationViolation
    {
        public readonly ValidationViolationKind Kind;
        /// <summary>Field name or event scope, e.g. "turn", "batch".</summary>
        public readonly string Scope;
        /// <summary>Stable code, e.g. "required-null", "non-finite", "end-before-start".</summary>
        public readonly string Code;
        /// <summary>Optional event sequence the violation refers to.</summary>
        public readonly long EventSeq;

        public ValidationViolation(ValidationViolationKind kind, string scope, string code, long eventSeq = -1)
        {
            Kind = kind; Scope = scope; Code = code; EventSeq = eventSeq;
        }

        public override string ToString() => $"{Kind}:{Scope}:{Code}";
    }
}
