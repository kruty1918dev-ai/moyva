using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Turns.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// Resolves the local player's owner id and pushes it into the fog service
    /// so the shared fog grid becomes the local perspective.
    ///
    /// Resolution order matches the rest of bootstrap: the authoritative turn
    /// service first, then spawn assignments / session / launch context via
    /// <see cref="IBootstrapOwnerIdResolver"/>.
    /// Retried every tick until a non-empty owner is applied — safe to keep
    /// calling afterwards since <see cref="IFogLocalPerspective.
    /// SetLocalPerspectiveOwnerId"/> no-ops on an unchanged value.
    /// </summary>
    internal sealed class FogLocalPerspectiveInitializer : IInitializable, ITickable
    {
        private readonly IFogOfWarService _fog;
        private readonly IBootstrapOwnerIdResolver _ownerResolver;
        private readonly ITurnService _turns;

        public FogLocalPerspectiveInitializer(
            IFogOfWarService fog,
            IBootstrapOwnerIdResolver ownerResolver,
            [InjectOptional] ITurnService turns = null)
        {
            _fog = fog;
            _ownerResolver = ownerResolver;
            _turns = turns;
        }

        public void Initialize() => TryApply();

        public void Tick() => TryApply();

        private void TryApply()
        {
            if (_fog == null)
                return;

            string ownerId = _turns?.LocalOwnerId;
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                try
                {
                    ownerId = _ownerResolver?.ResolveActiveOwnerId();
                }
                catch (System.Exception)
                {
                    // Construction/other services may not be resolvable yet.
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(ownerId))
                return;

            _fog.SetLocalPerspectiveOwnerId(ownerId);
        }
    }
}
