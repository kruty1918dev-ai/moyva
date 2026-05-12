using UnityEngine;
using Zenject;
using Kruty1918.Moyva.InfoPanel.API;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// Автоматичний інсталер для WorldUIConfig.
    /// Якщо конфіг не знайдений у контейнері, створює дефолтний.
    /// </summary>
    public sealed class WorldUIConfigAutoInstaller : MonoInstaller
    {
        [Header("UI Config (Optional)")]
        [SerializeField] private WorldUIConfigSO _uiConfig;

        [Header("Auto-Load from Resources")]
        [SerializeField] private bool _autoLoadFromResources = true;
        [SerializeField] private string _resourcesPath = "Configs/WorldUIConfig";

        public override void InstallBindings()
        {
            WorldUIConfigSO configToUse = _uiConfig;

            // Якщо конфіг не призначений, намагаємось завантажити з Resources
            if (configToUse == null && _autoLoadFromResources)
            {
                configToUse = Resources.Load<WorldUIConfigSO>(_resourcesPath);
                if (configToUse != null)
                {
                    Debug.Log($"[WorldUIConfigAutoInstaller] ✓ Конфіг завантажено з Resources: {_resourcesPath}");
                }
            }

            // Якщо конфіг знайдений, передаємо у контейнер
            if (configToUse != null)
            {
                Container.BindInstance(configToUse).AsSingle();
                Debug.Log("[WorldUIConfigAutoInstaller] ✓ WorldUIConfigSO передано у контейнер");
            }
            else
            {
                Debug.LogWarning("[WorldUIConfigAutoInstaller] ⚠ WorldUIConfigSO не знайдено. UI панелі можуть не працювати.");
            }
        }
    }
}
