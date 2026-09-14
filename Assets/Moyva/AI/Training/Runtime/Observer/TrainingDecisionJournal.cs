using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingDecisionJournal
    {
        private readonly object _gate = new object();
        private readonly int _capacity;
        private readonly Queue<AgentDecisionEvent> _events;
        private readonly string _jsonlPath;
        private readonly long _maxBytes;
        private long _sequence;

        public event Action<AgentDecisionEvent> Appended;

        public TrainingDecisionJournal(int capacity = 4096, string jsonlPath = null, long maxBytes = 8 * 1024 * 1024)
        {
            _capacity = Math.Max(64, capacity);
            _events = new Queue<AgentDecisionEvent>(_capacity);
            _jsonlPath = string.IsNullOrWhiteSpace(jsonlPath) ? null : Path.GetFullPath(jsonlPath);
            _maxBytes = Math.Max(1024 * 1024, maxBytes);
        }

        public long CurrentSequence
        {
            get { lock (_gate) return _sequence; }
        }

        public long EarliestSequence
        {
            get { lock (_gate) return _events.Count == 0 ? _sequence + 1 : _events.Peek().sequence; }
        }

        public long NextSequence()
        {
            lock (_gate) return ++_sequence;
        }

        public void Append(AgentDecisionEvent entry)
        {
            if (entry == null) return;
            lock (_gate)
            {
                if (entry.sequence <= 0) entry.sequence = ++_sequence;
                else _sequence = Math.Max(_sequence, entry.sequence);
                while (_events.Count >= _capacity) _events.Dequeue();
                _events.Enqueue(entry);
            }
            AppendJsonl(entry);
            Appended?.Invoke(entry);
        }

        public AgentDecisionEvent[] Query(int? arena = null, string agent = null, string action = null,
            bool errorsOnly = false, long afterSequence = 0)
        {
            lock (_gate)
            {
                return _events.Where(e => e.sequence > afterSequence
                    && (!arena.HasValue || e.arenaId == arena.Value)
                    && (string.IsNullOrWhiteSpace(agent) || string.Equals(e.agentId, agent, StringComparison.Ordinal))
                    && (string.IsNullOrWhiteSpace(action) || string.Equals(e.actionId, action, StringComparison.Ordinal))
                    && (!errorsOnly || !string.IsNullOrWhiteSpace(e.rejectionReason)))
                    .ToArray();
            }
        }

        // Sequence is observer/session monotonic. This compatibility method may only advance it.
        public void ResetSequence(long value = 0)
        {
            lock (_gate) _sequence = Math.Max(_sequence, Math.Max(0, value));
        }

        private void AppendJsonl(AgentDecisionEvent entry)
        {
            if (string.IsNullOrEmpty(_jsonlPath)) return;
            try
            {
                string directory = Path.GetDirectoryName(_jsonlPath);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                if (File.Exists(_jsonlPath) && new FileInfo(_jsonlPath).Length > _maxBytes)
                {
                    string rotated = _jsonlPath + "." + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".old";
                    if (File.Exists(rotated)) File.Delete(rotated);
                    File.Move(_jsonlPath, rotated);
                }
                File.AppendAllText(_jsonlPath, JsonUtility.ToJson(entry) + Environment.NewLine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Cannot append Moyva training decision journal: " + e.Message);
            }
        }
    }
}
