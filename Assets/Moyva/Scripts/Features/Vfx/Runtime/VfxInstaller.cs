using Kruty1918.JsonConfig;
using Kruty1918.Moyva.Vfx.API;
using Zenject;

namespace Kruty1918.Moyva.Vfx.Runtime
{
    /// <summary>
    /// Композиція VFX-шару геймплею. Викликається з BootstrapInstaller
    /// (сцена гри) — сервіс сценовий, бо ефекти прив'язані до світу.
    /// </summary>
    public static class VfxInstaller
    {
        /// <summary>Реєструє VFX-сервіси в контейнері.</summary>
        public static void Install(DiContainer container)
        {
            if (!container.HasBinding<VfxCatalogConfig>())
            {
                container.Bind<VfxCatalogConfig>()
                    .FromMethod(_ => JsonConfigRuntime.GetLegacyResource<VfxCatalogConfig>(
                        nameof(VfxCatalogConfig)))
                    .AsSingle()
                    .IfNotBound();
            }

            // Pooled spawner: root створюється Zenject-ом (без new GameObject у
            // runtime-коді) і живе разом зі сценою.
            if (!container.HasBinding<VfxPool>())
            {
                container.Bind<VfxPool>()
                    .FromMethod(ctx => new VfxPool(
                        ctx.Container.CreateEmptyGameObject("[VfxPool]").transform))
                    .AsSingle();
            }

            if (!container.HasBinding<IVfxSpawner>())
            {
                container.Bind<IVfxSpawner>()
                    .FromMethod(ctx => ctx.Container.Resolve<VfxPool>())
                    .AsSingle();
            }

            container.BindInterfacesAndSelfTo<GameplayVfxService>()
                .AsSingle()
                .NonLazy();
        }
    }
}
