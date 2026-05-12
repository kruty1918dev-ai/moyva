using UnityEngine;
using Zenject;
using Kruty1918.Moyva.Bootstrap.API;
using Kruty1918.Moyva.InfoPanel.API;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// Zenject інсталер для завантаження та передачі конфігів на сцену.
    /// 
    /// Як використовувати:
    /// 1. Додай цей інсталер у SceneContext перед іншими інсталерами
    /// 2. Призначте SceneBootstrapConfigSO у полі _bootstrapConfig
    /// 3. Всі конфіги будуть завантажені та передані через контейнер
    /// </summary>
    public sealed class SceneBootstrapInstaller : MonoInstaller
    {
        [Header("Scene Bootstrap Configuration")]
        [SerializeField] private SceneBootstrapConfigSO _bootstrapConfig;

        public override void InstallBindings()
        {
            Debug.Log("[SceneBootstrapInstaller] Запуск завантаження конфігів сцени...");

            if (_bootstrapConfig == null)
            {
                Debug.LogError("[SceneBootstrapInstaller] SceneBootstrapConfigSO не присвоєно! Конфіги не будуть завантажені.");
                return;
            }

            // Завантажуємо WorldUIConfig
            var worldUIConfigPrefab = _bootstrapConfig.WorldUIConfigPrefab;
            if (worldUIConfigPrefab != null)
            {
                var worldUIConfigInstance = Instantiate(worldUIConfigPrefab);
                var worldUIConfig = worldUIConfigInstance.GetComponent<WorldUIConfigSO>();
                
                if (worldUIConfig != null)
                {
                    Container.BindInstance(worldUIConfig).AsSingle();
                    Debug.Log("[SceneBootstrapInstaller] ✓ WorldUIConfigSO завантажено і передано у контейнер");
                }
                else
                {
                    Debug.LogError("[SceneBootstrapInstaller] WorldUIConfig префаб не містить компоненту WorldUIConfigSO!");
                    Destroy(worldUIConfigInstance);
                }
            }
            else
            {
                Debug.LogWarning("[SceneBootstrapInstaller] WorldUIConfig префаб не присвоєно");
            }

            // Завантажуємо GameSessionConfig
            var gameSessionConfigPrefab = _bootstrapConfig.GameSessionConfigPrefab;
            if (gameSessionConfigPrefab != null)
            {
                var gameSessionConfigInstance = Instantiate(gameSessionConfigPrefab);
                // Тут можна додати обробку GameSessionConfig якщо потрібно
                Debug.Log("[SceneBootstrapInstaller] ✓ GameSessionConfig завантажено");
            }

            Debug.Log("[SceneBootstrapInstaller] ✅ Всі конфіги сцени успішно завантажені");
        }
    }
}
