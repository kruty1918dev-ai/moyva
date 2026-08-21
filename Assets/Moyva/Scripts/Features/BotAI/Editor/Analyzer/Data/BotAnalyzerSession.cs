using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer
{
    [Serializable]
    public sealed class BotAnalyzerSession
    {
        private readonly BotAnalyzerSettings _settings;
        private readonly List<BotAnalyzerFrame> _frames = new();
        private readonly List<BotAnalyzerEvent> _events = new();
        private long _eventSequence;

        public BotAnalyzerSession(BotAnalyzerSettings settings)
        {
            _settings = settings ?? BotAnalyzerSettings.CreateDefault();
            StartedUtc = DateTime.UtcNow.ToString("O");
        }

        public string StartedUtc { get; private set; }
        public string LastOwnerId { get; private set; } = string.Empty;
        public IReadOnlyList<BotAnalyzerFrame> Frames => _frames;
        public IReadOnlyList<BotAnalyzerEvent> Events => _events;
        public BotAnalyzerFrame LatestFrame => _frames.Count == 0 ? null : _frames[^1];

        public void Append(BotAnalyzerFrame frame, IReadOnlyList<BotAnalyzerEvent> events)
        {
            if (frame == null) return;
            LastOwnerId = frame.OwnerId ?? string.Empty;
            _frames.Add(frame);
            TrimFront(_frames, Math.Max(50, _settings.MaxFrames));
            if (events == null) return;
            foreach (BotAnalyzerEvent e in events)
            {
                if (e == null) continue;
                e.Sequence = ++_eventSequence;
                _events.Add(e);
            }
            TrimFront(_events, Math.Max(100, _settings.MaxTimelineEvents));
        }

        public void AppendEvent(BotAnalyzerEvent e)
        {
            if (e == null) return;
            e.Sequence = ++_eventSequence;
            _events.Add(e);
            TrimFront(_events, Math.Max(100, _settings.MaxTimelineEvents));
        }

        public List<BotAnalyzerEvent> GetFilteredEvents(
            BotAnalyzerTimelineFilter filter, string search, int maxCount, bool newestFirst)
        {
            var result = new List<BotAnalyzerEvent>();
            string needle = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
            if (newestFirst)
            {
                for (int i = _events.Count - 1; i >= 0 && result.Count < maxCount; i--)
                    TryAdd(_events[i], filter, needle, result);
            }
            else
            {
                int start = Math.Max(0, _events.Count - maxCount);
                for (int i = start; i < _events.Count && result.Count < maxCount; i++)
                    TryAdd(_events[i], filter, needle, result);
            }
            return result;
        }

        public void Clear()
        {
            _frames.Clear();
            _events.Clear();
            _eventSequence = 0;
            StartedUtc = DateTime.UtcNow.ToString("O");
            LastOwnerId = string.Empty;
        }

        private static void TryAdd(BotAnalyzerEvent e, BotAnalyzerTimelineFilter filter, string needle, List<BotAnalyzerEvent> result)
        {
            if (!BotAnalyzerTimelineBuilder.MatchesFilter(e, filter)) return;
            if (needle != null)
            {
                string haystack = $"{e.Type} {e.Title} {e.Detail} {e.ActorId} {e.TargetId}";
                if (haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) < 0) return;
            }
            result.Add(e);
        }

        private static void TrimFront<T>(List<T> list, int max)
        {
            int excess = list.Count - max;
            if (excess > 0) list.RemoveRange(0, excess);
        }
    }
}
