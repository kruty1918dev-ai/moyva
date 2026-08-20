namespace Kruty1918.Moyva.Units.API
{
    public interface IUnitCommandAuthorityEndpointRegistry
    {
        void AttachUnitCommandServices(
            IUnitMovementService movementService,
            IUnitOwnershipQuery ownershipQuery);

        void DetachUnitCommandServices(
            IUnitMovementService movementService,
            IUnitOwnershipQuery ownershipQuery);
    }
}
