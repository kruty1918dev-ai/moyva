using System.Collections.Generic;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Persistence;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>
    /// Enforces participant rules: max counts, strict world lock, etc.
    /// </summary>
    public interface IParticipantPolicyService
    {
        bool CanJoin(
            ParticipantIdentity candidate,
            IReadOnlyList<Participant> currentParticipants,
            SessionRules rules,
            WorldSnapshot worldSnapshot);
    }
}
