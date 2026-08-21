using System.Text;

namespace Kruty1918.Moyva.BotAI.Diagnostics
{
    public sealed class BotDiagnosticsFormatter
    {
        public string Format(BotDiagnosticEvent e)
        {
            var b = new StringBuilder(256);

            b.Append(BotDiagnosticsConstants.Prefix)
                .Append('[').Append(e.Level).Append(']')
                .Append('[').Append(e.Category).Append(']')
                .Append('[').Append(string.IsNullOrWhiteSpace(e.Code) ? "NO-CODE" : e.Code).Append(']')
                .Append("[seq=").Append(e.Sequence).Append(']')
                .Append("[turn=").Append(e.GlobalTurn).Append(']');

            if (!string.IsNullOrWhiteSpace(e.OwnerId))
                b.Append("[owner=").Append(e.OwnerId).Append(']');

            if (!string.IsNullOrWhiteSpace(e.Phase))
                b.Append("[phase=").Append(e.Phase).Append(']');

            b.Append(' ').Append(e.Message);

            if (!string.IsNullOrWhiteSpace(e.Reason))
                b.Append(" | reason=").Append(e.Reason);

            if (!string.IsNullOrWhiteSpace(e.Details))
                b.Append(" | details=").Append(e.Details);

            return b.ToString();
        }
    }
}
