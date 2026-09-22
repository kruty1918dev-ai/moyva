using Kruty1918.Moyva.Combat.API;
using Zenject;
using Kruty1918.EntityHealth;

namespace Kruty1918.Moyva.Combat
{
    /// <summary>
    /// Zenject installer для системи здоров'я.
    /// Встановлює <see cref="IHealthRegistry"/> як singleton.
    ///
    /// Підключення:
    ///   Додайте <see cref="CombatInstaller"/> у список installers вашого SceneContext або ProjectContext.
    /// </summary>
    public sealed class CombatInstaller : Installer<CombatInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IHealthRegistry>()
                .To<HealthRegistry>()
                .AsSingle();
        }
    }
}
