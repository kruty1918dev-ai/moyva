using Kruty1918.Moyva.FogOfWar.API;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Вузький runtime API, через який <see cref="FogOfWarVolumeController"/> взаємодіє з volume visual updater-ом.
    /// Дозволяє controller-у не залежати від concrete <c>FogOfWarVolumeUpdater</c>.
    /// </summary>
    internal interface IFogVolumeRuntimeUpdater
    {
        /// <summary>
        /// Під'єднує controller як host для runtime visual update path.
        /// </summary>
        /// <param name="controller">Host-компонент сцени.</param>
        void AttachController(FogOfWarVolumeController controller);

        /// <summary>
        /// Від'єднує controller від runtime visual updater-а.
        /// </summary>
        /// <param name="controller">Host-компонент, який більше не активний.</param>
        void DetachController(FogOfWarVolumeController controller);

        /// <summary>
        /// Просить updater виконати первинний runtime build до приходу gameplay fog dirty-updates.
        /// </summary>
        /// <param name="controller">Host-компонент сцени.</param>
        /// <param name="context">Світовий контекст для volume build.</param>
        void RequestStartupBuildFromController(FogOfWarVolumeController controller, FogWorldVisualContext context);

        /// <summary>
        /// Просить updater перебудувати весь fog volume з поточного gameplay fog state.
        /// </summary>
        /// <param name="controller">Host-компонент сцени.</param>
        void RequestFullRebuildFromController(FogOfWarVolumeController controller);
    }
}
