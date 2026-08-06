using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Вибирає активний visual path FogOfWar.
    ///
    /// ScreenSpace:
    /// - gameplay state записується у R8 texture;
    /// - TWC fog meshes не будуються;
    /// - FogOfWarVolumeController не запускає runtime volume build.
    ///
    /// LegacyTwcVolume:
    /// - використовується попередній TWC volume renderer.
    /// </summary>
    internal sealed class FogVisualUpdaterRouter
        : IFogVisualUpdater,
          IFogVolumeRuntimeUpdater,
          ITickable,
          IDisposable
    {
        private readonly FogOfWarSettings _settings;
        private readonly FogScreenSpaceTextureUpdater _screenSpaceUpdater;
        private readonly FogOfWarVolumeUpdater _legacyVolumeUpdater;

        private FogVisualPresentationMode? _lastLoggedMode;
        private bool _disposed;

        [Inject]
        public FogVisualUpdaterRouter(
            FogScreenSpaceTextureUpdater screenSpaceUpdater,
            FogOfWarVolumeUpdater legacyVolumeUpdater,
            [InjectOptional] FogOfWarSettings settings = null)
        {
            _screenSpaceUpdater =
                screenSpaceUpdater;

            _legacyVolumeUpdater =
                legacyVolumeUpdater;

            _settings =
                settings;

            LogActiveModeIfChanged();
        }

        private FogVisualPresentationMode ActiveMode
        {
            get
            {
                /*
                 * Коли settings відсутній, залишаємо стару поведінку.
                 * Це безпечніше для тестів і старих сцен.
                 */
                return _settings != null
                    ? _settings.PresentationMode
                    : FogVisualPresentationMode.LegacyTwcVolume;
            }
        }

        private IFogVisualUpdater ActiveVisualUpdater
        {
            get
            {
                return ActiveMode ==
                       FogVisualPresentationMode.ScreenSpace
                    ? _screenSpaceUpdater
                    : _legacyVolumeUpdater;
            }
        }

        private bool UsesLegacyVolume =>
            ActiveMode ==
            FogVisualPresentationMode.LegacyTwcVolume;

        public void Initialize(
            int width,
            int height,
            FogWorldVisualContext context)
        {
            LogActiveModeIfChanged();

            ActiveVisualUpdater.Initialize(
                width,
                height,
                context);
        }

        public void SetWorldContext(
            FogWorldVisualContext context)
        {
            LogActiveModeIfChanged();

            ActiveVisualUpdater.SetWorldContext(
                context);
        }

        public void PreviewRevealArea(
            Vector2Int center,
            int radius,
            FogRevealShape shape,
            bool keepVisible)
        {
            LogActiveModeIfChanged();

            ActiveVisualUpdater.PreviewRevealArea(
                center,
                radius,
                shape,
                keepVisible);
        }

        public void UpdateDirtyTiles(
            IFogOfWarService fogService,
            IEnumerable<Vector2Int> dirtyTiles)
        {
            LogActiveModeIfChanged();

            ActiveVisualUpdater.UpdateDirtyTiles(
                fogService,
                dirtyTiles);
        }

        public void RequestCellsUpdate(
            IFogOfWarService fogService,
            IReadOnlyList<FogCellVisualChange> changes,
            FogWorldVisualContext context)
        {
            LogActiveModeIfChanged();

            ActiveVisualUpdater.RequestCellsUpdate(
                fogService,
                changes,
                context);
        }

        public void RebuildFullVisual(
            IFogOfWarService fogService)
        {
            LogActiveModeIfChanged();

            ActiveVisualUpdater.RebuildFullVisual(
                fogService);
        }

        public void AttachController(
            FogOfWarVolumeController controller)
        {
            LogActiveModeIfChanged();

            /*
             * У screen-space режимі controller залишається
             * джерелом settings/world context, але не запускає
             * TWC runtime build.
             */
            if (!UsesLegacyVolume)
                return;

            _legacyVolumeUpdater.AttachController(
                controller);
        }

        public void DetachController(
            FogOfWarVolumeController controller)
        {
            /*
             * Detach виконуємо завжди, щоб коректно очистити
             * попереднє legacy-підключення при зміні режиму.
             */
            _legacyVolumeUpdater.DetachController(
                controller);
        }

        public void RequestStartupBuildFromController(
            FogOfWarVolumeController controller,
            FogWorldVisualContext context)
        {
            LogActiveModeIfChanged();

            if (!UsesLegacyVolume)
                return;

            _legacyVolumeUpdater
                .RequestStartupBuildFromController(
                    controller,
                    context);
        }

        public void RequestFullRebuildFromController(
            FogOfWarVolumeController controller)
        {
            LogActiveModeIfChanged();

            if (!UsesLegacyVolume)
                return;

            _legacyVolumeUpdater
                .RequestFullRebuildFromController(
                    controller);
        }

        public void Tick()
        {
            /*
             * Screen-space texture оновлюється одразу після
             * dirty/full update і не потребує власного Tick.
             */
            if (UsesLegacyVolume)
            {
                _legacyVolumeUpdater.Tick();
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _screenSpaceUpdater?.Dispose();
            _legacyVolumeUpdater?.Dispose();
        }

        private void LogActiveModeIfChanged()
        {
            FogVisualPresentationMode mode =
                ActiveMode;

            if (_lastLoggedMode.HasValue
                && _lastLoggedMode.Value == mode)
            {
                return;
            }

            _lastLoggedMode = mode;

            Debug.Log(
                "[FogOfWar] Visual presentation mode: " +
                $"{mode}.");
        }
    }
}