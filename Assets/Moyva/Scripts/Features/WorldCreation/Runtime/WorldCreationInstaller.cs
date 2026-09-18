using Kruty1918.Moyva.WorldCreation.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.WorldCreation.Runtime
{
    /// <summary>
    /// <para>
    /// Zenject-інсталер для модуля WorldCreation (Runtime).
    /// Реєструє <see cref="IWorldCreationService"/> у контейнері.
    /// </para>
    /// <para>
    /// ЯК ДОДАТИ ДО СЦЕНИ:
    /// 1. Додай компонент до GameObject зі SceneContext або ProjectContext.
    /// 2. Перетягни <see cref="WorldCreationDefaultsSO"/> у поле <b>defaults</b>.
    /// 3. Додай цей інсталер до списку Mono Installers у відповідному Context.
    /// </para>
    /// <para>
    /// Залежності, що надаються іншим модулям:
    ///   - <c>IWorldCreationService</c> (singleton)
    /// </para>
    /// </summary>
    public sealed class WorldCreationInstaller : MonoInstaller
    {
        [Tooltip("ScriptableObject із типовими налаштуваннями (Assets → Create → Moyva → WorldCreation → Defaults).")]
        [SerializeField] private WorldCreationDefaultsSO defaults;

        public override void InstallBindings()
        {
            if (defaults == null)
            {
            }

            Container.Bind<IWorldCreationService>()
                .To<WorldCreationService>()
                .AsSingle()
                .WithArguments(defaults);
        }
    }
}
