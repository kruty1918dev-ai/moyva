using System;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    [Serializable]
    public sealed class MenuPreviewKingdomPlacementSettings
    {
        [Tooltip("Увімкнути post-process розміщення королівств поверх згенерованої мапи прев'ю.")]
        public bool Enabled = true;

        [Header("Зони королівств (тайли)")]
        [Tooltip("Прямокутна зона пошуку замку Kingdom A в координатах тайлів.")]
        public RectInt KingdomAZone = new RectInt(8, 8, 72, 40);

        [Tooltip("Прямокутна зона пошуку замку Kingdom B в координатах тайлів.")]
        public RectInt KingdomBZone = new RectInt(96, 32, 72, 40);

        [Header("Дистанції та кількості")]
        [Tooltip("Мінімальна мангеттен-відстань між двома замками.")]
        [Min(1)] public int CastleMinDistance = 48;

        [Tooltip("Радіус (тайли) локальних будівель навколо замку.")]
        [Min(1)] public int KingdomSettlementRadius = 12;

        [Tooltip("Кількість складів біля кожного замку.")]
        [Min(0)] public int WarehousesPerKingdom = 2;

        [Tooltip("Кількість додаткових локальних поселень біля кожного замку.")]
        [Min(0)] public int KingdomLocalSettlementCount = 2;

        [Tooltip("Кількість малих поселень (з ратушею) по мапі.")]
        [Min(0)] public int SmallTownCount = 6;

        [Tooltip("Мінімальна мангеттен-відстань між будь-якими новими поселеннями.")]
        [Min(1)] public int MinSettlementDistance = 10;

        [Header("Фільтри по рельєфу")]
        [Tooltip("Мінімальна дозволена висота для розміщення будівлі.")]
        public float MinHeight = 0.2f;

        [Tooltip("Максимальна дозволена висота для розміщення будівлі.")]
        public float MaxHeight = 0.85f;

        [Tooltip("Чорний список biome tile id, на яких не можна ставити будівлі (наприклад water, sea, coast).")]
        public string[] ForbiddenBiomeTileIds = { "water", "sea", "coast", "river" };

        [Tooltip("Якщо увімкнено, порівнює базовий tile id до роздільника, напр. grass для grass-cliff-N.")]
        public bool MatchBaseTileType = true;

        [Tooltip("Роздільник для базового tile id.")]
        public char TileSeparator = '-';

        [Header("Building IDs")]
        [Tooltip("ID будівлі-замку.")]
        public string CastleBuildingId = "castle";

        [Tooltip("ID будівлі-ратуші для малих поселень.")]
        public string TownHallBuildingId = "townhall";

        [Tooltip("ID складу для локального оточення замку.")]
        public string WarehouseBuildingId = "warehouse";

        [Tooltip("ID додаткової локальної будівлі біля замку (поселення/житло тощо).")]
        public string LocalSettlementBuildingId = "village";

        [Tooltip("Макс. кількість спроб пошуку позиції для одного об'єкта.")]
        [Min(8)] public int MaxAttemptsPerPlacement = 128;

        public void ClampAndNormalize()
        {
            KingdomAZone.width = Mathf.Max(1, KingdomAZone.width);
            KingdomAZone.height = Mathf.Max(1, KingdomAZone.height);
            KingdomBZone.width = Mathf.Max(1, KingdomBZone.width);
            KingdomBZone.height = Mathf.Max(1, KingdomBZone.height);

            CastleMinDistance = Mathf.Max(1, CastleMinDistance);
            KingdomSettlementRadius = Mathf.Max(1, KingdomSettlementRadius);
            WarehousesPerKingdom = Mathf.Max(0, WarehousesPerKingdom);
            KingdomLocalSettlementCount = Mathf.Max(0, KingdomLocalSettlementCount);
            SmallTownCount = Mathf.Max(0, SmallTownCount);
            MinSettlementDistance = Mathf.Max(1, MinSettlementDistance);
            MaxAttemptsPerPlacement = Mathf.Max(8, MaxAttemptsPerPlacement);

            if (MinHeight > MaxHeight)
            {
                float temp = MinHeight;
                MinHeight = MaxHeight;
                MaxHeight = temp;
            }
        }
    }
}