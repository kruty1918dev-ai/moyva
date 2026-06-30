using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Kruty1918.Moyva.Construction.API
{
    [Serializable]
    public sealed class BuildingConstructionData
    {
        [TableList(AlwaysExpanded = false)]
        public List<BuildingDefinition.BuildingConstructionCostEntry> Cost = new List<BuildingDefinition.BuildingConstructionCostEntry>();

        [MinValue(0)]
        public int BuildTurns = 1;

        public bool RequiresBuilder = true;

        [MinValue(0)]
        public int WorkRequired;
    }
}
