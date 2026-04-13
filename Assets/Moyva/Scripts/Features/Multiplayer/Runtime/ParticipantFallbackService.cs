using System.Collections.Generic;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    /// <summary>
    /// Визначає, який учасник замінює гравця що покинув сесію.
    /// Якщо правила дозволяють, повертає бота-замінника; інакше — null.
    /// </summary>
    internal sealed class ParticipantFallbackService : IParticipantFallbackService
    {
        public Participant GetFallback(
            ParticipantIdentity leavingParticipant,
            IReadOnlyList<Participant> remaining,
            SessionRules rules)
        {
            if (!rules.AllowBotsFallbackOnLeave)
                return null;

            var botIdentity = new ParticipantIdentity(
                ParticipantIdentity.BotIdPrefix + leavingParticipant.PlayerId,
                leavingParticipant.Nickname);

            return new Participant(botIdentity, isBot: true, isHost: false);
        }
    }
}
