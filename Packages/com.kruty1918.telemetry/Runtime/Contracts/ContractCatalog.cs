using System.Text;
using Kruty1918.Telemetry.Serialization;

namespace Kruty1918.Telemetry.Contracts
{
    /// <summary>
    /// Generates the machine-readable data catalog (JSON) and a human-readable
    /// Markdown rendering from a frozen <see cref="ContractRegistry"/>.
    /// </summary>
    public static class ContractCatalog
    {
        public static string ToJson(ContractRegistry registry)
        {
            var w = new TelemetryJsonWriter();
            w.BeginObject();
            w.Field("catalogVersion", 1);
            w.Key("contracts").BeginArray();
            foreach (var c in registry.All())
            {
                w.BeginObject();
                w.Field("contractId", c.ContractId);
                w.Field("version", c.Version);
                w.Field("eventType", c.EventType);
                w.Field("description", c.Description);
                w.Field("producer", c.Producer);
                w.Field("priority", c.Priority.ToString());
                w.Field("privacy", c.Privacy.ToString());
                w.Field("retention", c.Retention);
                w.Field("samplingRate", c.SamplingRate);
                w.Field("normalization", c.NormalizationId);
                w.Key("rules").BeginArray();
                foreach (var r in c.RuleIds) w.Value(r);
                w.EndArray();
                w.Key("fields").BeginArray();
                foreach (var f in c.Fields)
                {
                    w.BeginObject();
                    w.Field("name", f.Name);
                    w.Field("type", f.Type.ToString());
                    w.Field("required", f.Required);
                    w.Field("unit", f.Unit);
                    w.Field("semantics", f.Semantics);
                    if (f.Min.HasValue) w.Field("min", f.Min.Value);
                    if (f.Max.HasValue) w.Field("max", f.Max.Value);
                    w.Field("maxLength", f.MaxLength);
                    w.Field("privacy", f.Privacy.ToString());
                    w.Field("fingerprint", f.FingerprintInclude);
                    w.EndObject();
                }
                w.EndArray();
                w.EndObject();
            }
            w.EndArray();
            w.EndObject();
            return w.ToString();
        }

        public static string ToMarkdown(ContractRegistry registry)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Telemetry Data Catalog");
            sb.AppendLine();
            foreach (var c in registry.All())
            {
                sb.AppendLine($"## `{c.EventType}` — {c.ContractId} v{c.Version}");
                sb.AppendLine();
                sb.AppendLine(c.Description ?? "");
                sb.AppendLine();
                sb.AppendLine($"- Producer: `{c.Producer}`  Priority: `{c.Priority}`  Privacy: `{c.Privacy}`  Retention: `{c.Retention}`");
                sb.AppendLine($"- Sampling: `{c.SamplingRate}`  Normalization: `{c.NormalizationId ?? "-"}`");
                sb.AppendLine();
                sb.AppendLine("| Field | Type | Req | Unit | Semantics | Fingerprint |");
                sb.AppendLine("|---|---|---|---|---|---|");
                foreach (var f in c.Fields)
                    sb.AppendLine($"| `{f.Name}` | {f.Type} | {(f.Required ? "yes" : "no")} | {f.Unit ?? "-"} | {f.Semantics ?? "-"} | {(f.FingerprintInclude ? "yes" : "no")} |");
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
