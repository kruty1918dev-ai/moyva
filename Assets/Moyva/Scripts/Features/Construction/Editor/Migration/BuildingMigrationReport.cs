using System.Collections.Generic;

namespace Kruty1918.Moyva.Construction.Editor
{
    public sealed class BuildingMigrationReport
    {
        public int LegacyDefinitions;
        public int CreatedAssets;
        public int ReusedAssets;
        public int AddedFogRevealModules;
        public readonly List<string> Messages = new List<string>();

        public override string ToString()
        {
            return $"legacy={LegacyDefinitions}, created={CreatedAssets}, reused={ReusedAssets}, fogModules={AddedFogRevealModules}";
        }
    }
}
