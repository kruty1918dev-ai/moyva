using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public enum BuildingRole
    {
        None = 0,
        Housing = 1,
        Production = 2,
        Storage = 3,
        SettlementCenter = 4,
        Defense = 5,
        Wall = 6,
        Gate = 7,
        Decoration = 8,
        Support = 9,
    }

    [Serializable]
    public sealed class BuildingIdentity
    {
        [Required]
        [ValidateInput(nameof(HasValidId), "ID is required.")]
        public string Id = "new-building";

        [Required]
        public string DisplayName = "New Building";

        public BuildingCategory Category = BuildingCategory.Civilian;
        public BuildingRole Role = BuildingRole.Support;

        [TextArea(2, 5)]
        public string Description;

        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = false)]
        public List<string> Tags = new List<string>();

        private bool HasValidId(string value) => !string.IsNullOrWhiteSpace(value);
    }
}
