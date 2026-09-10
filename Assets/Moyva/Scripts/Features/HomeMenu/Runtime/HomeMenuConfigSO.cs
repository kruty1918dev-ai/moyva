using UnityEngine;

using Kruty1918.Moyva.Jsonization;
namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Конфігурація сцен і часових затримок для старту гри з домашнього меню.
    /// Залежності: використовується HomeMenuGameStarter та startup pipeline.
    /// </summary>
[System.Serializable]
public sealed class HomeMenuConfigSO : MoyvaJsonConfigObject
    {
        /// <summary>Назва gameplay-сцени.</summary>
        public string gameplaySceneName = "Gamplay_Scene";

        /// <summary>Назва сцени домашнього меню.</summary>
        public string homeMenuSceneName = "HomeMenu";

        /// <summary>Мінімальний час перед переходом до сцени для стабілізації UX.</summary>
        public float minPreloadSeconds = 0.2f;

        /// <summary>Додаткова затримка перед активацією сцени.</summary>
        public float sceneActivationDelay = 0f;

        /// <summary>Enable the pilot UnityHTML shell for the main Home Menu. Missing JSON keeps the default false.</summary>
        public bool useUnityHtmlShell = false;

        public MenuSimulationSettings menuSimulation = new MenuSimulationSettings();
    }

    [System.Serializable]
    public sealed class MenuSimulationSettings
    {
        public bool enabled = false;
        public int mapSide = 32;
        public int mobileMapSide = 24;
        public int textureEdge = 960;
        public int mobileTextureEdge = 640;
        public int framesPerSecond = 24;
        public int mobileFramesPerSecond = 20;
        public int maxUnits = 8;
        public float turnSeconds = 3.5f;
        public float shotSeconds = 7f;
        public float cycleSeconds = 90f;
    }
}
