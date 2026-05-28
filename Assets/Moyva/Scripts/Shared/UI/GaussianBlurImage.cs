using System;
using UnityEngine;

namespace Kruty1918.Moyva.Shared.UI
{
    /// <summary>
    /// Legacy adapter kept for scene/prefab compatibility.
    /// Use UIBackgroundBlur for all new usage.
    /// </summary>
    [Obsolete("GaussianBlurImage is deprecated. Use UIBackgroundBlur instead.")]
    [AddComponentMenu("UI/Effects/Gaussian Blur Image (Legacy)")]
    public class GaussianBlurImage : UIBackgroundBlur
    {
    }
}
