using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.RenderGraphModule.Util.RenderGraphUtils;

namespace Kruty1918.Moyva.Visuals
{
    /// <summary>
    /// Fullscreen URP pass that applies the day/night screen filter shader.
    /// </summary>
    public sealed class DayNightScreenFilterFeature : ScriptableRendererFeature
    {
        private const string ShaderName = "Moyva/2D/DayNight Screen Filter";

        [System.Serializable]
        private struct FilterSettings
        {
            public Color DayTint;
            public Color NightTint;
            public Color DawnTint;
            public Color DuskTint;
            [Range(0f, 1f)] public float FilterStrength;
            [Range(0f, 2f)] public float DaySaturation;
            [Range(0f, 2f)] public float NightSaturation;
            [Range(0.5f, 2f)] public float DayContrast;
            [Range(0.5f, 2f)] public float NightContrast;
            [Range(0f, 2f)] public float DayExposure;
            [Range(0f, 2f)] public float NightExposure;
            [Range(0f, 1f)] public float PhaseTintStrength;
            [Range(0f, 1f)] public float NightMinBrightness;
            [Range(0f, 1f)] public float ColorizeStrength;
        }

        public enum PresetKind
        {
            NeutralContrast = 0,
            CinematicDusk = 1,
            ExtremeNight = 2,
        }

        [Tooltip("Етап рендера, на якому застосовується fullscreen фільтр дня/ночі.")]
        [SerializeField] private RenderPassEvent _renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        [Header("Ручне налаштування")]
        [Tooltip("Базовий відтінок для фази Дня. Впливає на всю сцену у денний час.")]
        [SerializeField] private Color _dayTint = Color.white;
        [Tooltip("Базовий відтінок для фази Ночі. Чим темніший колір, тим глибша ніч.")]
        [SerializeField] private Color _nightTint = new Color(0.30f, 0.38f, 0.62f, 1f);
        [Tooltip("Відтінок для світанку (перехід Ніч -> День).")]
        [SerializeField] private Color _dawnTint = new Color(1.12f, 0.84f, 0.62f, 1f);
        [Tooltip("Відтінок для сутінок (перехід День -> Ніч).")]
        [SerializeField] private Color _duskTint = new Color(0.70f, 0.52f, 0.95f, 1f);
        [Tooltip("Загальна сила фільтра: 0 = вимкнено, 1 = повна інтенсивність.")]
        [SerializeField, Range(0f, 1f)] private float _filterStrength = 0.86f;
        [Tooltip("Насиченість кольорів удень.")]
        [SerializeField, Range(0f, 2f)] private float _daySaturation = 1.0f;
        [Tooltip("Насиченість кольорів уночі. Менше значення робить ніч менш кольоровою.")]
        [SerializeField, Range(0f, 2f)] private float _nightSaturation = 0.58f;
        [Tooltip("Контраст удень.")]
        [SerializeField, Range(0.5f, 2f)] private float _dayContrast = 1.0f;
        [Tooltip("Контраст уночі. Більше значення = виразніші світлі/темні області.")]
        [SerializeField, Range(0.5f, 2f)] private float _nightContrast = 1.35f;
        [Tooltip("Експозиція удень (загальна яскравість денної фази).")]
        [SerializeField, Range(0f, 2f)] private float _dayExposure = 1.02f;
        [Tooltip("Експозиція уночі. Зменшуйте для темнішої ночі.")]
        [SerializeField, Range(0f, 2f)] private float _nightExposure = 0.46f;
        [Tooltip("Сила підмішування відтінків світанку/сутінок у перехідних фазах.")]
        [SerializeField, Range(0f, 1f)] private float _phaseTintStrength = 0.85f;
        [Tooltip("Мінімальна яскравість уночі. 0 = майже повна темрява, 1 = без затемнення.")]
        [SerializeField, Range(0f, 1f)] private float _nightMinBrightness = 0.14f;
        [Tooltip("Наскільки сильно сцена приводиться до обраного фазового кольору.")]
        [SerializeField, Range(0f, 1f)] private float _colorizeStrength = 0.75f;

        private Material _material;
        private DayNightScreenFilterPass _pass;

        public override void Create()
        {
            if (_material == null)
            {
                var shader = Shader.Find(ShaderName);
                if (shader != null)
                    _material = CoreUtils.CreateEngineMaterial(shader);
            }

            ApplyMaterialProperties();

            _pass = new DayNightScreenFilterPass(_material)
            {
                renderPassEvent = _renderPassEvent
            };
        }

        private void OnValidate()
        {
            ApplyMaterialProperties();

            if (_pass != null)
                _pass.renderPassEvent = _renderPassEvent;
        }

        [ContextMenu("Apply Preset/1 Neutral Contrast")]
        public void ApplyPresetNeutralContrast()
        {
            ApplyPreset(PresetKind.NeutralContrast);
        }

        [ContextMenu("Apply Preset/2 Cinematic Dusk")]
        public void ApplyPresetCinematicDusk()
        {
            ApplyPreset(PresetKind.CinematicDusk);
        }

        [ContextMenu("Apply Preset/3 Extreme Night")]
        public void ApplyPresetExtremeNight()
        {
            ApplyPreset(PresetKind.ExtremeNight);
        }

        public void ApplyPreset(PresetKind preset)
        {
            FilterSettings settings = preset switch
            {
                PresetKind.CinematicDusk => CreateCinematicDuskPreset(),
                PresetKind.ExtremeNight => CreateExtremeNightPreset(),
                _ => CreateNeutralContrastPreset(),
            };

            _dayTint = settings.DayTint;
            _nightTint = settings.NightTint;
            _dawnTint = settings.DawnTint;
            _duskTint = settings.DuskTint;
            _filterStrength = settings.FilterStrength;
            _daySaturation = settings.DaySaturation;
            _nightSaturation = settings.NightSaturation;
            _dayContrast = settings.DayContrast;
            _nightContrast = settings.NightContrast;
            _dayExposure = settings.DayExposure;
            _nightExposure = settings.NightExposure;
            _phaseTintStrength = settings.PhaseTintStrength;
            _nightMinBrightness = settings.NightMinBrightness;
            _colorizeStrength = settings.ColorizeStrength;

            ApplyMaterialProperties();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_material == null || _pass == null)
                return;

            ApplyMaterialProperties();

            if (renderingData.cameraData.isPreviewCamera)
                return;

#if URP_COMPATIBILITY_MODE
            _pass.Setup(renderer.cameraColorTargetHandle);
#endif
            renderer.EnqueuePass(_pass);
        }

        protected override void Dispose(bool disposing)
        {
            if (_material != null)
            {
                CoreUtils.Destroy(_material);
                _material = null;
            }
        }

        private void ApplyMaterialProperties()
        {
            if (_material == null)
                return;

            FilterSettings active = ResolveActiveSettings();

            _material.SetColor("_DayTint", active.DayTint);
            _material.SetColor("_NightTint", active.NightTint);
            _material.SetColor("_DawnTint", active.DawnTint);
            _material.SetColor("_DuskTint", active.DuskTint);
            _material.SetFloat("_FilterStrength", active.FilterStrength);
            _material.SetFloat("_DaySaturation", active.DaySaturation);
            _material.SetFloat("_NightSaturation", active.NightSaturation);
            _material.SetFloat("_DayContrast", active.DayContrast);
            _material.SetFloat("_NightContrast", active.NightContrast);
            _material.SetFloat("_DayExposure", active.DayExposure);
            _material.SetFloat("_NightExposure", active.NightExposure);
            _material.SetFloat("_PhaseTintStrength", active.PhaseTintStrength);
            _material.SetFloat("_NightMinBrightness", active.NightMinBrightness);
            _material.SetFloat("_ColorizeStrength", active.ColorizeStrength);
        }

        private FilterSettings ResolveActiveSettings()
        {
            return new FilterSettings
            {
                DayTint = _dayTint,
                NightTint = _nightTint,
                DawnTint = _dawnTint,
                DuskTint = _duskTint,
                FilterStrength = _filterStrength,
                DaySaturation = _daySaturation,
                NightSaturation = _nightSaturation,
                DayContrast = _dayContrast,
                NightContrast = _nightContrast,
                DayExposure = _dayExposure,
                NightExposure = _nightExposure,
                PhaseTintStrength = _phaseTintStrength,
                NightMinBrightness = _nightMinBrightness,
                ColorizeStrength = _colorizeStrength,
            };
        }

        private static FilterSettings CreateNeutralContrastPreset()
        {
            return new FilterSettings
            {
                DayTint = Color.white,
                NightTint = new Color(0.34f, 0.42f, 0.68f, 1f),
                DawnTint = new Color(1.10f, 0.88f, 0.66f, 1f),
                DuskTint = new Color(0.74f, 0.56f, 0.96f, 1f),
                FilterStrength = 0.82f,
                DaySaturation = 1.0f,
                NightSaturation = 0.62f,
                DayContrast = 1.0f,
                NightContrast = 1.28f,
                DayExposure = 1.02f,
                NightExposure = 0.52f,
                PhaseTintStrength = 0.78f,
                NightMinBrightness = 0.18f,
                ColorizeStrength = 0.65f,
            };
        }

        private static FilterSettings CreateCinematicDuskPreset()
        {
            return new FilterSettings
            {
                DayTint = new Color(1.02f, 0.98f, 0.92f, 1f),
                NightTint = new Color(0.28f, 0.34f, 0.58f, 1f),
                DawnTint = new Color(1.18f, 0.84f, 0.56f, 1f),
                DuskTint = new Color(0.82f, 0.48f, 0.96f, 1f),
                FilterStrength = 0.90f,
                DaySaturation = 0.98f,
                NightSaturation = 0.52f,
                DayContrast = 1.02f,
                NightContrast = 1.38f,
                DayExposure = 1.0f,
                NightExposure = 0.46f,
                PhaseTintStrength = 0.95f,
                NightMinBrightness = 0.12f,
                ColorizeStrength = 0.82f,
            };
        }

        private static FilterSettings CreateExtremeNightPreset()
        {
            return new FilterSettings
            {
                DayTint = new Color(1.0f, 0.98f, 0.93f, 1f),
                NightTint = new Color(0.20f, 0.26f, 0.44f, 1f),
                DawnTint = new Color(1.24f, 0.82f, 0.50f, 1f),
                DuskTint = new Color(0.90f, 0.42f, 0.98f, 1f),
                FilterStrength = 1.0f,
                DaySaturation = 0.95f,
                NightSaturation = 0.25f,
                DayContrast = 1.04f,
                NightContrast = 1.70f,
                DayExposure = 1.0f,
                NightExposure = 0.22f,
                PhaseTintStrength = 1.0f,
                NightMinBrightness = 0.04f,
                ColorizeStrength = 0.95f,
            };
        }

        private sealed class DayNightScreenFilterPass : ScriptableRenderPass
        {
            private readonly Material _material;
#if URP_COMPATIBILITY_MODE
            private RTHandle _cameraColorTarget;
#endif

            public DayNightScreenFilterPass(Material material)
            {
                _material = material;
            }

#if URP_COMPATIBILITY_MODE
            public void Setup(RTHandle colorTarget)
            {
                _cameraColorTarget = colorTarget;
            }

            [System.Obsolete(DeprecationMessage.CompatibilityScriptingAPIObsoleteFrom2023_3)]
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (_material == null || _cameraColorTarget == null)
                    return;

                var cmd = CommandBufferPool.Get("Moyva DayNight Screen Filter");
                Blitter.BlitCameraTexture(cmd, _cameraColorTarget, _cameraColorTarget, _material, 0);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
#endif

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (_material == null)
                    return;

                var resourceData = frameData.Get<UniversalResourceData>();
                if (resourceData.isActiveTargetBackBuffer)
                    return;

                var source = resourceData.activeColorTexture;
                if (!source.IsValid())
                    return;

                var destinationDesc = renderGraph.GetTextureDesc(source);
                destinationDesc.name = "_MoyvaDayNightFilteredColor";
                destinationDesc.clearBuffer = false;

                var destination = renderGraph.CreateTexture(destinationDesc);

                var filterParams = new BlitMaterialParameters(source, destination, _material, 0);
                renderGraph.AddBlitPass(filterParams, "Moyva DayNight Screen Filter");

                var copyBackParams = new BlitMaterialParameters(destination, source, _material, 1);
                renderGraph.AddBlitPass(copyBackParams, "Moyva DayNight Copy Back");
            }
        }
    }
}