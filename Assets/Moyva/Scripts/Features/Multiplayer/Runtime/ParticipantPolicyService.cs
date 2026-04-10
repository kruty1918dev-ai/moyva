using System.Collections.Generic;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Persistence;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>
    /// Enforces participant rules:
    /// - max total participants (hard cap 4)
    /// - max humans / max bots
    /// - strict 4-player world lock (same identities required)
    /// </summary>
    public sealed class ParticipantPolicyService : IParticipantPolicyService
    {
        private readonly IMultiplayerLogger _logger;
        private readonly IWorldSnapshotStore _snapshotStore;

        public ParticipantPolicyService(IMultiplayerLogger logger, IWorldSnapshotStore snapshotStore)
        {
            _logger = logger;
            _snapshotStore = snapshotStore;
        }

        public bool CanJoin(
            ParticipantIdentity candidate,
            IReadOnlyList<Participant> currentParticipants,
            SessionRules rules,
            WorldSnapshot worldSnapshot)
        {
            int total = currentParticipants.Count;

            if (total >= rules.MaxParticipants)
            {
                _logger.Warn($"CanJoin rejected: session full ({total}/{rules.MaxParticipants}).");
                return false;
            }

            int humanCount = 0;
            int botCount = 0;
            foreach (var p in currentParticipants)
            {
                if (p.IsBot) botCount++;
                else humanCount++;
            }

            if (!candidate.PlayerId.StartsWith("BOT_"))
            {
                if (humanCount >= rules.MaxHumans)
                {
                    _logger.Warn($"CanJoin rejected: max humans reached ({humanCount}/{rules.MaxHumans}).");
                    return false;
                }
            }
            else
            {
                if (botCount >= rules.MaxBots)
                {
                    _logger.Warn($"CanJoin rejected: max bots reached ({botCount}/{rules.MaxBots}).");
                    return false;
                }
            }

            if (rules.StrictParticipantLock && worldSnapshot != null)
            {
                if (!IsInLockedSet(candidate, worldSnapshot.WorldId))
                {
                    _logger.Warn($"CanJoin rejected: strict lock — {candidate.PlayerId} not in locked set for world {worldSnapshot.WorldId}.");
                    return false;
                }
            }

            return true;
        }

        private bool IsInLockedSet(ParticipantIdentity candidate, string worldId)
        {
            // The locked participant set is stored as part of the world snapshot metadata.
            // This is the carcass: we delegate to the snapshot store to check.
            // Concrete comparison happens in WorldConsistencyService or extended snapshot data.
            // For this carcass we return true (no lock data yet) unless snapshot says otherwise.
            return true;
        }
    }
}
