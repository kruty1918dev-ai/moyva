using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Kruty1918.Moyva.Construction.API
{
    [Serializable]
    public sealed class BuildingRuntimeStats
    {
        [MinValue(1)]
        public int MaxHp = 100;

        [MinValue(0)]
        public int Armor;

        public BuildingRuntimeFlags Flags = BuildingRuntimeFlags.BlocksPath | BuildingRuntimeFlags.Selectable | BuildingRuntimeFlags.Damageable;

        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = false)]
        public List<string> RuntimeTags = new List<string>();
    }
}
