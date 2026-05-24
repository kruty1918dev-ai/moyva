using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

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

            var resolvedProjectSettings = ResolveProjectSettings();
            if (!Container.HasBinding<MoyvaProjectSettingsSO>())
                Container.BindInstance(resolvedProjectSettings).AsSingle();

            if (!Container.HasBinding<IGridProjection>())
                Container.Bind<IGridProjection>().FromInstance(GridProjectionFactory.Create(resolvedProjectSettings)).AsSingle();

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
                : MoyvaProjectSettingsSO.CreateRuntimeDefault();
            resolved.Normalize();
            return resolved;
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
}