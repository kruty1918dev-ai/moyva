using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Clouds.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public sealed partial class HomeMenuBackgroundPreviewController
    {
        private void TryAutoAssignTargetImage()
        {
            if (_targetImage == null)
                _targetImage = GetComponent<RawImage>();
        }

        private void PrepareTargetImage()
        {
            if (_targetImage == null)
                return;

            if (_stretchTargetToParent)
            {
                RectTransform targetRect = _targetImage.rectTransform;
                targetRect.anchorMin = Vector2.zero;
                targetRect.anchorMax = Vector2.one;
                targetRect.offsetMin = Vector2.zero;
                targetRect.offsetMax = Vector2.zero;
                targetRect.pivot = new Vector2(0.5f, 0.5f);
            }

            _targetImage.uvRect = new Rect(0f, 0f, 1f, 1f);
        }

        private void ApplyCoverUv()
        {
            if (_targetImage == null || _generatedTexture == null)
                return;

            var rect = _targetImage.rectTransform.rect;
            float viewportWidth = rect.width > 1f ? rect.width : Screen.width;
            float viewportHeight = rect.height > 1f ? rect.height : Screen.height;
            if (viewportWidth <= 1f || viewportHeight <= 1f)
            {
                _targetImage.uvRect = new Rect(0f, 0f, 1f, 1f);
                return;
            }

            float viewportAspect = viewportWidth / viewportHeight;
            float textureAspect = _generatedTexture.width / (float)_generatedTexture.height;

            Rect uv = new Rect(0f, 0f, 1f, 1f);
            if (viewportAspect > textureAspect)
            {
                float uvHeight = Mathf.Clamp01(textureAspect / viewportAspect);
                uv.y = (1f - uvHeight) * 0.5f;
                uv.height = uvHeight;
            }
            else
            {
                float uvWidth = Mathf.Clamp01(viewportAspect / textureAspect);
                uv.x = (1f - uvWidth) * 0.5f;
                uv.width = uvWidth;
            }

            _targetImage.uvRect = uv;
        }

    }
}
