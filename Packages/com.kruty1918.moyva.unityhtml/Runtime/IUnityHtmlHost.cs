using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityHTML.Runtime
{
    public interface IUnityHtmlHost : IDisposable
    {
        UnityHtmlMountResult Mount(
            RectTransform root,
            UnityHtmlDocument document,
            IReadOnlyDictionary<string, object> globals = null);

        void Unmount();
    }
}
