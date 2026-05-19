using System;
using UnityEngine;

namespace Kruty1918.Moyva.SaveSystem
{
    /// <summary>
    /// Read-only helper for inspecting slot contents without fully loading game state.
    /// Useful for bootstrap decisions like "load save" vs "start new game".
    /// </summary>
    public interface ISaveInspectorService
    {
        bool HasBlock(int slot, Type moduleType);
        bool HasBlock<TModule>(int slot = 0);
        bool HasBlock(int slot, string moduleTypeFullName);
        bool TryGetBlockPayload(int slot, string moduleTypeFullName, out byte[] payload);
        /// <summary>
        /// Try to read FogOfWar snapshot from slot without loading the whole game.
        /// Returns true and sets <paramref name="snapshot"/> when the fog block exists and could be parsed.
        /// </summary>
        bool TryGetFogSnapshot(int slot, out bool[,] snapshot);
    }
}