namespace Kruty1918.Moyva.SaveSystem
{
    public enum GameLaunchMode
    {
        Unknown = 0,
        DirectGameplayTest = 1,
        MenuNewGame = 2,
        MenuLoadGame = 3,
        MenuJoinGame = 4,
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

        public static void ConfigureDirectGameplayTest()
        {
            Mode = GameLaunchMode.DirectGameplayTest;
            SaveSlot = 0;
            _autoLoadOverride = false;
            _autoSaveOverride = false;
        }

        public static void ConfigureMenuNewGame(int saveSlot = 0)
        {
            Mode = GameLaunchMode.MenuNewGame;
            SaveSlot = ClampSlot(saveSlot);
            _autoLoadOverride = false;
            _autoSaveOverride = true;
        }

        public static void ConfigureMenuLoadGame(int saveSlot)
        {
            Mode = GameLaunchMode.MenuLoadGame;
            SaveSlot = ClampSlot(saveSlot);
            _autoLoadOverride = true;
            _autoSaveOverride = true;
        }

        public static void ConfigureMenuJoinGame()
        {
            Mode = GameLaunchMode.MenuJoinGame;
            SaveSlot = 0;
            _autoLoadOverride = false;
            _autoSaveOverride = false;
        }

        public static bool IsAutoLoadEnabled()
            => _autoLoadOverride ?? SavePlayModeOptions.AutoLoadEnabled;

        public static bool IsAutoSaveEnabled()
            => _autoSaveOverride ?? SavePlayModeOptions.AutoSaveEnabled;

        private static int ClampSlot(int slot)
        {
            if (slot < 0) return 0;
            if (slot > 99) return 99;
            return slot;
        }
    }
}