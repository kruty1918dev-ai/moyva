using System.Collections.Generic;
using Kruty1918.SaveSystem;

namespace Kruty1918.Moyva.SaveSystem
{
    /// <summary>
    /// Moyva gameplay save-module order. These stable identities are the persisted
    /// block keys of existing .mvs files — the strings and order values are part of
    /// the save contract and must not change.
    /// </summary>
    internal static class MoyvaSaveModuleOrdering
    {
        internal static SaveModuleOrdering Create()
            => new SaveModuleOrdering(new Dictionary<string, int>
            {
                ["Kruty1918.Moyva.Generator.Runtime.GeneratedWorldSaveModule"] = 100,
                ["Kruty1918.Moyva.Construction.Runtime.ConstructionSaveModule"] = 200,
                ["Kruty1918.Moyva.Economy.Runtime.EconomySaveModule"] = 300,
                ["Kruty1918.Moyva.Bootstrap.Runtime.BootstrapStarterPackSaveModule"] = 350,
                ["Kruty1918.Moyva.Bootstrap.Runtime.UnitsSaveModule"] = 400,
                ["Kruty1918.Moyva.FogOfWar.Runtime.FogOfWarSaveModule"] = 500,
                ["Kruty1918.Moyva.Bootstrap.Runtime.TurnSaveModule"] = 900,
            });
    }
}
