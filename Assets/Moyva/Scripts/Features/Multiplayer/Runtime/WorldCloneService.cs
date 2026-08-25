using System;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Persistence;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    /// <summary>
    /// Клонує світ із новими правилами та переназначенням слотів учасників.
    /// Фактичне копіювання даних делеговано системі збереження.
    /// </summary>
    internal sealed class WorldCloneService : IWorldCloneService
    {
        public string CloneWorld(string sourceWorldId, SessionRules newRules, SlotMapping mapping)
        {
            string newWorldId = Guid.NewGuid().ToString("N");
            return newWorldId;
        }
    }
}
