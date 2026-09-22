using System.Collections.Generic;
using Kruty1918.Telemetry.Contracts;
using Kruty1918.Telemetry.Core;
using Kruty1918.Telemetry.Validation;

namespace Kruty1918.Telemetry.Serialization
{
    /// <summary>
    /// <see cref="ITelemetryEventWriter"/> that serializes event fields to JSON while
    /// validating each field against the contract's structural rules inline.
    /// Unknown fields are rejected; invalid values are recorded as violations and
    /// skipped (written as omitted), never throwing into gameplay code.
    /// </summary>
    public sealed class ContractValidatingWriter : ITelemetryEventWriter
    {
        private readonly TelemetryJsonWriter _json;
        private readonly EventContract _contract;
        private readonly List<ValidationViolation> _violations;
        private readonly HashSet<string> _written;
        private readonly int _maxObjectFields;

        public ContractValidatingWriter(TelemetryJsonWriter json, EventContract contract,
            List<ValidationViolation> violations, int maxObjectFields = 64)
        {
            _json = json;
            _contract = contract;
            _violations = violations;
            _written = new HashSet<string>(System.StringComparer.Ordinal);
            _maxObjectFields = maxObjectFields;
        }

        public IReadOnlyList<ValidationViolation> Violations => _violations;

        public bool MissingRequired(out string name)
        {
            foreach (var f in _contract.Fields)
                if (f.Required && !_written.Contains(f.Name)) { name = f.Name; return true; }
            name = null;
            return false;
        }

        private bool Begin(string name, ContractFieldType t, object value)
        {
            var f = _contract?.Find(name);
            if (f == null)
            {
                // Nested scopes have no per-field contract: accept with default caps.
                if (_contract == null)
                {
                    if (_written.Count >= _maxObjectFields) { Violate(name, "maxfields"); return false; }
                    if (value is string s && s.Length > 256) { Violate(name, "maxlen"); return false; }
                    if (value is double dd && (double.IsNaN(dd) || double.IsInfinity(dd))) { Violate(name, "non-finite"); return false; }
                    _written.Add(name);
                    return true;
                }
                Violate(name, "unknown-field");
                return false;
            }
            if (f.Type != t) { Violate(name, "type:" + f.Type); return false; }
            if (!f.ValidateScalar(value, out string err)) { Violate(name, err); return false; }
            _written.Add(name);
            return true;
        }

        private void Violate(string field, string code)
            => _violations.Add(new ValidationViolation(ValidationViolationKind.Structural, field, code));

        public void Field(string name, int value) { if (Begin(name, ContractFieldType.Int, value)) _json.Field(name, value); }
        public void Field(string name, long value) { if (Begin(name, ContractFieldType.Long, value)) _json.Field(name, value); }
        public void Field(string name, bool value) { if (Begin(name, ContractFieldType.Bool, value)) _json.Field(name, value); }
        public void Field(string name, float value) { if (Begin(name, ContractFieldType.Float, (double)value)) _json.Field(name, value); }
        public void Field(string name, double value) { if (Begin(name, ContractFieldType.Double, value)) _json.Field(name, value); }

        public void Field(string name, string value)
        {
            if (!Begin(name, ContractFieldType.String, value)) return;
            _json.Field(name, value);
        }

        public void Field(string name, int[] values)
        {
            if (!BeginArray(name, ContractFieldType.IntArray, values?.Length ?? 0)) return;
            _json.Key(name).BeginArray();
            foreach (var v in values) _json.Value(v);
            _json.EndArray();
        }

        public void Field(string name, long[] values)
        {
            if (!BeginArray(name, ContractFieldType.LongArray, values?.Length ?? 0)) return;
            _json.Key(name).BeginArray();
            foreach (var v in values) _json.Value(v);
            _json.EndArray();
        }

        public void Field(string name, float[] values)
        {
            if (!BeginArray(name, ContractFieldType.FloatArray, values?.Length ?? 0)) return;
            _json.Key(name).BeginArray();
            foreach (var v in values) _json.Value(v);
            _json.EndArray();
        }

        public void Field(string name, string[] values)
        {
            if (!BeginArray(name, ContractFieldType.StringArray, values?.Length ?? 0)) return;
            _json.Key(name).BeginArray();
            foreach (var v in values) _json.Value(v);
            _json.EndArray();
        }

        public void FieldObject(string name, System.Action<ITelemetryEventWriter> write)
        {
            if (!BeginComposite(name, ContractFieldType.Object, write == null ? 0 : 1)) return;
            _json.Key(name).BeginObject();
            write?.Invoke(new ContractValidatingWriter(_json, null, _violations, _maxObjectFields));
            _json.EndObject();
        }

        public void FieldObjectArray(string name, System.Action<ITelemetryEventWriter>[] write)
        {
            if (!BeginComposite(name, ContractFieldType.ObjectArray, write?.Length ?? 0)) return;
            _json.Key(name).BeginArray();
            if (write != null)
                for (int i = 0; i < write.Length; i++)
                {
                    _json.BeginObject();
                    write[i]?.Invoke(new ContractValidatingWriter(_json, null, _violations, _maxObjectFields));
                    _json.EndObject();
                }
            _json.EndArray();
        }

        private bool BeginComposite(string name, ContractFieldType t, int count)
        {
            if (_contract == null)
            {
                if (_written.Count >= _maxObjectFields) { Violate(name, "maxfields"); return false; }
                _written.Add(name);
                return true;
            }
            var f = _contract.Find(name);
            if (f == null) { Violate(name, "unknown-field"); return false; }
            if (f.Type != t) { Violate(name, "type:" + f.Type); return false; }
            if (f.MaxLength > 0 && count > f.MaxLength) { Violate(name, "maxlen"); return false; }
            _written.Add(name);
            return true;
        }

        private bool BeginArray(string name, ContractFieldType t, int len)
        {
            if (_contract == null)
            {
                if (_written.Count >= _maxObjectFields) { Violate(name, "maxfields"); return false; }
                if (len > 1024) { Violate(name, "maxlen"); return false; }
                _written.Add(name);
                return true;
            }
            var f = _contract.Find(name);
            if (f == null) { Violate(name, "unknown-field"); return false; }
            if (f.Type != t) { Violate(name, "type:" + f.Type); return false; }
            if (f.MaxLength > 0 && len > f.MaxLength) { Violate(name, "maxlen"); return false; }
            _written.Add(name);
            return true;
        }
    }
}
