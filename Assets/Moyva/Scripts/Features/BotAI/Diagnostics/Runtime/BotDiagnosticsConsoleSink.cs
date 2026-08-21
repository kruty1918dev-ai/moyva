using UnityEngine;

namespace Kruty1918.Moyva.BotAI.Diagnostics
{
    internal sealed class BotDiagnosticsConsoleSink
    {
        private readonly BotDiagnosticsFormatter _formatter;

        public BotDiagnosticsConsoleSink(BotDiagnosticsFormatter formatter)
        {
            _formatter = formatter;
        }

        public void Write(BotDiagnosticEvent diagnosticEvent)
        {
            string text = _formatter.Format(diagnosticEvent);

            switch (diagnosticEvent.Level)
            {
                case BotDiagnosticLevel.Critical:
                case BotDiagnosticLevel.Error:
                    Debug.LogError(text);
                    break;

                case BotDiagnosticLevel.Warning:
                    Debug.LogWarning(text);
                    break;

                default:
                    Debug.Log(text);
                    break;
            }
        }
    }
}
