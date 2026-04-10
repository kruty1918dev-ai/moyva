using Kruty1918.Moyva.Multiplayer.Config;

namespace Kruty1918.Moyva.Multiplayer.Persistence
{
    /// <summary>
    /// Clones a world with new rules and a remapped participant slot layout.
    /// </summary>
    public interface IWorldCloneService
    {
        /// <summary>
        /// Clones the source world, applying new rules.
        /// Returns the new worldId.
        /// </summary>
        string CloneWorld(string sourceWorldId, SessionRules newRules, SlotMapping mapping);
    }

    /// <summary>Describes how participant slots are remapped in the cloned world.</summary>
    public sealed class SlotMapping
    {
        public int[] OldToNewSlotIndices { get; }

        public SlotMapping(int[] oldToNewSlotIndices)
        {
            OldToNewSlotIndices = oldToNewSlotIndices;
        }
    }
}
