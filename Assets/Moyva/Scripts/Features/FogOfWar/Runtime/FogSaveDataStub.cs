using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed class FogSaveDataStub : IFogSaveDataProvider
    {
        public bool[,] LoadExploredData()
        {
            Debug.Log("[FogOfWar][STUB] LoadExploredData() -> null (new game).");
            return null;
        }

        public void SaveExploredData(bool[,] explored)
        {
            Debug.Log("[FogOfWar][STUB] SaveExploredData() not implemented. See docs/systems/fog-of-war/save-system-stub.md");
        }
    }
}
