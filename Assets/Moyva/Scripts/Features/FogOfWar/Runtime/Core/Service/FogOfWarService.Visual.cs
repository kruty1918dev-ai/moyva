using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogOfWarService
    {
        public IReadOnlyCollection<Vector2Int> GetLastDirtyTiles()
            => _visualDirtyBuffer.DirtyTiles;

        private void TrackVisualChange(Vector2Int tile, FogStateType oldState, int oldHeightKey)
        {
            FogStateType newState = GetFogState(tile);
            int newHeightKey = ResolveVisualHeightKey(tile);
            _visualDirtyBuffer.TrackChange(tile, oldState, newState, oldHeightKey, newHeightKey);
        }

        private int ResolveVisualHeightKey(Vector2Int tile)
        {
            if (!_visualContext.IsValid)
                return 0;

            if (!_hasVisualHeightSampler)
            {
                _visualHeightSampler = new FogVolumeHeightSampler(_visualContext, _settings);
                _hasVisualHeightSampler = true;
            }

            return _visualHeightSampler.ResolveHeightKey(tile);
        }

        private void ResetVisualHeightSampler()
        {
            _visualHeightSampler = default;
            _hasVisualHeightSampler = false;
        }

        private FogVisualFlushResult FlushVisual()
        {
            int dirtyCount = _visualDirtyBuffer.DirtyCount;
            int changeCount = _visualDirtyBuffer.ChangeCount;
            int versionBefore = Version;
            bool hadVisualUpdater = _visualUpdater != null;
            bool updaterCalled = false;
            string updaterType = _visualUpdater != null ? _visualUpdater.GetType().Name : "null";
            Debug.Log($"{DirectDiagTag} FogService.FlushVisual dirty={dirtyCount}, hasVisualUpdater={_visualUpdater != null}.");
            Debug.Log($"{StartDiagTag} FlushVisual dirty={dirtyCount}, changes={changeCount}, hasVisualUpdater={_visualUpdater != null}, updateType=dirty-update.");
            CountFogStateTiles(out int visibleBeforeFlush, out int exploredBeforeFlush, out int unexploredBeforeFlush);
            Debug.Log($"{StartupChainTag} Fog.FlushVisual ENTER dirty={dirtyCount}, changes={changeCount}, hasVisualUpdater={_visualUpdater != null}, stateVisible={visibleBeforeFlush}, stateExplored={exploredBeforeFlush}, stateUnexplored={unexploredBeforeFlush}, dirtySamples={FormatDirtyTileSamples()}.");
            if (dirtyCount > 0 && _visualUpdater == null)
                Debug.LogWarning($"{StartDiagTag} FlushVisual dirty tiles exist but visualUpdater is null dirty={dirtyCount}.");
            if (_visualUpdater != null)
            {
                if (changeCount > 0)
                    _visualUpdater.RequestCellsUpdate(this, _visualDirtyBuffer.Changes, _visualContext);
                else
                    _visualUpdater.UpdateDirtyTiles(this, _visualDirtyBuffer.DirtyTiles);

                updaterCalled = true;
                Debug.Log($"{StartDiagTag} FlushVisual updaterCalled=true dirty={dirtyCount}, changes={changeCount}.");
                Debug.Log($"{StartupChainTag} Fog.FlushVisual UPDATER_CALLED dirty={dirtyCount}, updater={_visualUpdater.GetType().Name}.");
            }

            if (dirtyCount > 0)
                BumpVersion();

            _visualDirtyBuffer.Clear();
            Debug.Log($"{StartupChainTag} Fog.FlushVisual EXIT dirtyAfterClear={_visualDirtyBuffer.DirtyCount}, version={Version}.");
            Debug.Log($"{StartupRevealDiagTag} FlushVisualResult dirty={dirtyCount}, changes={changeCount}, hadVisualUpdater={hadVisualUpdater}, updaterCalled={updaterCalled}, updater={updaterType}, visualUpdateDispatched={updaterCalled}, versionBefore={versionBefore}, versionAfter={Version}, stateVisible={visibleBeforeFlush}, stateExplored={exploredBeforeFlush}, stateUnexplored={unexploredBeforeFlush}.");
            return new FogVisualFlushResult(
                dirtyCount,
                changeCount,
                hadVisualUpdater,
                updaterCalled,
                updaterType,
                versionBefore,
                Version,
                visibleBeforeFlush,
                exploredBeforeFlush,
                unexploredBeforeFlush);
        }

        private string FormatDirtyTileSamples(int maxSamples = 8)
        {
            if (_visualDirtyBuffer.DirtyCount == 0)
                return "none";

            int count = 0;
            var samples = new System.Text.StringBuilder();
            foreach (var tile in _visualDirtyBuffer.DirtyTiles)
            {
                if (count > 0)
                    samples.Append(", ");

                samples.Append(tile).Append('=').Append(GetFogState(tile));
                count++;
                if (count >= maxSamples)
                    break;
            }

            if (_visualDirtyBuffer.DirtyCount > count)
                samples.Append(", ...");

            return samples.ToString();
        }

        private readonly struct FogVisualFlushResult
        {
            public FogVisualFlushResult(
                int dirtyCount,
                int changeCount,
                bool hadVisualUpdater,
                bool updaterCalled,
                string updaterType,
                int versionBefore,
                int versionAfter,
                int visible,
                int explored,
                int unexplored)
            {
                DirtyCount = dirtyCount;
                ChangeCount = changeCount;
                HadVisualUpdater = hadVisualUpdater;
                UpdaterCalled = updaterCalled;
                UpdaterType = updaterType;
                VersionBefore = versionBefore;
                VersionAfter = versionAfter;
                Visible = visible;
                Explored = explored;
                Unexplored = unexplored;
            }

            public int DirtyCount { get; }
            public int ChangeCount { get; }
            public bool HadVisualUpdater { get; }
            public bool UpdaterCalled { get; }
            public string UpdaterType { get; }
            public int VersionBefore { get; }
            public int VersionAfter { get; }
            public int Visible { get; }
            public int Explored { get; }
            public int Unexplored { get; }
            public bool VisualFogDispersalRequested => DirtyCount > 0 && UpdaterCalled;
        }
    }
}
