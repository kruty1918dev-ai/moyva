using System;
using System.IO;
using System.Text;
using Kruty1918.Moyva.Diagnostics.API;
using UnityEngine;

namespace Kruty1918.Moyva.Diagnostics.Runtime.Sinks
{
    internal sealed class JsonFileDiagnosticSink : IDiagnosticSink
    {
        private const string DirectoryName = "MoyvaDiagnostics";
        private const string FileName = "diagnostic-flows.jsonl";

        private readonly string _filePath;

        public JsonFileDiagnosticSink()
        {
            _filePath = Path.Combine(Application.persistentDataPath, DirectoryName, FileName);
        }

        public void Emit(DiagnosticFlowAnalysis analysis, string formattedMessage)
        {
            if (analysis == null)
                return;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
                File.AppendAllText(_filePath, BuildJsonLine(analysis) + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MoyvaFlow] JsonFileDiagnosticSink failed: {ex.Message}");
            }
        }

        private static string BuildJsonLine(DiagnosticFlowAnalysis analysis)
        {
            var builder = new StringBuilder(256);
            builder.Append('{')
                .Append("\"flow\":\"").Append(Escape(analysis.FlowName)).Append("\",")
                .Append("\"trace\":\"").Append(Escape(analysis.TraceId)).Append("\",")
                .Append("\"status\":\"").Append(analysis.Status).Append("\",")
                .Append("\"elapsedMs\":").Append(((int)analysis.ElapsedMilliseconds).ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(',')
                .Append("\"breakPoint\":\"").Append(Escape(analysis.RootBreakPointStepId)).Append("\",")
                .Append("\"lastCompleted\":\"").Append(Escape(analysis.LastCompletedStepId)).Append("\"")
                .Append('}');
            return builder.ToString();
        }

        private static string Escape(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
