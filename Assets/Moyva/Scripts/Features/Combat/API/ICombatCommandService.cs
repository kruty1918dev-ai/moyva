using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.Combat.API
{
    public interface ICombatCommandService
    {
        bool TryPreview(
            string attackerEntityId,
            string targetEntityId,
            out CombatCommandPreview preview,
            out string reason);

        Task<CombatCommandResult> ExecuteAsync(
            string requesterOwnerId,
            string attackerEntityId,
            string targetEntityId,
            CancellationToken token = default);
    }
}
