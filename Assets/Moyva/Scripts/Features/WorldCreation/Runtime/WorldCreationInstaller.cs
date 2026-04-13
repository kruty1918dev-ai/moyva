using Kruty1918.Moyva.WorldCreation.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.WorldCreation.Runtime
{
    /// <summary>
    /// Zenject-інсталер для модуля WorldCreation (Runtime).
    /// Реєструє <see cref="IWorldCreationService"/> у контейнері.
    ///
    /// ЯК ДОДАТИ ДО СЦЕНИ:
    /// 1. Додай компонент до GameObject зі SceneContext або ProjectContext.
    /// 2. Перетягни <see cref="WorldCreationDefaultsSO"/> у поле <b>defaults</b>.
    /// 3. Додай цей інсталер до списку Mono Installers у відповідному Context.
    ///
    /// Залежності, що надаються іншим модулям:
    ///   - <c>IWorldCreationService</c> (singleton)
    /// </summary>
    public sealed class WorldCreationInstaller : MonoInstaller
    {
        [Tooltip("ScriptableObject із типовими налаштуваннями (Assets → Create → Moyva → WorldCreation → Defaults).")]
        [SerializeField] private WorldCreationDefaultsSO defaults;

        public override void InstallBindings()
        {
            if (defaults == null)
            {
                Debug.LogWarning("[WorldCreationInstaller] Поле 'defaults' не призначено. " +
                                 "WorldCreationService використає вбудовані значення за замовчуванням.");
            }

            Container.Bind<IWorldCreationService>()
                .To<WorldCreationService>()
                .AsSingle()
                .WithArguments(defaults);
        }
    }
}
