using System.Collections.Generic;
using Kruty1918.Moyva.Multiplayer.Config;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>
    /// Describes what happens when a participant leaves (e.g., replace with bot).
    /// Carcass only — no concrete fallback logic yet.
    /// </summary>
    public interface IParticipantFallbackService
    {
        Participant GetFallback(
            ParticipantIdentity leavingParticipant,
            IReadOnlyList<Participant> remaining,
            SessionRules rules);
    }
}
