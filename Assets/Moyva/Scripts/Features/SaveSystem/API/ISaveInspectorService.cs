using System;

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
    }
}