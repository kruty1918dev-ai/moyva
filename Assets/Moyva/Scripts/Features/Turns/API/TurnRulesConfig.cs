using System;
using Kruty1918.JsonConfig;

namespace Kruty1918.Moyva.Turns.API
{
    /// <summary>
    /// JSON-authored turn rules preset (document id <c>turn-rules</c>).
    /// Supplies the default flow mode per session kind and the simultaneous
    /// round timer. The host lobby may override the defaults per match; the
    /// resolved values are synchronized to clients through the turn-sync
    /// payload, so this document never has to be read on remote peers.
    /// </summary>
    [Serializable]
    public sealed class TurnRulesConfig : JsonConfigObject
    {
        /// <summary>Default flow mode for multiplayer sessions.</summary>
        public string MultiplayerMode = "simultaneous";

        /// <summary>Default flow mode for solo/direct/bot sessions.</summary>
        public string SoloMode = "sequential";

        /// <summary>Enable the simultaneous round timer by default.</summary>
        public bool RoundTimerEnabled = true;

        /// <summary>Default round timer in seconds (clamped to [5, 3600]).</summary>
        public int RoundTimerSeconds = 90;

        /// <summary>
        /// Keep the round timer paused while any living faction still owes its
        /// initial castle, so round 1 cannot expire before everyone placed it.
        /// </summary>
        public bool HoldTimerUntilInitialCastlesPlaced = true;
    }
}
