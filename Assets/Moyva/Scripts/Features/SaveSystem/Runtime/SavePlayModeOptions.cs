namespace Kruty1918.Moyva.SaveSystem
{
    public enum GameLaunchMode
    {
        Unknown = 0,
        DirectGameplayTest = 1,
        MenuNewGame = 2,
        MenuLoadGame = 3,
        MenuJoinGame = 4,
        MenuMultiplayerGame = 5,
    }

    /// <summary>
    /// Cross-scene runtime launch context used to align startup behavior.
    /// Allows gameplay scene to differentiate direct test launch vs menu launch.
    /// </summary>
    public static class GameLaunchContext
    {
        private static bool? _autoLoadOverride;
        private static bool? _autoSaveOverride;

        public static GameLaunchMode Mode { get; private set; } = GameLaunchMode.Unknown;
        public static int SaveSlot { get; private set; } = 0;
        public static bool HasWorldSettings { get; private set; }
        public static string WorldName { get; private set; } = string.Empty;
        public static int Seed { get; private set; }
        public static int Size { get; private set; }
        public static int Width { get; private set; }
        public static int Height { get; private set; }
        public static int MapType { get; private set; }
        public static int Difficulty { get; private set; }
        public static int MaxPlayers { get; private set; }
        public static bool IsPrivate { get; private set; }

        public static void ConfigureDirectGameplayTest()
        {
            Mode = GameLaunchMode.DirectGameplayTest;
            SaveSlot = 0;
            ClearWorldSettings();
            _autoLoadOverride = false;
            _autoSaveOverride = false;
        }

        public static void ConfigureMenuNewGame(int saveSlot = 0)
        {
            Mode = GameLaunchMode.MenuNewGame;
            SaveSlot = ClampSlot(saveSlot);
            ClearWorldSettings();
            _autoLoadOverride = false;
            _autoSaveOverride = true;
        }

        public static void ConfigureMenuNewGame(
            int saveSlot,
            string worldName,
            int seed,
            int size,
            int mapType,
            int difficulty,
            int maxPlayers,
            bool isPrivate,
            int width = 0,
            int height = 0)
        {
            Mode = GameLaunchMode.MenuNewGame;
            SaveSlot = ClampSlot(saveSlot);
            SetWorldSettings(worldName, seed, size, mapType, difficulty, maxPlayers, isPrivate, width, height);
            _autoLoadOverride = false;
            _autoSaveOverride = true;
        }

        public static void ConfigureMenuLoadGame(int saveSlot)
        {
            Mode = GameLaunchMode.MenuLoadGame;
            SaveSlot = ClampSlot(saveSlot);
            ClearWorldSettings();
            _autoLoadOverride = true;
            _autoSaveOverride = true;
        }

        public static void ConfigureMenuJoinGame()
        {
            Mode = GameLaunchMode.MenuJoinGame;
            SaveSlot = 0;
            ClearWorldSettings();
            _autoLoadOverride = false;
            _autoSaveOverride = false;
        }

        public static void ConfigureMenuMultiplayerGame(
            string worldName,
            int seed,
            int size,
            int mapType,
            int difficulty,
            int maxPlayers,
            bool isPrivate,
            int width = 0,
            int height = 0)
        {
            Mode = GameLaunchMode.MenuMultiplayerGame;
            SaveSlot = 0;
            SetWorldSettings(worldName, seed, size, mapType, difficulty, maxPlayers, isPrivate, width, height);
            _autoLoadOverride = false;
            _autoSaveOverride = false;
        }

        public static bool IsAutoLoadEnabled()
            => _autoLoadOverride ?? SavePlayModeOptions.AutoLoadEnabled;

        public static bool IsAutoSaveEnabled()
            => _autoSaveOverride ?? SavePlayModeOptions.AutoSaveEnabled;

        public static bool TryGetWorldDimensions(out int width, out int height)
        {
            width = 0;
            height = 0;

            if (!HasWorldSettings)
                return false;

            if (Width > 0 && Height > 0)
            {
                width = Width;
                height = Height;
                return true;
            }

            int side = Size switch
            {
                0 => 32,
                1 => 64,
                2 => 128,
                _ => 64,
            };

            width = side;
            height = side;
            return true;
        }

        public static bool TryGetSeed(out int seed)
        {
            seed = HasWorldSettings ? Seed : 0;
            return HasWorldSettings && seed != 0;
        }

        private static void SetWorldSettings(
            string worldName,
            int seed,
            int size,
            int mapType,
            int difficulty,
            int maxPlayers,
            bool isPrivate,
            int width,
            int height)
        {
            HasWorldSettings = true;
            WorldName = string.IsNullOrWhiteSpace(worldName)
                ? "Новий світ"
                : worldName.Trim();
            Seed = seed;
            Size = size;
            Width = width > 0 ? width : 0;
            Height = height > 0 ? height : 0;
            MapType = mapType;
            Difficulty = difficulty;
            MaxPlayers = maxPlayers < 1 ? 1 : maxPlayers;
            IsPrivate = isPrivate;
        }

        private static void ClearWorldSettings()
        {
            HasWorldSettings = false;
            WorldName = string.Empty;
            Seed = 0;
            Size = 0;
            Width = 0;
            Height = 0;
            MapType = 0;
            Difficulty = 0;
            MaxPlayers = 0;
            IsPrivate = false;
        }

        private static int ClampSlot(int slot)
        {
            if (slot < 0) return 0;
            if (slot > 99) return 99;
            return slot;
        }
    }

    /// <summary>
    /// Runtime-accessible play mode flags controlled from editor tools.
    /// In player builds both flags are always enabled.
    /// </summary>
    public static class SavePlayModeOptions
    {
        private const string AutoLoadKey = "Moyva.Save.PlayMode.AutoLoad";
        private const string AutoSaveKey = "Moyva.Save.PlayMode.AutoSave";

        public static bool AutoLoadEnabled
        {
            get => GetBool(AutoLoadKey, true);
            set => SetBool(AutoLoadKey, value);
        }

        public static bool AutoSaveEnabled
        {
            get => GetBool(AutoSaveKey, true);
            set => SetBool(AutoSaveKey, value);
        }

        private static bool GetBool(string key, bool defaultValue)
        {
#if UNITY_EDITOR
            return UnityEditor.EditorPrefs.GetBool(key, defaultValue);
#else
            return true;
#endif
        }

        private static void SetBool(string key, bool value)
        {
#if UNITY_EDITOR
            UnityEditor.EditorPrefs.SetBool(key, value);
#endif
        }
    }
}