using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.BotAI.Diagnostics
{
    public sealed class BotDiagnosticsRingBuffer : IBotDiagnosticsBuffer
    {
        private readonly List<BotDiagnosticEvent> _events;
        private readonly int _capacity;

        public BotDiagnosticsRingBuffer(BotDiagnosticsSettings settings)
        {
            settings ??= new BotDiagnosticsSettings();
            settings.Normalize();
            _capacity = settings.BufferCapacity;
            _events = new List<BotDiagnosticEvent>(_capacity);
        }

        public int Count => _events.Count;
        public int Capacity => _capacity;

        public void Add(BotDiagnosticEvent diagnosticEvent)
        {
            _events.Add(diagnosticEvent);
            int excess = _events.Count - _capacity;
            if (excess > 0)
                _events.RemoveRange(0, excess);
        }

        public IReadOnlyList<BotDiagnosticEvent> Capture()
            => _events.ToArray();

        public IReadOnlyList<BotDiagnosticEvent> CaptureSince(long sequenceExclusive)
        {
            var result = new List<BotDiagnosticEvent>();
            for (int i = 0; i < _events.Count; i++)
            {
                if (_events[i].Sequence > sequenceExclusive)
                    result.Add(_events[i]);
            }
            return result;
        }

        public void Clear() => _events.Clear();
    }
}
