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
        bool SetValue(string elementId, string value);
    }

    public interface IUnityHtmlMotion
    {
        void Play(string targetId, string preset, float duration, float delay);
        void Stop(string targetId);
    }
}
