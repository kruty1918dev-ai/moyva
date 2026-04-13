using System.Collections.Generic;
using Kruty1918.Moyva.Multiplayer.Core;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    /// <summary>
    /// Вибирає нового хоста коли поточний хост від'єднується.
    /// Пріоритет — перший живий учасник-людина (не бот).
    /// </summary>
    internal sealed class HostMigrationService : IHostMigrationService
    {
        private readonly IMultiplayerLogger _logger;

        public HostMigrationService(IMultiplayerLogger logger)
        {
            _logger = logger;
        }

        public Participant ChooseNewHost(IReadOnlyList<Participant> remaining)
        {
            foreach (var participant in remaining)
            {
                if (!participant.IsBot)
                {
                    _logger.Info($"HostMigrationService: новий хост → {participant.Identity}");
                    return participant.AsHost();
                }
            }

            _logger.Warn("HostMigrationService: жодного живого людського учасника — хост не призначено.");
            return null;
        }
    }
}
