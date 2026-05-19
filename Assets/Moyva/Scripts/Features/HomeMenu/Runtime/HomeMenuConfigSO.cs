using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Конфігурація сцен і часових затримок для старту гри з домашнього меню.
    /// Залежності: використовується HomeMenuGameStarter та startup pipeline.
    /// </summary>
    [CreateAssetMenu(fileName = "HomeMenuConfig", menuName = "Moyva/Home Menu/Home Menu Config")]
    public sealed class HomeMenuConfigSO : ScriptableObject
    {
        /// <summary>Назва gameplay-сцени.</summary>
        public string gameplaySceneName = "Gamplay_Scene";

        /// <summary>Назва сцени домашнього меню.</summary>
        public string homeMenuSceneName = "HomeMenu";

        /// <summary>Мінімальний час перед переходом до сцени для стабілізації UX.</summary>
        public float minPreloadSeconds = 0.8f;

        /// <summary>Додаткова затримка перед активацією сцени.</summary>
        public float sceneActivationDelay = 0.2f;
    }
}