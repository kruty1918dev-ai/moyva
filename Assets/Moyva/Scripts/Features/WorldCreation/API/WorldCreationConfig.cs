namespace Kruty1918.Moyva.WorldCreation.API
{
    /// <summary>
    /// Повна конфігурація сесії створення нового світу.
    /// Заповнюється UI, зберігається у <see cref="IWorldCreationService"/>
    /// і передається у <see cref="Kruty1918.Moyva.Signals.WorldCreationConfirmedSignal"/>.
    /// </summary>
    public sealed class WorldCreationConfig
    {
        // ── Основні параметри ────────────────────────────────────────────

        /// <summary>Назва світу. Використовується в збереженнях та заголовках.</summary>
        public string WorldName { get; set; } = "Новий світ";

        /// <summary>
        /// Seed генератора.
        /// 0 означає «не задано» — сервіс підставить випадкове значення перед підтвердженням.
        /// </summary>
        public int Seed { get; set; } = 0;

        /// <summary>Пресет розміру карти.</summary>
        public WorldSizePreset SizePreset { get; set; } = WorldSizePreset.Medium;

        /// <summary>Ширина карти в тайлах; актуально лише якщо <see cref="SizePreset"/> == Custom.</summary>
        public int CustomWidth { get; set; } = 64;

        /// <summary>Висота карти в тайлах; актуально лише якщо <see cref="SizePreset"/> == Custom.</summary>
        public int CustomHeight { get; set; } = 64;

        /// <summary>Ширина маленького світу в тайлах.</summary>
        public int SmallWidth { get; set; } = 32;

        /// <summary>Висота маленького світу в тайлах.</summary>
        public int SmallHeight { get; set; } = 32;

        /// <summary>Ширина середнього світу в тайлах.</summary>
        public int MediumWidth { get; set; } = 64;

        /// <summary>Висота середнього світу в тайлах.</summary>
        public int MediumHeight { get; set; } = 64;

        /// <summary>Ширина великого світу в тайлах.</summary>
        public int LargeWidth { get; set; } = 128;

        /// <summary>Висота великого світу в тайлах.</summary>
        public int LargeHeight { get; set; } = 128;

        /// <summary>Тип/шаблон карти.</summary>
        public MapTypePreset MapType { get; set; } = MapTypePreset.Balanced;

        // ── Правила гри ──────────────────────────────────────────────────

        /// <summary>Рівень складності.</summary>
        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Normal;

        /// <summary>Чи увімкнені бот-гравці.</summary>
        public bool EnableBots { get; set; } = true;

        /// <summary>Кількість людських гравців (1–4).</summary>
        public int HumanPlayerCount { get; set; } = 1;

        /// <summary>Кількість бот-гравців (0–4).</summary>
        public int BotCount { get; set; } = 1;

        /// <summary>Стартова кількість золота на фракцію.</summary>
        public int StartingGold { get; set; } = 200;

        /// <summary>Стартова кількість їжі на фракцію.</summary>
        public int StartingFood { get; set; } = 100;

        // ── Параметри генерації ──────────────────────────────────────────

        /// <summary>Щільність лісів [0..1]. 0 = немає лісів, 1 = максимальна кількість.</summary>
        public float ForestDensity { get; set; } = 0.4f;

        /// <summary>Щільність гір [0..1].</summary>
        public float MountainDensity { get; set; } = 0.3f;

        /// <summary>Щільність водних зон [0..1].</summary>
        public float WaterDensity { get; set; } = 0.25f;

        /// <summary>Щільність POI (сіл, таборів тощо) [0..1].</summary>
        public float VillageDensity { get; set; } = 0.2f;

        /// <summary>Чи генерувати річки.</summary>
        public bool GenerateRivers { get; set; } = true;

        /// <summary>Чи генерувати біоми.</summary>
        public bool GenerateBiomes { get; set; } = true;

        /// <summary>Чи застосовувати WFC-полірування тайлів після генерації.</summary>
        public bool ApplyWFC { get; set; } = true;

        // ── Утиліти ──────────────────────────────────────────────────────

        /// <summary>
        /// Повертає фактичну ширину карти з урахуванням пресету.
        /// </summary>
        public int ResolvedWidth => SizePreset switch
        {
            WorldSizePreset.Small  => SmallWidth,
            WorldSizePreset.Medium => MediumWidth,
            WorldSizePreset.Large  => LargeWidth,
            WorldSizePreset.Custom => CustomWidth,
            _                      => 64
        };

        /// <summary>
        /// Повертає фактичну висоту карти з урахуванням пресету.
        /// </summary>
        public int ResolvedHeight => SizePreset switch
        {
            WorldSizePreset.Small  => SmallHeight,
            WorldSizePreset.Medium => MediumHeight,
            WorldSizePreset.Large  => LargeHeight,
            WorldSizePreset.Custom => CustomHeight,
            _                      => 64
        };

        /// <summary>
        /// Загальна кількість фракцій (люди + боти).
        /// </summary>
        public int TotalFactions => HumanPlayerCount + BotCount;
    }
}
