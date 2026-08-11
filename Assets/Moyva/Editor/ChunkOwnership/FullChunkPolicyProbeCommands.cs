#if MOYVA_LEGACY_SCRIPTABLEOBJECT_EDITOR
using Kruty1918.Moyva.MapChunks.API;
using Kruty1918.Moyva.MapChunks.Runtime;
using Unity.Pipeline.Commands;
using UnityEngine;

namespace Kruty1918.Moyva.Editor.ChunkOwnership
{
    public static class FullChunkPolicyProbeCommands
    {
        [CliCommand(
            "moyva-full-chunk-policy-probe",
            "Verify the live compiled fixed-16 full-chunk policy.")]
        public static string Probe()
        {
            Vector2Int cropped = MapChunkSizePolicy.CropMapSize(50, 50);
            var settings = ScriptableObject.CreateInstance<MapChunkSettingsSO>();
            try
            {
                settings.ChunkSize = 8;
                IMapChunkSettingsProvider provider = settings;
                var layout = new MapChunkLayoutService(provider);
                layout.Configure(50, 50, 1f, false, default);

                bool allFull = true;
                for (int i = 0; i < layout.Chunks.Count; i++)
                {
                    RectInt rect = layout.Chunks[i].TileRect;
                    if (rect.width != 16 || rect.height != 16)
                    {
                        allFull = false;
                        break;
                    }
                }

                bool lastInside = layout.TryGetChunkCoord(
                    new Vector2Int(47, 47), out MapChunkCoord owner);
                bool croppedCellRejected = !layout.TryGetChunkCoord(
                    new Vector2Int(48, 48), out _);

                bool ok = cropped == new Vector2Int(48, 48)
                    && MapChunkSizePolicy.ChunkSize == 16
                    && provider.ChunkSize == 16
                    && layout.ChunkSize == 16
                    && layout.Width == 48
                    && layout.Height == 48
                    && layout.Chunks.Count == 9
                    && allFull
                    && lastInside
                    && owner.X == 2
                    && owner.Y == 2
                    && croppedCellRejected;

                return "FULL_CHUNK_POLICY_PROBE "
                    + $"ok={ok} request=50x50 effective={cropped.x}x{cropped.y} "
                    + $"chunkSize={layout.ChunkSize} chunks={layout.Chunks.Count} "
                    + $"allDescriptors16={allFull} lastOwner=({owner.X},{owner.Y}) "
                    + $"cell48Rejected={croppedCellRejected}";
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }
    }
}

#endif
