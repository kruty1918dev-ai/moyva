using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityHTML.Runtime
{
    public interface IUnityHtmlHost : IDisposable
    {
        IUnityHtmlMotion Motion { get; }

        UnityHtmlMountResult Mount(
            RectTransform root,
            UnityHtmlDocument document,
            IReadOnlyDictionary<string, object> globals = null);

        void Unmount();
        bool UpdateRegion(string elementId, string html);
        bool UpdateRegions(
            IReadOnlyDictionary<string, string> regions,
            IReadOnlyDictionary<string, object> globals = null);
        bool SetValue(string elementId, string value);
    }

    public interface IUnityHtmlMotion
    {
        void Play(string targetId, string preset, float duration, float delay);
        void Stop(string targetId);

        /// <summary>
        /// Deterministically returns the element to its resting transform/alpha,
        /// whether or not a tracked motion is still active. A completed fade-out
        /// removes itself from the active set but leaves the CanvasGroup at alpha 0;
        /// this also covers that case by snapping alpha to the computed opacity.
        /// </summary>
        void RestoreResting(string targetId);
    }
}
