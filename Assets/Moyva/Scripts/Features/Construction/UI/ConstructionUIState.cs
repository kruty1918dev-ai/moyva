using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.UI
{
    /// <summary>
    /// Незмінний (immutable) знімок поточного стану UI будівництва.
    /// Передається підкомпонентам UI щоб вони могли оновити відображення.
    /// </summary>
    public sealed class ConstructionUIState
    {
        /// <summary>Поточний стан сесії розміщення (Idle / Placing / Confirmed).</summary>
        public BuildingPlacementState PlacementState { get; }

        /// <summary>ID будівлі, вибраної гравцем, або null.</summary>
        public string SelectedBuildingId { get; }

        /// <summary>Стан preview на останньому тайлі (None / Valid / Blocked).</summary>
        public BuildingPreviewState LastPreviewState { get; }

        /// <summary>Позиція останнього preview-тайлу.</summary>
        public Vector2Int LastPreviewPosition { get; }

        /// <summary>Чи активний режим знесення.</summary>
        public bool IsDemolishMode { get; }

        /// <summary>Чи активний режим будівництва (будь-яка дія доступна).</summary>
        public bool IsConstructionModeActive { get; }

        /// <summary>True коли активна сесія розміщення (будівля вибрана, очікуємо тайл).</summary>
        public bool IsPlacing => PlacementState == BuildingPlacementState.Placing;

        /// <summary>True коли будівля вибрана.</summary>
        public bool HasSelection => !string.IsNullOrEmpty(SelectedBuildingId);

        public ConstructionUIState(
            BuildingPlacementState placementState,
            string selectedBuildingId,
            BuildingPreviewState lastPreviewState,
            Vector2Int lastPreviewPosition,
            bool isDemolishMode = false,
            bool isConstructionModeActive = false)
        {
            PlacementState = placementState;
            SelectedBuildingId = selectedBuildingId;
            LastPreviewState = lastPreviewState;
            LastPreviewPosition = lastPreviewPosition;
            IsDemolishMode = isDemolishMode;
            IsConstructionModeActive = isConstructionModeActive;
        }

        /// <summary>Стан за замовчуванням — Idle, без вибору.</summary>
        public static ConstructionUIState Default =>
            new ConstructionUIState(BuildingPlacementState.Idle, null, BuildingPreviewState.None, Vector2Int.zero);
    }
}
