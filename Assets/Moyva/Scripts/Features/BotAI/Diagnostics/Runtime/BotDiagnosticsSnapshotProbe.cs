using System;
using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Diagnostics
{
    internal sealed class BotDiagnosticsSnapshotProbe : ITickable, IInitializable
    {
        private readonly ITurnService _turns;
        private readonly IBotWorldSnapshotBuilder _snapshots;
        private readonly IEconomyInfoMediator _economy;
        private readonly IBotDiagnosticsLogger _log;
        private readonly BotDiagnosticsSettings _settings;

        private double _nextProbe;
        private string _lastSignature = string.Empty;

        public BotDiagnosticsSnapshotProbe(
            IBotDiagnosticsLogger log,
            BotDiagnosticsSettings settings,
            [InjectOptional] ITurnService turns = null,
            [InjectOptional] IBotWorldSnapshotBuilder snapshots = null,
            [InjectOptional] IEconomyInfoMediator economy = null)
        {
            _log = log;
            _settings = settings;
            _turns = turns;
            _snapshots = snapshots;
            _economy = economy;
        }

        public void Initialize()
        {
            if (_snapshots == null)
            {
                _log.Error(
                    BotDiagnosticCategory.Snapshot,
                    "SNAPSHOT.BUILDER_MISSING",
                    "IBotWorldSnapshotBuilder is unavailable.",
                    reason:
                        "The bot cannot inspect its own units/buildings/visible enemies if the snapshot builder is not bound.");
            }
        }

        public void Tick()
        {
            if (_turns == null ||
                !_turns.IsActiveFactionBot ||
                _turns.Phase != TurnPhase.AwaitingInput)
            {
                return;
            }

            double now = Time.realtimeSinceStartupAsDouble;
            if (now < _nextProbe)
                return;

            _nextProbe =
                now + Math.Max(0.25, _settings.SnapshotProbeSeconds);

            string owner = _turns.ActiveOwnerId;
            if (string.IsNullOrWhiteSpace(owner))
            {
                _log.Error(
                    BotDiagnosticCategory.Snapshot,
                    "SNAPSHOT.ACTIVE_OWNER_EMPTY",
                    "Bot flag is active but ActiveOwnerId is empty.");
                return;
            }

            if (_snapshots == null)
                return;

            try
            {
                BotWorldSnapshot snapshot =
                    _snapshots.Build(owner, _turns.GlobalTurn);

                if (snapshot == null)
                {
                    _log.Error(
                        BotDiagnosticCategory.Snapshot,
                        "SNAPSHOT.NULL",
                        "BotWorldSnapshotBuilder returned null.",
                        ownerId: owner);
                    return;
                }

                string resources = BuildResources(owner);
                string signature =
                    $"turn={snapshot.GlobalTurn};owner={snapshot.OwnerId};" +
                    $"ownUnits={snapshot.OwnUnits.Count};ownBuildings={snapshot.OwnBuildings.Count};" +
                    $"visibleUnits={snapshot.VisibleEnemyUnits.Count};visibleBuildings={snapshot.VisibleEnemyBuildings.Count};" +
                    $"memory={snapshot.Memory.Count};readyRecruitment={snapshot.ReadyRecruitmentItems.Count};resources={resources}";

                if (!_settings.LogSnapshotHeartbeat &&
                    string.Equals(signature, _lastSignature, StringComparison.Ordinal))
                {
                    return;
                }

                _lastSignature = signature;

                _log.Trace(
                    BotDiagnosticCategory.Snapshot,
                    "SNAPSHOT.STATE",
                    "Captured bot world snapshot.",
                    details:
                        $"start={snapshot.StartPosition}; round={snapshot.Round}; globalTurn={snapshot.GlobalTurn}; " +
                        $"phase={snapshot.Phase}; actions={snapshot.ActionsThisTurn}; " +
                        $"ownUnits={snapshot.OwnUnits.Count}; ownBuildings={snapshot.OwnBuildings.Count}; " +
                        $"visibleEnemyUnits={snapshot.VisibleEnemyUnits.Count}; " +
                        $"visibleEnemyBuildings={snapshot.VisibleEnemyBuildings.Count}; " +
                        $"memory={snapshot.Memory.Count}; readyRecruitment={snapshot.ReadyRecruitmentItems.Count}; " +
                        $"resources=[{resources}]",
                    ownerId: owner);
            }
            catch (Exception exception)
            {
                _log.Exception(
                    BotDiagnosticCategory.Snapshot,
                    "SNAPSHOT.EXCEPTION",
                    exception,
                    "Exception while building diagnostic bot snapshot.",
                    owner);
            }
        }

        private string BuildResources(string ownerId)
        {
            if (_economy == null)
                return "economy-mediator-unavailable";

            IReadOnlyDictionary<string, float> totals;
            try
            {
                totals = _economy.GetOwnerResourceTotals(ownerId);
            }
            catch (Exception exception)
            {
                return $"economy-read-failed:{exception.GetType().Name}:{exception.Message}";
            }

            if (totals == null || totals.Count == 0)
                return "none";

            var keys = new List<string>(totals.Keys);
            keys.Sort(StringComparer.Ordinal);

            var b = new StringBuilder();
            for (int i = 0; i < keys.Count; i++)
            {
                if (i > 0)
                    b.Append(", ");

                string id = keys[i];
                b.Append(id).Append('=').Append(totals[id].ToString("0.##"));
            }
            return b.ToString();
        }
    }
}
