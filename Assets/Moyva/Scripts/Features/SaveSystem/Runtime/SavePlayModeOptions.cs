namespace Kruty1918.Moyva.SaveSystem
{
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