using System;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// The render pipeline never asks "when did state change"; it diffs the desired
    /// snapshot (derived purely from <see cref="HomeMenuMoyvaUiState"/> + view data)
    /// against the snapshot that was actually committed to the DOM and picks the
    /// cheapest operation that makes them equal.
    /// </summary>
    internal enum HomeMenuRenderOperation
    {
        /// <summary>Desired snapshot already matches the mounted document.</summary>
        None,

        /// <summary>Only region children changed; swap nav/brand regions in place.</summary>
        UpdateRegions,

        /// <summary>
        /// Root classes, modals, viewport, or document identity changed — submit the
        /// full document. The host reconciles in place and only falls back to a
        /// fresh context if reconciliation provably fails, so a remount happens
        /// exactly when needed and never because of timing.
        /// </summary>
        MountDocument
    }

    /// <summary>
    /// Explicit transition phases of the shell. Route swaps run
    /// Stable -> ExitingRoute -> Stable; every other change applies immediately.
    /// While ExitingRoute, incoming state changes only update the pending snapshot —
    /// the deadline handler always renders the latest desired state, so stale
    /// deferred work can never resurrect an old page.
    /// </summary>
    internal enum HomeMenuRenderPhase
    {
        Stable,
        ExitingRoute
    }

    /// <summary>
    /// Immutable description of everything the mounted document is supposed to show.
    /// Comparing snapshots replaces the scattered caches (_lastRouteMarkup,
    /// _mountedRootClass, _lastRenderedRoute, ...) that used to drift out of sync
    /// with the real DOM and caused blank panels / missed remounts.
    /// </summary>
    internal sealed class HomeMenuUiSnapshot
    {
        public string Route;
        public string ViewportClass;
        public string RootClass;
        public string NavMarkup;
        public string BrandMarkup;
        public string ModalsMarkup;

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

        public bool RouteEquals(HomeMenuUiSnapshot other)
            => other != null && string.Equals(Route, other.Route, StringComparison.Ordinal);
    }

    /// <summary>
    /// Pure decision table: given the mounted snapshot and the desired snapshot,
    /// choose the cheapest render operation that makes the DOM match state.
    /// Keeping this pure makes the transition model deterministic and unit-testable.
    /// </summary>
    internal static class HomeMenuRenderPlanner
    {
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
