using System.Collections.Generic;
using Kruty1918.Moyva.Multiplayer.Core;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    /// <summary>
    /// Вибирає нового хоста коли поточний хост від'єднується.
    /// Новим хостом стає перший активний учасник.
    /// </summary>
    internal sealed class HostMigrationService : IHostMigrationService
    {
        /// <summary>Обирає першого активного учасника новим хостом.</summary>
        public Participant ChooseNewHost(IReadOnlyList<Participant> remaining)
        {
            foreach (var participant in remaining)
                return participant.AsHost();

            return null;
        }
    }
}
