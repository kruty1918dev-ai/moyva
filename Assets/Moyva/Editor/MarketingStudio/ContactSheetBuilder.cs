using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Marketing.Contracts;
using UnityEngine;

namespace Kruty1918.Moyva.Marketing.EditorTools
{
    /// <summary>
    /// Builds a contact-sheet PNG tiling the run's produced screenshots for
    /// human-in-the-loop review (approve/reject/regenerate decisions).
    /// </summary>
    public static class ContactSheetBuilder
    {
        public static string Build(string runFolder, MarketingManifest manifest)
        {
            if (string.IsNullOrEmpty(runFolder) || manifest == null) return null;
            var images = new List<Texture2D>();
            foreach (var rel in manifest.filesGenerated)
            {
                if (!rel.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;
                if (rel.Contains("text_")) continue; // keep sheet clean of variants
                string abs = Path.Combine(runFolder, rel.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(abs)) continue;
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                try { tex.LoadImage(File.ReadAllBytes(abs)); }
                catch { UnityEngine.Object.DestroyImmediate(tex); continue; }
                images.Add(tex);
                if (images.Count >= 16) break;
            }
            if (images.Count == 0) return null;

            const int cols = 4;
            int thumbW = 480;
            int thumbH = Mathf.Max(1, Mathf.RoundToInt(thumbW * images[0].height / (float)images[0].width));
            int rows = Mathf.CeilToInt(images.Count / (float)cols);
            var sheet = new Texture2D(cols * thumbW, rows * thumbH, TextureFormat.RGBA32, false);
            var clear = new Color32(24, 24, 28, 255);
            var pixels = new Color32[sheet.width * sheet.height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;
            sheet.SetPixels32(pixels);

            var rt = new RenderTexture(thumbW, thumbH, 0, RenderTextureFormat.ARGB32);
            var prev = RenderTexture.active;
            try
            {
                for (int i = 0; i < images.Count; i++)
                {
                    Graphics.Blit(images[i], rt);
                    RenderTexture.active = rt;
                    var thumb = new Texture2D(thumbW, thumbH, TextureFormat.RGBA32, false);
                    thumb.ReadPixels(new Rect(0, 0, thumbW, thumbH), 0, 0);
                    thumb.Apply();
                    int col = i % cols;
                    int row = rows - 1 - i / cols;
                    sheet.SetPixels(col * thumbW, row * thumbH, thumbW, thumbH, thumb.GetPixels());
                    UnityEngine.Object.DestroyImmediate(thumb);
                }
            }
            finally
            {
                RenderTexture.active = prev;
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
                foreach (var img in images) UnityEngine.Object.DestroyImmediate(img);
            }

            sheet.Apply();
            string outPath = Path.Combine(runFolder, "ContactSheet.png");
            File.WriteAllBytes(outPath, sheet.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(sheet);
            return outPath;
        }
    }
}
