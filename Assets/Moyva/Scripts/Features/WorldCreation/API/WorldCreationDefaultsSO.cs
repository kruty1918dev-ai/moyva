using UnityEngine;

namespace Kruty1918.Moyva.WorldCreation.API
{
    /// <summary>
    /// ScriptableObject із типовими значеннями для екрану створення світу.
    /// Дозволяє дизайнерам налаштовувати дефолти без правки коду.
    ///
    /// Створення: Assets → Create → Moyva → WorldCreation → Defaults
    /// </summary>
    [CreateAssetMenu(menuName = "Moyva/WorldCreation/Defaults", fileName = "WorldCreationDefaults")]
    public sealed class WorldCreationDefaultsSO : ScriptableObject
    {
        [Header("Основні параметри")]
        [Tooltip("Назва нового світу за замовчуванням.")]
        public string DefaultWorldName = "Новий світ";

        [Tooltip("Розмір карти за замовчуванням.")]
        public WorldSizePreset DefaultSizePreset = WorldSizePreset.Medium;

        [Tooltip("Ширина (тайли) коли вибрано Custom. Мін: 16.")]
        [Min(16)] public int DefaultCustomWidth = 64;

        [Tooltip("Висота (тайли) коли вибрано Custom. Мін: 16.")]
        [Min(16)] public int DefaultCustomHeight = 64;

        [Tooltip("Тип карти за замовчуванням.")]
        public MapTypePreset DefaultMapType = MapTypePreset.Balanced;

        [Header("Правила гри")]
        [Tooltip("Складність за замовчуванням.")]
        public DifficultyLevel DefaultDifficulty = DifficultyLevel.Normal;

        [Tooltip("Чи увімкнені боти за замовчуванням.")]
        public bool DefaultEnableBots = true;

        [Tooltip("Кількість людських гравців. Мін: 1, Макс: 4.")]
        [Range(1, 4)] public int DefaultHumanPlayerCount = 1;

        [Tooltip("Кількість ботів за замовчуванням. Мін: 0, Макс: 4.")]
        [Range(0, 4)] public int DefaultBotCount = 1;

        [Tooltip("Стартове золото на фракцію.")]
        [Min(0)] public int DefaultStartingGold = 200;

        [Tooltip("Стартова їжа на фракцію.")]
        [Min(0)] public int DefaultStartingFood = 100;

        [Header("Параметри генерації")]
        [Tooltip("Щільність лісів [0..1].")]
        [Range(0f, 1f)] public float DefaultForestDensity = 0.4f;

        [Tooltip("Щільність гір [0..1].")]
        [Range(0f, 1f)] public float DefaultMountainDensity = 0.3f;

        [Tooltip("Щільність водних зон [0..1].")]
        [Range(0f, 1f)] public float DefaultWaterDensity = 0.25f;

        [Tooltip("Щільність POI (сіл, таборів) [0..1].")]
        [Range(0f, 1f)] public float DefaultVillageDensity = 0.2f;

        [Tooltip("Генерувати річки за замовчуванням.")]
        public bool DefaultGenerateRivers = true;

        [Tooltip("Генерувати біоми за замовчуванням.")]
        public bool DefaultGenerateBiomes = true;

        [Tooltip("Застосовувати WFC-полірування тайлів за замовчуванням.")]
        public bool DefaultApplyWFC = true;

        /// <summary>
        /// Створює <see cref="WorldCreationConfig"/> з усіма значеннями з цього SO.
        /// </summary>
        public WorldCreationConfig ToConfig()
        {
            return new WorldCreationConfig
            {
                WorldName        = DefaultWorldName,
                Seed             = 0,
                SizePreset       = DefaultSizePreset,
                CustomWidth      = DefaultCustomWidth,
                CustomHeight     = DefaultCustomHeight,
                MapType          = DefaultMapType,
                Difficulty       = DefaultDifficulty,
                EnableBots       = DefaultEnableBots,
                HumanPlayerCount = DefaultHumanPlayerCount,
                BotCount         = DefaultBotCount,
                StartingGold     = DefaultStartingGold,
                StartingFood     = DefaultStartingFood,
                ForestDensity    = DefaultForestDensity,
                MountainDensity  = DefaultMountainDensity,
                WaterDensity     = DefaultWaterDensity,
                VillageDensity   = DefaultVillageDensity,
                GenerateRivers   = DefaultGenerateRivers,
                GenerateBiomes   = DefaultGenerateBiomes,
                ApplyWFC         = DefaultApplyWFC
            };
        }
    }
}
