using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kruty1918.Moyva.Grid.Runtime
{
    /// <summary>
    /// Zenject installer модуля Grid.
    /// Формує залежності між <see cref="IGridService"/>, <see cref="IGridResizeService"/>,
    /// <see cref="ITileSettingsService"/> та конфігурацією <see cref="TileRegistrySO"/>.
    /// </summary>
    public class GridInstaller : MonoInstaller
    {
        /// <summary>
        /// Реєстр типів тайлів, який постачається у DI як singleton instance.
        /// </summary>
        [SerializeField] private TileRegistrySO tileRegistry;

        /// <summary>
        /// Профілі шарів terrain (нова layer-based ідентичність). Якщо задано,
        /// сервіси читають вагу руху/блокування будівництва саме звідси.
        /// </summary>
        [SerializeField] private TerrainLayerProfileSO terrainLayerProfiles;

        [SerializeField] private MoyvaProjectSettingsSO projectSettings;

        /// <summary>
        /// Базова ширина сітки, якщо стартовий контекст запуску не перевизначив розмір.
        /// </summary>
        [SerializeField] private int gridWidth = 10;

        /// <summary>
        /// Базова висота сітки, якщо стартовий контекст запуску не перевизначив розмір.
        /// </summary>
        [SerializeField] private int gridHeight = 10;

        /// <summary>
        /// Реєструє залежності модуля Grid у контейнері Zenject.
        /// </summary>
        public override void InstallBindings()
        {
            // 1) Беремо інспекторні значення як дефолтний розмір світу.
            int resolvedWidth = gridWidth;
            int resolvedHeight = gridHeight;

            // 2) Якщо SaveSystem надав launch-розмір світу, перевизначаємо дефолти.
            if (TryGetLaunchWorldDimensions(out int launchWidth, out int launchHeight))
            {
                resolvedWidth = launchWidth;
                resolvedHeight = launchHeight;
            }

            // 3) Публікуємо TileRegistry як singleton instance для всіх споживачів.
            Container.BindInstance(tileRegistry).AsSingle();

            // 3b) Профілі шарів terrain (опційно) — нове джерело параметрів за id шару.
            if (terrainLayerProfiles != null)
                Container.BindInstance(terrainLayerProfiles).AsSingle();

            var resolvedProjectSettings = ResolveProjectSettings();
            if (!Container.HasBinding<MoyvaProjectSettingsSO>())
                Container.BindInstance(resolvedProjectSettings).AsSingle();

            if (!Container.HasBinding<IGridProjection>())
                Container.Bind<IGridProjection>().FromInstance(GridProjectionFactory.Create(resolvedProjectSettings)).AsSingle();

            Container.BindInterfacesTo<Project3DLightingInitializer>().AsSingle().NonLazy();

            // 4) Реєструємо центральний GridService як реалізацію IGridService.
            //    Додатково передаємо розв'язані ширину/висоту через конструктор.
            Container.Bind<IGridService>().To<GridService>().AsSingle()
                .WithArguments(resolvedWidth, resolvedHeight);

            // 5) Реєструємо сервіс читання параметрів тайлів (вага руху тощо).
            Container.Bind<ITileSettingsService>().To<TileSettingsService>().AsSingle();
        }

        private MoyvaProjectSettingsSO ResolveProjectSettings()
        {
            MoyvaProjectSettingsSO resolved = projectSettings != null
                ? projectSettings
                : LoadProjectSettingsAssetOrDefault();
            resolved.Normalize();
            return resolved;
        }

        private static MoyvaProjectSettingsSO LoadProjectSettingsAssetOrDefault()
        {
#if UNITY_EDITOR
            var settings = AssetDatabase.LoadAssetAtPath<MoyvaProjectSettingsSO>(MoyvaProjectSettingsSO.DefaultAssetPath);
            if (settings != null)
                return settings;
#endif

            return MoyvaProjectSettingsSO.CreateRuntimeDefault();
        }

        /// <summary>
        /// Прагне дістати стартові розміри світу через рефлексію з SaveSystem.GameLaunchContext.
        /// Використано рефлексію, щоб уникнути жорсткої compile-time залежності Grid -> SaveSystem API-тип.
        /// </summary>
        /// <param name="width">Ширина, якщо значення знайдено.</param>
        /// <param name="height">Висота, якщо значення знайдено.</param>
        /// <returns><see langword="true"/>, якщо валідні розміри отримані; інакше <see langword="false"/>.</returns>
        private static bool TryGetLaunchWorldDimensions(out int width, out int height)
        {
            // 1) Ініціалізуємо значення за замовчуванням для out-параметрів.
            width = 0;
            height = 0;

            // 2) Знаходимо тип GameLaunchContext у збірці SaveSystem.
            var contextType = System.Type.GetType("Kruty1918.Moyva.SaveSystem.GameLaunchContext, Kruty1918.Moyva.SaveSystem");

            // 3) Отримуємо публічний статичний метод TryGetWorldDimensions(int,out int).
            var method = contextType?.GetMethod("TryGetWorldDimensions", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            // 4) Якщо тип/метод відсутній, працюємо з дефолтними розмірами.
            if (method == null)
                return false;

            // 5) Готуємо буфер аргументів для виклику методу через reflection.
            object[] args = { width, height };

            // 6) Викликаємо метод і перевіряємо, чи він повернув успіх.
            if (!(method.Invoke(null, args) is bool result) || !result)
                return false;

            // 7) Читаємо out-параметри назад у локальні змінні.
            width = (int)args[0];
            height = (int)args[1];

            // 8) Валідуємо, що розміри додатні.
            return width > 0 && height > 0;
        }
    }

    internal sealed class Project3DLightingInitializer : IInitializable
    {
        private const string RuntimeDirectionalLightName = "Moyva 3D Directional Light";

        private readonly MoyvaProjectSettingsSO _settings;

        public Project3DLightingInitializer([InjectOptional] MoyvaProjectSettingsSO settings = null)
        {
            _settings = settings;
        }

        public void Initialize()
        {
            if (_settings == null)
                return;

            _settings.Normalize();
            if (!_settings.Uses3DProjectMode() || !_settings.AutoConfigure3DLighting)
                return;

            ConfigureAmbientLighting();
            ConfigureDirectionalLight();
            ConfigureAtmosphericFog();
        }

        private void ConfigureAmbientLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = _settings.Project3DAmbientSkyColor;
            RenderSettings.ambientEquatorColor = _settings.Project3DAmbientEquatorColor;
            RenderSettings.ambientGroundColor = _settings.Project3DAmbientGroundColor;
            RenderSettings.reflectionIntensity = Mathf.Clamp(RenderSettings.reflectionIntensity <= 0f ? 0.45f : RenderSettings.reflectionIntensity, 0f, 1f);
        }

        private void ConfigureDirectionalLight()
        {
            if (!_settings.CreateDirectionalLightIn3D)
                return;

            Light directionalLight = FindDirectionalLight();
            if (directionalLight == null)
            {
                var lightObject = new GameObject(RuntimeDirectionalLightName);
                directionalLight = lightObject.AddComponent<Light>();
            }

            directionalLight.type = LightType.Directional;
            directionalLight.transform.rotation = Quaternion.Euler(_settings.Project3DLightEuler);
            directionalLight.color = _settings.Project3DLightColor;
            directionalLight.intensity = Mathf.Clamp(_settings.Project3DLightIntensity, 0f, 8f);
            directionalLight.shadows = _settings.Project3DLightShadows ? LightShadows.Soft : LightShadows.None;
        }

        private void ConfigureAtmosphericFog()
        {
            RenderSettings.fog = _settings.Enable3DAtmosphericFog;
            if (!RenderSettings.fog)
                return;

            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = _settings.Project3DAtmosphericFogColor;
            RenderSettings.fogDensity = Mathf.Clamp(_settings.Project3DAtmosphericFogDensity, 0f, 0.1f);
        }

        private static Light FindDirectionalLight()
        {
            var lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null && lights[i].type == LightType.Directional)
                    return lights[i];
            }

            return null;
        }
    }
}