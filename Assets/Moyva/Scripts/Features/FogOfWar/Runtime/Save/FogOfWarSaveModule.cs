using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Save module для FogOfWar explored state.
    /// Зберігає explored snapshot і fixed vision areas, але не зберігає короткоживучий visible state:
    /// він має відновлюватися з юнітів, reveal sources та bootstrap/runtime logic після load.
    /// </summary>
    [SaveModuleId("Kruty1918.Moyva.FogOfWar.Runtime.FogOfWarSaveModule")]
    internal sealed class FogOfWarSaveModule : ISaveModule
    {
        private const int FormatVersionWithFixedVisionAreas = -2;
        private const int FormatVersionWithOwnerSnapshots = -3;

        private readonly IFogExplorationSnapshotStore _fogSnapshotStore;
        private readonly IFogOwnerExplorationSnapshotStore _ownerSnapshotStore;
        private readonly FogOfWarService _runtimeFogOfWarService;
        /// <summary>
        /// Створює save module для поточного gameplay fog service.
        /// </summary>
        /// <param name="fogOfWarService">Fog service, з якого читається і в який завантажується save state.</param>
        public FogOfWarSaveModule(IFogExplorationSnapshotStore fogSnapshotStore)
        {
            _fogSnapshotStore = fogSnapshotStore;
            _ownerSnapshotStore = fogSnapshotStore as IFogOwnerExplorationSnapshotStore;
            _runtimeFogOfWarService = fogSnapshotStore as FogOfWarService;
        }

        /// <summary>
        /// Записує explored snapshot і fixed vision area snapshot у save context.
        /// </summary>
        /// <param name="context">Поточний save context з writer-ом.</param>
        public void OnSave(ISaveContext context)
        {
            context.Writer.Write(FormatVersionWithOwnerSnapshots);

            bool[,] snapshot = _fogSnapshotStore.GetExploredSnapshot();
            if (snapshot == null)
            {
                context.Writer.Write(0);
                context.Writer.Write(0);
                WriteFixedVisionAreas(context);
                WriteOwnerSnapshots(context);
                return;
            }

            int width = snapshot.GetLength(0);
            int height = snapshot.GetLength(1);

            context.Writer.Write(width);
            context.Writer.Write(height);

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    context.Writer.Write(snapshot[x, y]);

            WriteFixedVisionAreas(context);
            WriteOwnerSnapshots(context);
        }

        /// <summary>
        /// Відновлює explored snapshot і fixed vision areas із save context.
        /// </summary>
        /// <param name="context">Поточний load context з reader-ом.</param>
        public void OnLoad(ISaveContext context)
        {
            int markerOrWidth = context.Reader.ReadInt32();
            if (markerOrWidth < 0)
            {
                ReadVersioned(context, markerOrWidth);
                return;
            }

            ReadLegacyExploredSnapshot(context, markerOrWidth);
        }

        private void WriteFixedVisionAreas(ISaveContext context)
        {
            var areas = _runtimeFogOfWarService?.GetFixedVisionAreasSnapshot();
            int count = areas?.Count ?? 0;
            context.Writer.Write(count);

            if (areas == null)
                return;

            for (int index = 0; index < areas.Count; index++)
            {
                var area = areas[index];
                context.Writer.Write(area.AreaId ?? string.Empty);
                context.Writer.Write(area.Position.x);
                context.Writer.Write(area.Position.y);
                context.Writer.Write(area.VisionRange);
                context.Writer.Write((int)area.Shape);
            }
        }

        private void ReadVersioned(ISaveContext context, int version)
        {
            if (version != FormatVersionWithFixedVisionAreas
                && version != FormatVersionWithOwnerSnapshots)
            {
                return;
            }

            int width = context.Reader.ReadInt32();
            int height = context.Reader.ReadInt32();

            if (width > 0 && height > 0)
                ReadExploredSnapshot(context, width, height);

            int fixedAreaCount = context.Reader.ReadInt32();
            if (fixedAreaCount <= 0)
            {
                if (version == FormatVersionWithOwnerSnapshots)
                    ReadOwnerSnapshots(context);
                return;
            }

            var areas = new FogFixedVisionAreaSnapshot[fixedAreaCount];
            int validCount = 0;
            for (int index = 0; index < fixedAreaCount; index++)
            {
                string areaId = context.Reader.ReadString();
                int x = context.Reader.ReadInt32();
                int y = context.Reader.ReadInt32();
                int visionRange = context.Reader.ReadInt32();
                var shape = (FogRevealShape)context.Reader.ReadInt32();

                if (string.IsNullOrWhiteSpace(areaId) || visionRange <= 0)
                    continue;

                areas[validCount++] = new FogFixedVisionAreaSnapshot(areaId, new Vector2Int(x, y), visionRange, shape);
            }

            if (_runtimeFogOfWarService == null || validCount == 0)
            {
                if (version == FormatVersionWithOwnerSnapshots)
                    ReadOwnerSnapshots(context);
                return;
            }

            if (validCount != areas.Length)
            {
                var compacted = new FogFixedVisionAreaSnapshot[validCount];
                for (int index = 0; index < validCount; index++)
                    compacted[index] = areas[index];

                areas = compacted;
            }

            _runtimeFogOfWarService.LoadFixedVisionAreasSnapshot(areas);
            if (version == FormatVersionWithOwnerSnapshots)
                ReadOwnerSnapshots(context);
        }

        private void WriteOwnerSnapshots(ISaveContext context)
        {
            if (_ownerSnapshotStore == null)
            {
                context.Writer.Write(0);
                return;
            }

            var owners = _ownerSnapshotStore.GetKnownFogOwnerIds();
            var payloads = new System.Collections.Generic.List<(string OwnerId, bool[,] Snapshot)>();
            if (owners != null)
            {
                foreach (string ownerId in owners)
                {
                    if (string.IsNullOrWhiteSpace(ownerId))
                        continue;
                    bool[,] snapshot = _ownerSnapshotStore.GetExploredSnapshot(ownerId);
                    if (snapshot == null)
                        continue;
                    payloads.Add((ownerId.Trim(), snapshot));
                }
            }

            context.Writer.Write(payloads.Count);
            for (int index = 0; index < payloads.Count; index++)
            {
                var payload = payloads[index];
                context.Writer.Write(payload.OwnerId);
                WriteExploredSnapshot(context, payload.Snapshot);
            }
        }

        private void ReadOwnerSnapshots(ISaveContext context)
        {
            int count = context.Reader.ReadInt32();
            if (count <= 0 || _ownerSnapshotStore == null)
            {
                for (int index = 0; index < count; index++)
                    SkipOwnerSnapshot(context);
                return;
            }

            for (int index = 0; index < count; index++)
            {
                string ownerId = context.Reader.ReadString();
                int width = context.Reader.ReadInt32();
                int height = context.Reader.ReadInt32();
                if (width <= 0 || height <= 0)
                    continue;

                var snapshot = new bool[width, height];
                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                        snapshot[x, y] = context.Reader.ReadBoolean();

                _ownerSnapshotStore.LoadFromSnapshot(ownerId, snapshot);
            }
        }

        private static void SkipOwnerSnapshot(ISaveContext context)
        {
            context.Reader.ReadString();
            int width = context.Reader.ReadInt32();
            int height = context.Reader.ReadInt32();
            if (width <= 0 || height <= 0)
                return;
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    context.Reader.ReadBoolean();
        }

        private void ReadLegacyExploredSnapshot(ISaveContext context, int width)
        {
            int height = context.Reader.ReadInt32();
            if (width <= 0 || height <= 0)
                return;

            ReadExploredSnapshot(context, width, height);
        }

        private void ReadExploredSnapshot(ISaveContext context, int width, int height)
        {
            var snapshot = new bool[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    snapshot[x, y] = context.Reader.ReadBoolean();

            _fogSnapshotStore.LoadFromSnapshot(snapshot);
        }

        private static void WriteExploredSnapshot(ISaveContext context, bool[,] snapshot)
        {
            if (snapshot == null)
            {
                context.Writer.Write(0);
                context.Writer.Write(0);
                return;
            }

            int width = snapshot.GetLength(0);
            int height = snapshot.GetLength(1);
            context.Writer.Write(width);
            context.Writer.Write(height);
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    context.Writer.Write(snapshot[x, y]);
        }
    }
}
