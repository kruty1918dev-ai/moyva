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

    public interface ICombatRemoteCommandRequester
    {
        event System.Action<CombatRemoteCommandResult> AttackRejected;

        bool TryRequestAttack(
            string requesterOwnerId,
            string attackerEntityId,
            string targetEntityId,
            out string reason);
    }

    public readonly struct CombatRemoteCommandResult
    {
        public CombatRemoteCommandResult(
            string requesterOwnerId,
            string attackerEntityId,
            string targetEntityId,
            string requestId,
            string reason)
        {
            RequesterOwnerId = requesterOwnerId ?? string.Empty;
            AttackerEntityId = attackerEntityId ?? string.Empty;
            TargetEntityId = targetEntityId ?? string.Empty;
            RequestId = requestId ?? string.Empty;
            Reason = reason ?? string.Empty;
        }

        public string RequesterOwnerId { get; }
        public string AttackerEntityId { get; }
        public string TargetEntityId { get; }
        public string RequestId { get; }
        public string Reason { get; }
    }
}
