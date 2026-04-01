using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    /// <summary>
    /// Керує режимом будівництва: вибір типу будівлі, попередній перегляд,
    /// розміщення (pending), підтвердження або скасування, undo/redo.
    /// </summary>
    public interface IBuildingConstructionService
    {
        bool IsInConstructionMode { get; }
        string SelectedBuildingTypeId { get; }
        IReadOnlyList<PendingBuilding> PendingBuildings { get; }

        /// <summary>Починає режим розміщення для вказаного типу будівлі.</summary>
        void StartPlacement(string buildingTypeId);

        /// <summary>Оновлює попередній перегляд на вказаній позиції.</summary>
        void PreviewAt(Vector2Int position);

        /// <summary>Розміщує будівлю на позиції (pending — ще не підтверджено).</summary>
        void PlaceAt(Vector2Int position);

        /// <summary>Скасовує всі pending-будівлі та виходить з режиму будівництва.</summary>
        void CancelAll();

        /// <summary>Підтверджує всі pending-будівлі та реєструє їх в ObjectsMap.</summary>
        void ConfirmAll();

        /// <summary>Скасовує останнє розміщення (Ctrl+Z).</summary>
        void Undo();

        /// <summary>Повторює скасоване розміщення (Ctrl+Y).</summary>
        void Redo();
    }
}
