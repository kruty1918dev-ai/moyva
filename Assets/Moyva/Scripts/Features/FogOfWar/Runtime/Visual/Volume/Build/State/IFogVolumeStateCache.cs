using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal interface IFogVolumeStateCache
    {
        int UnexploredCellCount { get; }
        int ExploredCellCount { get; }
        string RuntimeLayerSignature { get; }
        bool RuntimeLayerSignatureChanged { get; }
        IReadOnlyCollection<Vector2> UnexploredCells { get; }
        IReadOnlyCollection<Vector2> ExploredCells { get; }
        IReadOnlyDictionary<int, HashSet<Vector2>> UnexploredCellsByHeight { get; }
        IReadOnlyDictionary<int, HashSet<Vector2>> ExploredCellsByHeight { get; }

        void InitializeMapSize(int width, int height);
        void Rebuild(IFogOfWarService fogService, Func<Vector2Int, int> resolveHeightKey, bool unexploredEnabled, bool exploredEnabled);
        void ApplyDirty(IFogOfWarService fogService, IEnumerable<Vector2Int> dirtyTiles, Func<Vector2Int, int> resolveHeightKey, bool unexploredEnabled, bool exploredEnabled);
        void Clear();
        bool HasUnexploredCell(Vector2Int tile);
        bool HasExploredCell(Vector2Int tile);
        HashSet<Vector2> ResolveUnexploredCells(int heightKey);
        HashSet<Vector2> ResolveExploredCells(int heightKey);
        int CountNonEmptyHeightLayers(IReadOnlyDictionary<int, HashSet<Vector2>> cellsByHeight);
    }
}
