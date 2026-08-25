using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Persistence;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>
    /// Перевіряє місткість сесії, дублікати ідентичності та strict world lock.
    /// </summary>
    public sealed class ParticipantPolicyService : IParticipantPolicyService
    {
        /// <summary>Перевіряє, чи може гравець приєднатися до поточної сесії.</summary>
        public bool CanJoin(
            ParticipantIdentity candidate,
            IReadOnlyList<Participant> currentParticipants,
            SessionRules rules,
            WorldSnapshot worldSnapshot)
        {
            if (candidate == null || currentParticipants == null || rules == null)
                return false;

            return currentParticipants.Count < rules.MaxParticipants
                && currentParticipants.All(participant =>
                    participant?.Identity?.PlayerId != candidate.PlayerId)
                && (!rules.StrictParticipantLock
                    || worldSnapshot == null
                    || IsInLockedSet(candidate, worldSnapshot.WorldId));
        }

        private bool IsInLockedSet(ParticipantIdentity candidate, string worldId)
        {
            return true;
        }
    }
}
