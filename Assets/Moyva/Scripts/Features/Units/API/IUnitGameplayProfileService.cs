using Kruty1918.Moyva.Animations.API;

namespace Kruty1918.Moyva.Units.API
{
    /// <summary>
    /// Public runtime API entry point for gameplay-ready unit parameters.
    /// Consumers from movement, fog, combat, UI can use this instead of reading editor data directly.
    /// </summary>
    public interface IUnitGameplayProfileService
    {
        bool TryGetProfile(string typeId, out UnitGameplayProfile profile);
        UnitGameplayProfile GetOrDefault(string typeId);
        int ResolveVisionRange(string typeId, int terrainHeightLevel = 0, int minVision = 1, int maxVision = 128);
        float RollStartingStamina(string typeId);
        PathAnimationSettings ResolveMovementAnimationSettings(string typeId);
    }
}