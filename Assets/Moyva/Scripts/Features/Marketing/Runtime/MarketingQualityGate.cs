using System.Collections.Generic;
using Kruty1918.Moyva.Marketing.Contracts;
using UnityEngine;

namespace Kruty1918.Moyva.Marketing.Runtime
{
    /// <summary>
    /// Post-capture QA. Failed output is flagged and never reported as
    /// successful in the manifest.
    /// </summary>
    public static class MarketingQualityGate
    {
        public static List<string> Check(Texture2D frame, int expectedW, int expectedH,
            float expectedAspect, bool expectUi)
        {
            var issues = new List<string>();
            if (frame == null)
            {
                issues.Add("frame-null");
                return issues;
            }
            if (frame.width != expectedW || frame.height != expectedH)
                issues.Add($"wrong-resolution:{frame.width}x{frame.height}!={expectedW}x{expectedH}");
            float aspect = (float)frame.width / frame.height;
            if (Mathf.Abs(aspect - expectedAspect) > 0.02f)
                issues.Add($"wrong-aspect:{aspect:F2}!={expectedAspect:F2}");

            var pixels = frame.GetPixels32();
            int stride = Mathf.Max(1, frame.width / 96);
            int black = 0, white = 0, magenta = 0, total = 0;
            for (int i = 0; i < pixels.Length; i += stride)
            {
                var p = pixels[i];
                if (p.r < 16 && p.g < 16 && p.b < 16) black++;
                if (p.r > 239 && p.g > 239 && p.b > 239) white++;
                if (p.r > 200 && p.b > 200 && p.g < 60) magenta++;
                total++;
            }
            if ((float)black / total > 0.9f) issues.Add("black-frame");
            if ((float)white / total > 0.9f) issues.Add("white-frame");
            if ((float)magenta / total > 0.005f) issues.Add("magenta-shader");
            return issues;
        }
    }
}
