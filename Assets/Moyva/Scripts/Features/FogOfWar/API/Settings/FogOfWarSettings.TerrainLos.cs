using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    public partial class FogOfWarSettings
    {
        /// <summary>
        /// Висотний крок, з яким LOS-алгоритм рахує перепади рельєфу.
        /// </summary>
        [TitleGroup("Terrain LOS")]
        [MinValue(0.01f)]
        public float ElevationStep = 0.15f;

        /// <summary>
        /// Бонус до дальності огляду за кожен крок висоти спостерігача.
        /// </summary>
        [TitleGroup("Terrain LOS")]
        [MinValue(0)]
        public int ObserverHeightBonusPerStep = 1;

        /// <summary>
        /// Бонус до огляду при погляді вниз по схилу за кожен крок перепаду.
        /// </summary>
        [TitleGroup("Terrain LOS")]
        [MinValue(0)]
        public int DownhillVisionBonusPerStep = 1;

        /// <summary>
        /// Штраф до огляду при погляді вгору за кожен крок перепаду.
        /// </summary>
        [TitleGroup("Terrain LOS")]
        [MinValue(0)]
        public int UphillVisionPenaltyPerStep = 1;

        /// <summary>
        /// Верхня межа сумарного бонусу від висоти спостерігача.
        /// </summary>
        [TitleGroup("Terrain LOS")]
        [MinValue(0)]
        public int MaxObserverHeightBonus = 4;

        /// <summary>
        /// Верхня межа сумарного бонусу до дальності огляду при спуску вниз по рельєфу.
        /// </summary>
        [TitleGroup("Terrain LOS")]
        [MinValue(0)]
        public int MaxDownhillVisionBonus = 2;

        /// <summary>
        /// Верхня межа сумарного штрафу при огляді вгору.
        /// </summary>
        [TitleGroup("Terrain LOS")]
        [MinValue(0)]
        public int MaxUphillVisionPenalty = 6;

        /// <summary>
        /// Додатковий bias для розв'язання оклюзії на схилах.
        /// </summary>
        [TitleGroup("Terrain LOS")]
        [MinValue(0f)]
        public float OcclusionSlopeBias = 0.02f;

        /// <summary>
        /// Кількість ray samples на одну клітинку при LOS-розрахунку.
        /// </summary>
        [TitleGroup("Terrain LOS/Raycast")]
        [Range(1, 9)]
        public int TerrainRaySamplesPerTile = 5;

        /// <summary>
        /// Поріг, з якого часткова видимість уже вважається реальною видимістю.
        /// </summary>
        [TitleGroup("Terrain LOS/Raycast")]
        [Range(0.01f, 1f)]
        public float TerrainVisibilityThreshold = 0.5f;

        /// <summary>
        /// Множник для partial visibility detection у складних LOS-сценаріях.
        /// </summary>
        [TitleGroup("Terrain LOS/Raycast")]
        [Range(0f, 1f)]
        public float PartialVisibilityDetectionMultiplier = 1f;

        /// <summary>
        /// Крок променя в клітинках під час terrain LOS raycast.
        /// </summary>
        [TitleGroup("Terrain LOS/Raycast")]
        [Range(0.25f, 1f)]
        public float TerrainRayStepTiles = 0.5f;

        /// <summary>
        /// Зсув точки спостерігача над поверхнею тайла.
        /// </summary>
        [TitleGroup("Terrain LOS/Raycast")]
        [MinValue(0f)]
        public float ObserverEyeHeightOffset = 0.35f;

        /// <summary>
        /// Зсув точки семплу цілі над поверхнею тайла.
        /// </summary>
        [TitleGroup("Terrain LOS/Raycast")]
        [MinValue(0f)]
        public float TargetSampleHeightOffset = 0.1f;

        /// <summary>
        /// Співвідношення дистанції для дальніх семплів у terrain LOS.
        /// </summary>
        [TitleGroup("Terrain LOS/Raycast")]
        [Range(0.1f, 1f)]
        public float TerrainFarSampleDistanceRatio = 0.65f;

        /// <summary>
        /// Місткість кешу visibility calculations для terrain LOS.
        /// </summary>
        [TitleGroup("Terrain LOS/Raycast")]
        [MinValue(0)]
        public int TerrainVisibilityCacheCapacity = 24576;

        /// <summary>
        /// Чи дозволений окремий edge-based LOS logic для terrain transitions.
        /// </summary>
        [TitleGroup("Terrain LOS/Edges")]
        public bool EnableTerrainEdgeLineOfSight = true;

        /// <summary>
        /// Мінімальний перепад висоти, який уже вважається edge/cliff для LOS logic.
        /// </summary>
        [TitleGroup("Terrain LOS/Edges")]
        [MinValue(0.001f)]
        public float TerrainEdgeHeightThreshold = 0.12f;

        /// <summary>
        /// Скільки клітин edge-peek логіка може “зазирнути” за край.
        /// </summary>
        [TitleGroup("Terrain LOS/Edges")]
        [MinValue(0)]
        public int TerrainEdgePeekDistanceTiles = 1;

        /// <summary>
        /// Розмір blind zone за edge/cliff переходом у клітинках.
        /// </summary>
        [TitleGroup("Terrain LOS/Edges")]
        [MinValue(0)]
        public int TerrainEdgeBlindZoneTiles = 2;

        /// <summary>
        /// Масштаб distance-based blind zone для terrain edges.
        /// </summary>
        [TitleGroup("Terrain LOS/Edges")]
        [MinValue(0f)]
        public float TerrainEdgeBlindZoneDistanceScale = 0.35f;

        /// <summary>
        /// Максимальна довжина blind zone для terrain edges.
        /// </summary>
        [TitleGroup("Terrain LOS/Edges")]
        [MinValue(0)]
        public int TerrainEdgeMaxBlindZoneTiles = 4;

        /// <summary>
        /// Сила часткового peek-ефекту при погляді вгору через край.
        /// </summary>
        [TitleGroup("Terrain LOS/Edges")]
        [Range(0f, 1f)]
        public float TerrainEdgeUphillPeekStrength = 0.65f;
    }
}
