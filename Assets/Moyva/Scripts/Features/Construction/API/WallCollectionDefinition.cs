using UnityEngine;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Construction.API
{
    /// <summary>
    /// Колекція варіантів стін — набір з 6 сегментів стіни та воріт для одного архітектурного стилю.
    /// Потрібний варіант обирається автоматично залежно від наявних сусідів тайла.
    /// </summary>
    [System.Serializable]
    public class WallCollectionDefinition
    {
        [Header("— Ідентифікатори —")]
        [Tooltip("Унікальний ідентифікатор колекції стін у реєстрі. " +
                 "Використовується для розрізнення між різними стилями стін. " +
                 "Приклади: 'stone-wall', 'wooden-wall', 'castle-wall'.")]
        public string CollectionId = "default-wall";

        [Tooltip("ID будівлі-стіни у BuildingRegistrySO. " +
                 "Це значення присвоюється тайлу при розміщенні будь-якого сегмента стіни з цієї колекції. " +
                 "Має збігатися з полем Id відповідного BuildingDefinition.")]
        public string WallBuildingId = "wall";

        [Tooltip("ID будівлі-воріт у BuildingRegistrySO. " +
                 "Ворота можна встановити лише на тайл, де вже є стіна цієї колекції — вони замінюють її. " +
                 "Має збігатися з полем Id відповідного BuildingDefinition.")]
        public string GateBuildingId = "gate";

        [Header("— Варіанти стін (6 штук) —")]
        [Tooltip("ГОРИЗОНТАЛЬНА СТІНА (←→)\n\n" +
                 "Прямий сегмент стіни вздовж осі X (ліворуч–праворуч).\n\n" +
                 "Коли використовується:\n" +
                 "• Є сусіди лише ліворуч і/або праворуч (E, W або E+W)\n" +
                 "• Немає жодних сусідів (одиночний сегмент — fallback)\n" +
                 "• Є 3+ сусіди з обох горизонтальних боків (T-подібний та хрест — fallback)")]
        public GameObject HorizontalPrefab;

        [Tooltip("ВЕРТИКАЛЬНА СТІНА (↑↓)\n\n" +
                 "Прямий сегмент стіни вздовж осі Y (знизу вгору).\n\n" +
                 "Коли використовується:\n" +
                 "• Є сусіди лише зверху і/або знизу (N, S або N+S)\n" +
                 "• При T-подібному з'єднанні де є N+S вісь (N+S+E або N+S+W — fallback)")]
        public GameObject VerticalPrefab;

        [Tooltip("КУТ 'ПРАВИЙ ВЕРХНІЙ' — NE (↑→)\n\n" +
                 "Кутовий сегмент що з'єднує напрямки Північ і Схід.\n" +
                 "Стіна згинається: іде вгору і праворуч.\n\n" +
                 "Коли використовується:\n" +
                 "• Є сусід зверху (N) і праворуч (E), але НЕ знизу і НЕ ліворуч")]
        public GameObject CornerNorthEastPrefab;

        [Tooltip("КУТ 'ЛІВИЙ ВЕРХНІЙ' — NW (↑←)\n\n" +
                 "Кутовий сегмент що з'єднує напрямки Північ і Захід.\n" +
                 "Стіна згинається: іде вгору і ліворуч.\n\n" +
                 "Коли використовується:\n" +
                 "• Є сусід зверху (N) і ліворуч (W), але НЕ знизу і НЕ праворуч")]
        public GameObject CornerNorthWestPrefab;

        [Tooltip("КУТ 'ПРАВИЙ НИЖНІЙ' — SE (↓→)\n\n" +
                 "Кутовий сегмент що з'єднує напрямки Південь і Схід.\n" +
                 "Стіна згинається: іде вниз і праворуч.\n\n" +
                 "Коли використовується:\n" +
                 "• Є сусід знизу (S) і праворуч (E), але НЕ зверху і НЕ ліворуч")]
        public GameObject CornerSouthEastPrefab;

        [Tooltip("КУТ 'ЛІВИЙ НИЖНІЙ' — SW (↓←)\n\n" +
                 "Кутовий сегмент що з'єднує напрямки Південь і Захід.\n" +
                 "Стіна згинається: іде вниз і ліворуч.\n\n" +
                 "Коли використовується:\n" +
                 "• Є сусід знизу (S) і ліворуч (W), але НЕ зверху і НЕ праворуч")]
        public GameObject CornerSouthWestPrefab;

        [Header("— Ворота —")]
        [Tooltip("ВОРОТА\n\n" +
                 "Прохідний сегмент, що замінює стіну цієї ж колекції.\n\n" +
                 "Особливості:\n" +
                 "• Можна встановити ЛИШЕ на тайл де вже є стіна (WallBuildingId) цієї колекції\n" +
                 "• При встановленні стіна знищується і замінюється воротами\n" +
                 "• GateBuildingId має бути зареєстровано у BuildingRegistrySO")]
        public GameObject GatePrefab;

        [Header("— Декларативні правила резолвера —")]
        [Tooltip("Опціональний список case -> варіації BuildingId. " +
             "Якщо елементів немає, система використовує legacy fallback з prefab-полів вище.")]
        public List<TopologyCaseBinding> TopologyBindings = new();

        /// <summary>Чи містить колекція цей buildingId (стіна або ворота).</summary>
        public bool ContainsBuilding(string buildingId)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
                return false;

            if (buildingId == WallBuildingId || buildingId == GateBuildingId)
                return true;

            if (TopologyBindings == null)
                return false;

            for (int i = 0; i < TopologyBindings.Count; i++)
            {
                var binding = TopologyBindings[i];
                if (binding == null || binding.VariantBuildingIds == null)
                    continue;

                for (int j = 0; j < binding.VariantBuildingIds.Count; j++)
                {
                    if (binding.VariantBuildingIds[j] == buildingId)
                        return true;
                }
            }

            return false;
        }

        /// <summary>Чи є цей buildingId стіною з цієї колекції.</summary>
        public bool IsWall(string buildingId)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
                return false;

            if (buildingId == WallBuildingId)
                return true;

            if (buildingId == GateBuildingId || TopologyBindings == null)
                return false;

            for (int i = 0; i < TopologyBindings.Count; i++)
            {
                var binding = TopologyBindings[i];
                if (binding == null || binding.VariantBuildingIds == null)
                    continue;

                for (int j = 0; j < binding.VariantBuildingIds.Count; j++)
                {
                    if (binding.VariantBuildingIds[j] == buildingId)
                        return true;
                }
            }

            return false;
        }

        /// <summary>Чи є цей buildingId воротами з цієї колекції.</summary>
        public bool IsGate(string buildingId) =>
            buildingId == GateBuildingId;
    }
}
