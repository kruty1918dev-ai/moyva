namespace Kruty1918.Moyva.Signals
{
    /// <summary>
    /// Плоска структура з усіма параметрами створення світу.
    /// Передається у <see cref="WorldCreationConfirmedSignal"/>.
    /// Містить лише примітивні типи, щоб уникнути залежності Signals → WorldCreation.
    /// </summary>
    public struct WorldCreationConfigData
    {
        // ── Основні параметри ────────────────────────────────────────────
        /// <summary>Назва світу (відображується в UI та збереженнях).</summary>
        public string WorldName;

        /// <summary>Seed генератора. 0 = використай поточний у GenerationRules.</summary>
        public int Seed;

        /// <summary>Індекс <c>WorldSizePreset</c> (0=Small, 1=Medium, 2=Large, 3=Custom).</summary>
        public int SizePresetIndex;

        /// <summary>Ширина в тайлах; актуально лише коли SizePresetIndex == 3 (Custom).</summary>
        public int CustomWidth;

        /// <summary>Висота в тайлах; актуально лише коли SizePresetIndex == 3 (Custom).</summary>
        public int CustomHeight;

        /// <summary>Індекс <c>MapTypePreset</c> (0=Balanced, 1=Continental, 2=Island, 3=Mountain, 4=Plains).</summary>
        public int MapTypePresetIndex;

        // ── Правила гри ──────────────────────────────────────────────────
        /// <summary>Індекс <c>DifficultyLevel</c> (0=Easy, 1=Normal, 2=Hard, 3=Brutal).</summary>
        public int DifficultyIndex;

        /// <summary>Чи включені боти у сесії.</summary>
        public bool EnableBots;

        /// <summary>Кількість людських гравців (1–4).</summary>
        public int HumanPlayerCount;

        /// <summary>Кількість бот-гравців (0–4).</summary>
        public int BotCount;

        /// <summary>Кількість стартового золота на фракцію.</summary>
        public int StartingGold;

        /// <summary>Кількість стартової їжі на фракцію.</summary>
        public int StartingFood;

        // ── Параметри генерації ──────────────────────────────────────────
        /// <summary>Щільність лісів [0..1].</summary>
        public float ForestDensity;

        /// <summary>Щільність гір [0..1].</summary>
        public float MountainDensity;

        /// <summary>Щільність водних зон [0..1].</summary>
        public float WaterDensity;

        /// <summary>Щільність POI (сіл, таборів тощо) [0..1].</summary>
        public float VillageDensity;

        /// <summary>Чи генерувати річки.</summary>
        public bool GenerateRivers;

        /// <summary>Чи генерувати біоми.</summary>
        public bool GenerateBiomes;

        /// <summary>Чи застосовувати Wave Function Collapse після генерації.</summary>
        public bool ApplyWFC;
    }

    /// <summary>
    /// Надсилається <see cref="Kruty1918.Moyva.WorldCreation.UI.WorldCreationUIController"/>
    /// коли гравець натискає «Створити світ» і конфіг пройшов валідацію.
    ///
    /// Отримується: Bootstrap / SceneLoader — щоб ініціалізувати сцену з потрібними
    /// налаштуваннями генератора та сесії.
    /// </summary>
    public struct WorldCreationConfirmedSignal
    {
        public WorldCreationConfigData Config;
    }

    /// <summary>
    /// Надсилається коли гравець натискає «Скасувати» на екрані створення світу.
    /// Отримується: SceneLoader / MainMenuController — щоб повернутись у головне меню.
    /// </summary>
    public struct WorldCreationCancelledSignal { }
}
