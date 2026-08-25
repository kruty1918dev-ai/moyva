using System;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitAuthorityEndpointBridge :
        IInitializable,
        IDisposable
    {
        private readonly IUnitMovementService _movementService;
        private readonly IUnitOwnershipQuery _ownershipQuery;
        private readonly IUnitCommandAuthorityEndpointRegistry _registry;

        public UnitAuthorityEndpointBridge(
            IUnitMovementService movementService,
            IUnitOwnershipQuery ownershipQuery,
            [InjectOptional] IUnitCommandAuthorityEndpointRegistry registry = null)
        {
            _movementService = movementService;
            _ownershipQuery = ownershipQuery;
            _registry = registry;
        }

        public void Initialize()
        {
            if (_registry == null)
            {
                Debug.LogError(
                    "[MOYVA_MOVE][AUTHORITY_BRIDGE] "
                    + "parent authority registry is unavailable.");
                return;
            }

            _registry.AttachUnitCommandServices(
                _movementService,
                _ownershipQuery);
        }

        public void Dispose()
        {
            _registry?.DetachUnitCommandServices(
                _movementService,
                _ownershipQuery);
        }
    }
}
