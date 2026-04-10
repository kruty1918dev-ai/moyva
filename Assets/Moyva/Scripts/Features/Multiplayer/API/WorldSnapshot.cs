using System;

namespace Kruty1918.Moyva.Multiplayer.Persistence
{
    /// <summary>
    /// Lightweight world snapshot metadata.
    /// Actual world data is opaque — handled by the existing save system.
    /// </summary>
    public sealed class WorldSnapshot
    {
        public string WorldId { get; }
        public int Version { get; }
        public uint Checksum { get; }

        public WorldSnapshot(string worldId, int version, uint checksum)
        {
            WorldId = worldId ?? throw new ArgumentNullException(nameof(worldId));
            Version = version;
            Checksum = checksum;
        }

        public override string ToString() =>
            $"World[{WorldId}] v{Version} crc={Checksum:X8}";
    }
}
