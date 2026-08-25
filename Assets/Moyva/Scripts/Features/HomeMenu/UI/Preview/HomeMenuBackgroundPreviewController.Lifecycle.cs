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
        private void Awake()
        {
            TryAutoAssignTargetImage();
            PrepareTargetImage();
            EnsureCloudLayer();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            TryAutoAssignTargetImage();
            ValidateKingdomPlacementSettings();
        }
#endif

        private void OnEnable()
        {
            TryAutoAssignTargetImage();
            PrepareTargetImage();
            EnsureCloudLayer();

            if (_regenerateOnEnable)
                RegeneratePreview();
            else
                ResetClouds();
        }

        private void Update()
        {
            if (_generatedTexture != null && _targetImage != null && _targetImage.enabled)
                ApplyCoverUv();

            TickClouds(Time.unscaledDeltaTime);
        }

        private void OnDisable()
        {
            ClearClouds();
            DestroyLiveMeshPreview();
            DisposeGeneratedTexture();
        }

        private void OnDestroy()
        {
            ClearClouds();
            DestroyLiveMeshPreview();
            DisposeGeneratedTexture();
            DestroyRuntimeCloudMaterial();

            if (_ownsCloudLayer && _cloudsLayer != null)
                Destroy(_cloudsLayer.gameObject);
        }
    }
}
