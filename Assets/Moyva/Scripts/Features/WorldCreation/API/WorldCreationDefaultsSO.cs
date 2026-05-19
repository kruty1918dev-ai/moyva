using UnityEngine;
using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Grid.API;

namespace Kruty1918.Moyva.WorldCreation.API
{
    [System.Serializable]
    public sealed class TerrainLevelRestrictionRange
    {
        [Min(1)] public int MinLevel = 1;
        [Min(1)] public int MaxLevel = 1;
    }

    [System.Serializable]
    public sealed class WorldSizePresetDefinition
    {
        [Min(16)] public int Width = 64;
        [Min(16)] public int Height = 64;

        public WorldSizePresetDefinition() { }

        public WorldSizePresetDefinition(int width, int height)
        {
            Width = Mathf.Max(16, width);
            Height = Mathf.Max(16, height);
        }
    }

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
        public string DefaultWorldName = "Мій світ";

        [Tooltip("Якщо увімкнено, меню створення світу додасть індекс до стартової назви, коли існують попередні збереження.")]
        public bool AppendIndexWhenSaveExists = true;

        [Tooltip("Індекс першого світу, який додається до назви, якщо потрібно уникнути дублювання.")]
        [Min(1)] public int FirstWorldIndex = 1;

        [Tooltip("Розмір карти за замовчуванням.")]
        public WorldSizePreset DefaultSizePreset = WorldSizePreset.Medium;

        [Tooltip("Ширина (тайли) коли вибрано Custom. Мін: 16.")]
        [Min(16)] public int DefaultCustomWidth = 64;

        [Tooltip("Висота (тайли) коли вибрано Custom. Мін: 16.")]
        [Min(16)] public int DefaultCustomHeight = 64;

        [Header("Пресети розміру")]
        public WorldSizePresetDefinition SmallWorld = new(32, 32);
        public WorldSizePresetDefinition MediumWorld = new(64, 64);
        public WorldSizePresetDefinition LargeWorld = new(128, 128);

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

        [Header("Обмеження розміщення (через редактор світу)")]
        [Tooltip("Граф генерації (GraphAsset), який використовується для читання TileRegistry та параметрів HillGenerator у World Defaults Editor.")]
        public GraphAsset PlacementRulesGraph;

        [Tooltip("Реєстр тайлів для обмежень розміщення. Якщо задано — перекриває TileRegistry з GraphAsset.")]
        public TileRegistrySO TileRegistry;

        [Tooltip("Tile ID, на яких не можна будувати.")]
        public List<string> BlockedBuildingTileIds = new();

        [Tooltip("Діапазони рівнів HillGenerator, на яких не можна будувати (включно).")]
        public List<TerrainLevelRestrictionRange> BlockedBuildingHillLevelRanges = new();

        [Tooltip("Tile ID, на яких не можна розміщувати/рухати юнітів.")]
        public List<string> BlockedUnitTileIds = new();

        [Tooltip("Діапазони рівнів HillGenerator, на яких не можна розміщувати/рухати юнітів (включно).")]
        public List<TerrainLevelRestrictionRange> BlockedUnitHillLevelRanges = new();

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
                SmallWidth       = ResolveWidth(WorldSizePreset.Small),
                SmallHeight      = ResolveHeight(WorldSizePreset.Small),
                MediumWidth      = ResolveWidth(WorldSizePreset.Medium),
                MediumHeight     = ResolveHeight(WorldSizePreset.Medium),
                LargeWidth       = ResolveWidth(WorldSizePreset.Large),
                LargeHeight      = ResolveHeight(WorldSizePreset.Large),
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

        public int ResolveWidth(WorldSizePreset preset, int customWidth = 0)
        {
            return Mathf.Max(16, preset switch
            {
                WorldSizePreset.Small => SmallWorld?.Width ?? 32,
                WorldSizePreset.Medium => MediumWorld?.Width ?? 64,
                WorldSizePreset.Large => LargeWorld?.Width ?? 128,
                WorldSizePreset.Custom => customWidth > 0 ? customWidth : DefaultCustomWidth,
                _ => MediumWorld?.Width ?? 64,
            });
        }

        public int ResolveHeight(WorldSizePreset preset, int customHeight = 0)
        {
            return Mathf.Max(16, preset switch
            {
                WorldSizePreset.Small => SmallWorld?.Height ?? 32,
                WorldSizePreset.Medium => MediumWorld?.Height ?? 64,
                WorldSizePreset.Large => LargeWorld?.Height ?? 128,
                WorldSizePreset.Custom => customHeight > 0 ? customHeight : DefaultCustomHeight,
                _ => MediumWorld?.Height ?? 64,
            });
        }

        public string BuildIndexedWorldName(int existingSaveCount)
        {
            string baseName = string.IsNullOrWhiteSpace(DefaultWorldName)
                ? "Мій світ"
                : DefaultWorldName.Trim();

            if (!AppendIndexWhenSaveExists || existingSaveCount <= 0)
                return baseName;

            return $"{baseName} {Mathf.Max(1, FirstWorldIndex + existingSaveCount)}";
        }
    }
}
