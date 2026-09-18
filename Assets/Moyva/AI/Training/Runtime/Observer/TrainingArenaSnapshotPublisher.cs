using System;
using System.Collections.Generic;
using System.Linq;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingArenaSnapshotPublisher
    {
        private readonly Func<IReadOnlyList<TrainingEnvironment>> _environments;
        private readonly TrainingObserverServer _server;
        private readonly TrainingDecisionJournal _journal;
        private readonly double _minIntervalSeconds;
        private double _nextPeriodicAt;

        public TrainingArenaSnapshotPublisher(Func<IReadOnlyList<TrainingEnvironment>> environments,
            TrainingObserverServer server, TrainingDecisionJournal journal, float snapshotHz)
        {
            _environments = environments ?? throw new ArgumentNullException(nameof(environments));
            _server = server ?? throw new ArgumentNullException(nameof(server));
            _journal = journal;
            _minIntervalSeconds = 1.0 / Math.Max(0.1f, snapshotHz);
        }

        // Must be called from the Unity/main simulation thread. It is the only snapshot capture boundary.
        public void Tick(double realtimeSeconds)
        {
            var environments = _environments();
            if (environments == null || environments.Count == 0) return;
            bool periodic = realtimeSeconds >= _nextPeriodicAt;
            if (periodic) _nextPeriodicAt = realtimeSeconds + _minIntervalSeconds;

            int[] arenaIds = environments.Select(e => e.EnvironmentId).ToArray();
            int[] targets = _server.CollectSnapshotTargets(arenaIds, periodic);
            if (targets.Length == 0) return;
            var targetSet = new HashSet<int>(targets);
            long sequence = _journal?.CurrentSequence ?? _server.LastSequence;
            foreach (var environment in environments)
            {
                if (!targetSet.Contains(environment.EnvironmentId)) continue;
                ArenaSnapshot snapshot = environment.CaptureObserverSnapshot(_server.SessionId, sequence);
                _server.PublishSnapshot(snapshot, periodic);
            }
        }
    }
}
