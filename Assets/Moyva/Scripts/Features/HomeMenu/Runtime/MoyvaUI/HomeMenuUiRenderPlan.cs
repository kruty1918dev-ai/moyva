using System;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>HomeMenuRenderOperation — enum: головного меню рендер Operation.</summary>
    internal enum HomeMenuRenderOperation
    {
        /// <summary>Варіант None.</summary>
        None,

        /// <summary>Варіант UpdateRegions.</summary>
        UpdateRegions,

        /// <summary>Варіант MountDocument.</summary>
        MountDocument
    }

    /// <summary>HomeMenuRenderPhase — enum: головного меню рендер фази.</summary>
    internal enum HomeMenuRenderPhase
    {
        /// <summary>Варіант Stable.</summary>
        Stable,
        /// <summary>Варіант ExitingRoute.</summary>
        ExitingRoute
    }

    /// <summary>HomeMenuUiSnapshot — class: головного меню UI знімка.</summary>
    internal sealed class HomeMenuUiSnapshot
    {
        /// <summary>маршрутизації — string.</summary>
        public string Route;
        /// <summary>вʼюпорта Class — string.</summary>
        public string ViewportClass;
        /// <summary>кореня Class — string.</summary>
        public string RootClass;
        /// <summary>Розмітка навігаційного блоку.</summary>
        public string NavMarkup;
        /// <summary>Розмітка бренд-блоку.</summary>
        public string BrandMarkup;
        /// <summary>Розмітка модальних вікон.</summary>
        public string ModalsMarkup;

        /// <summary>Захоплює Capture.</summary>
        public static HomeMenuUiSnapshot Capture(
            HomeMenuMoyvaUiState state,
            HomeMenuMoyvaUiViewController view,
            string viewportClass)
        {
            return new HomeMenuUiSnapshot
            {
                Route = ResolveRoute(state),
                ViewportClass = viewportClass ?? string.Empty,
                RootClass = HomeMenuMoyvaUiMarkup.BuildRootClass(state, viewportClass),
                NavMarkup = HomeMenuMoyvaUiMarkup.BuildRouteMarkup(state, view),
                BrandMarkup = HomeMenuMoyvaUiMarkup.BuildBrandMarkup(view),
                ModalsMarkup = HomeMenuMoyvaUiMarkup.BuildModalsMarkup(state, view)
            };
        }

        private static string ResolveRoute(HomeMenuMoyvaUiState state)
            => string.IsNullOrWhiteSpace(state.CurrentRoute) ? "Main" : state.CurrentRoute.Trim();

        /// <summary>Маршрутизує Equals.</summary>
        public bool RouteEquals(HomeMenuUiSnapshot other)
            => other != null && string.Equals(Route, other.Route, StringComparison.Ordinal);
    }

    /// <summary>HomeMenuRenderPlanner — class: головного меню рендер Planner.</summary>
    internal static class HomeMenuRenderPlanner
    {
        /// <summary>Обирає Operation.</summary>
        public static HomeMenuRenderOperation ChooseOperation(
            HomeMenuUiSnapshot mounted,
            HomeMenuUiSnapshot desired)
        {
            if (mounted == null || desired == null)
                return HomeMenuRenderOperation.MountDocument;

            // Document-level changes cannot be expressed by a region swap:
            // viewport switches the shell CSS profile, root classes gate scoped CSS
            // (route-*, controls-page, reduced-motion), modals live outside regions.
            if (!OrdinalEquals(mounted.ViewportClass, desired.ViewportClass) ||
                !OrdinalEquals(mounted.RootClass, desired.RootClass) ||
                !OrdinalEquals(mounted.ModalsMarkup, desired.ModalsMarkup))
                return HomeMenuRenderOperation.MountDocument;

            if (!OrdinalEquals(mounted.NavMarkup, desired.NavMarkup) ||
                !OrdinalEquals(mounted.BrandMarkup, desired.BrandMarkup))
                return HomeMenuRenderOperation.UpdateRegions;

            return HomeMenuRenderOperation.None;
        }

        private static bool OrdinalEquals(string a, string b)
            => string.Equals(a, b, StringComparison.Ordinal);
    }
}
