using System;
using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogScreenSpaceTextureUpdater
    {
        private void CommitVisualState(
            string source)
        {
            FogScreenSpaceSettings screenSettings =
                ResolveScreenSettings();

            UploadImmediately();
            PublishShaderGlobals();

            if (screenSettings.UseLegacyWorldCurtain
                && screenSettings.CurtainEnabled)
            {
                _curtainRenderer.Rebuild(
                    _pixels,
                    _width,
                    _height,
                    _context);
            }
            else
            {
                _curtainRenderer.ClearPresentation();
            }

            if (screenSettings?.LogShaderDiagnostics == true)
            {
                int shaderStateHash =
                    ComputeFogStateHash();

                bool shaderStateChanged =
                    shaderStateHash
                    != _lastShaderStateHash;

                _lastShaderStateHash =
                    shaderStateHash;

                LogShaderDiagnosticsIfNeeded(
                    shaderStateChanged,
                    shaderStateHash);
            }
        }

        private void EnsureTexture(
            int width,
            int height)
        {
            width =
                Mathf.Max(
                    1,
                    width);

            height =
                Mathf.Max(
                    1,
                    height);

            if (_texture != null
                && _texture.width == width
                && _texture.height == height
                && _committedPixels != null
                && _committedPixels.Length
                == width * height
                && _pixels != null
                && _pixels.Length
                == width * height)
            {
                return;
            }

            DestroyTexture();

            _texture =
                new Texture2D(
                    width,
                    height,
                    TextureFormat.RGBA32,
                    false,
                    true)
                {
                    name =
                        "Moyva_FogStateTexture",

                    filterMode =
                        FilterMode.Point,

                    wrapMode =
                        TextureWrapMode.Clamp,

                    anisoLevel =
                        0,

                    hideFlags =
                        HideFlags.DontSave
                };

            _committedPixels =
                new Color32[
                    width * height];

            _pixels =
                new Color32[
                    width * height];

            FillBuffer(
                _committedPixels,
                UnexploredValue);

            CopyCommittedToVisual();

            _previewActive =
                false;

            _isDirty =
                true;
        }

        private void DestroyTexture()
        {
            if (_texture == null)
                return;

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(
                    _texture);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(
                    _texture);
            }

            _texture =
                null;
        }

        private static void FillBuffer(
            Color32[] buffer,
            Color32 value)
        {
            if (buffer == null)
                return;

            for (int i = 0;
                 i < buffer.Length;
                 i++)
            {
                buffer[i] =
                    value;
            }
        }

        private void CopyCommittedToVisual()
        {
            if (_committedPixels == null
                || _pixels == null
                || _committedPixels.Length
                != _pixels.Length)
            {
                return;
            }

            Array.Copy(
                _committedPixels,
                _pixels,
                _committedPixels.Length);

            _isDirty =
                true;
        }

        private void EndPreviewAndCopyCommittedToVisual()
        {
            _previewActive =
                false;

            CopyCommittedToVisual();
        }

        private bool SetCommittedPixelValue(
            Vector2Int tile,
            Color32 value)
        {
            return SetBufferPixelValue(
                _committedPixels,
                tile,
                value,
                false);
        }

        private bool SetPixelValue(
            Vector2Int tile,
            Color32 value)
        {
            return SetBufferPixelValue(
                _pixels,
                tile,
                value,
                true);
        }

        private bool SetBufferPixelValue(
            Color32[] buffer,
            Vector2Int tile,
            Color32 value,
            bool markVisualDirty)
        {
            if (buffer == null
                || !IsInBounds(tile))
            {
                return false;
            }

            int index =
                tile.x
                + tile.y * _width;

            Color32 current =
                buffer[index];

            if (AreEqual(
                    current,
                    value))
            {
                return false;
            }

            buffer[index] =
                value;

            if (markVisualDirty)
            {
                _isDirty =
                    true;
            }

            return true;
        }

        private static bool AreEqual(
            Color32 a,
            Color32 b)
        {
            return a.r == b.r
                   && a.g == b.g
                   && a.b == b.b
                   && a.a == b.a;
        }

    }
}
