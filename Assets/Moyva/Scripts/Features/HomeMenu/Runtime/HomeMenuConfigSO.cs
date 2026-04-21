using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Конфігурація головного меню: назви сцен, тривалості прелоаду, параметри UI.
    /// Створюється через меню Assets → Create → Moyva → HomeMenu → Config.
    /// </summary>
    [CreateAssetMenu(menuName = "Moyva/HomeMenu/Config", fileName = "HomeMenuConfig")]
    public sealed class HomeMenuConfigSO : ScriptableObject
    {
        [Header("Сцени")]
        [Tooltip("Назва ігрової сцени (як у Build Settings). Завантажується після підтвердження створення світу.")]
        [SerializeField] private string gameplaySceneName = "Gamplay_Scene";

        [Tooltip("Назва сцени самого меню (для можливих повернень).")]
        [SerializeField] private string homeMenuSceneName = "HomeMenu";

        [Header("Прелоад")]
        [Tooltip("Мінімальний час (сек.) показу оверлею завантаження при старті меню. " +
                 "Гарантує плавний показ навіть на дуже швидкому залізі.")]
        [Min(0f)]
        [SerializeField] private float minPreloadSeconds = 0.8f;

        [Tooltip("Штучна затримка (сек.) між 90% прогресу та активацією сцени, " +
                 "щоб UI прогресу встиг оновитись. Рекомендовано 0.2.")]
        [Min(0f)]
        [SerializeField] private float sceneActivationDelay = 0.2f;

        /// <summary>Назва ігрової сцени.</summary>
        public string GameplaySceneName => gameplaySceneName;

        /// <summary>Назва сцени головного меню.</summary>
        public string HomeMenuSceneName => homeMenuSceneName;

        /// <summary>Мінімальний час показу оверлею при старті меню.</summary>
        public float MinPreloadSeconds => minPreloadSeconds;

        /// <summary>Затримка перед активацією нової сцени.</summary>
        public float SceneActivationDelay => sceneActivationDelay;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(gameplaySceneName))
                Debug.LogWarning($"[{nameof(HomeMenuConfigSO)}] '{nameof(gameplaySceneName)}' не задано.");
            if (string.IsNullOrWhiteSpace(homeMenuSceneName))
                Debug.LogWarning($"[{nameof(HomeMenuConfigSO)}] '{nameof(homeMenuSceneName)}' не задано.");
        }
    }
}
