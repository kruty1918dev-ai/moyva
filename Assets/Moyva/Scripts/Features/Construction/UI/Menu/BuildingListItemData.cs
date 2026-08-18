using Kruty1918.Moyva.Construction.API;
using UnityEngine;

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

        /// <summary>Icon sprite shown on the building button. May be null.</summary>
        public Sprite Icon { get; }

        /// <summary>Editor-baked prefab preview shown on the toolbar. Falls back to Icon when null.</summary>
        public Sprite PreviewSprite { get; }

        /// <summary>True when this item is currently allowed to be clicked in the build menu.</summary>
        public bool IsInteractable { get; }

        /// <summary>Concrete global availability reason shown for a disabled item.</summary>
        public string UnavailableReason { get; }

        public BuildingListItemData(
            string id,
            string displayName,
            BuildingCategory category,
            Sprite icon = null,
            bool isInteractable = true,
            string unavailableReason = null)
            : this(
                id,
                displayName,
                category,
                icon,
                null,
                isInteractable,
                unavailableReason)
        {
        }

        public BuildingListItemData(
            string id,
            string displayName,
            BuildingCategory category,
            Sprite icon,
            Sprite previewSprite,
            bool isInteractable,
            string unavailableReason = null)
        {
            Id = id;
            // Id is internal identity only; never expose it as a UI label.
            DisplayName = string.IsNullOrWhiteSpace(displayName)
                ? "Будівля"
                : displayName.Trim();
            Category = category;
            Icon = icon;
            PreviewSprite = previewSprite;
            IsInteractable = isInteractable;
            UnavailableReason = isInteractable
                || string.IsNullOrWhiteSpace(unavailableReason)
                ? null
                : unavailableReason.Trim();
        }
    }
}
