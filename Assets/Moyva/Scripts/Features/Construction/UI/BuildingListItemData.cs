using Kruty1918.Moyva.Construction.API;

namespace Kruty1918.Moyva.Construction.UI
{
    /// <summary>
    /// Immutable UI-layer data for a single building entry in the selection list.
    /// Created from <see cref="BuildingDefinition"/> by <see cref="ConstructionUIController"/>.
    /// </summary>
    public sealed class BuildingListItemData
    {
        /// <summary>Unique building identifier (matches BuildingDefinition.Id).</summary>
        public string Id { get; }

        /// <summary>Human-readable name shown on the selection button.</summary>
        public string DisplayName { get; }

        /// <summary>Category the building belongs to.</summary>
        public BuildingCategory Category { get; }

        public BuildingListItemData(string id, string displayName, BuildingCategory category)
        {
            Id = id;
            DisplayName = displayName;
            Category = category;
        }
    }
}
