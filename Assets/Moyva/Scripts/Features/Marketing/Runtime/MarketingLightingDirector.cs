using System.Collections.Generic;
using Kruty1918.Moyva.Marketing.Contracts;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Marketing.Runtime
{
    /// <summary>
    /// Applies lighting presets to the scene's directional light + ambient.
    /// Stores originals and restores them on teardown — the gameplay scene is
    /// never permanently modified.
    /// </summary>
    public sealed class MarketingLightingDirector
    {
        private struct SunState
        {
            public Light light;
            public Color color;
            public float intensity;
            public Quaternion rotation;
            public float shadowStrength;
        }

        private readonly List<SunState> _originals = new List<SunState>();
        private Color _ambientColor;
        private AmbientMode _ambientMode;
        private float _ambientIntensity;
        private bool _captured;

        public void Capture()
        {
            if (_captured) return;
            _captured = true;
            _originals.Clear();
            foreach (var l in Object.FindObjectsByType<Light>())
            {
                if (l.type != LightType.Directional) continue;
                _originals.Add(new SunState
                {
                    light = l,
                    color = l.color,
                    intensity = l.intensity,
                    rotation = l.transform.rotation,
                    shadowStrength = l.shadowStrength,
                });
            }
            _ambientColor = RenderSettings.ambientLight;
            _ambientMode = RenderSettings.ambientMode;
            _ambientIntensity = RenderSettings.ambientIntensity;
        }

        public void Apply(MarketingLightingStyle style)
        {
            Capture();
            if (style == MarketingLightingStyle.Unchanged) return;

            Color sunColor;
            float sunIntensity;
            Quaternion sunRot;
            switch (style)
            {
                case MarketingLightingStyle.Dawn:
                    sunColor = new Color(1f, 0.78f, 0.62f); sunIntensity = 0.9f;
                    sunRot = Quaternion.Euler(18f, -30f, 0f); break;
                case MarketingLightingStyle.Morning:
                    sunColor = new Color(1f, 0.93f, 0.82f); sunIntensity = 1.1f;
                    sunRot = Quaternion.Euler(38f, -20f, 0f); break;
                case MarketingLightingStyle.GoldenHour:
                    sunColor = new Color(1f, 0.72f, 0.45f); sunIntensity = 1.05f;
                    sunRot = Quaternion.Euler(14f, 40f, 0f); break;
                case MarketingLightingStyle.Dusk:
                    sunColor = new Color(0.95f, 0.6f, 0.45f); sunIntensity = 0.75f;
                    sunRot = Quaternion.Euler(10f, 60f, 0f); break;
                case MarketingLightingStyle.Overcast:
                    sunColor = new Color(0.85f, 0.9f, 0.95f); sunIntensity = 0.65f;
                    sunRot = Quaternion.Euler(60f, 0f, 0f); break;
                default: // Day
                    sunColor = new Color(1f, 0.97f, 0.9f); sunIntensity = 1.15f;
                    sunRot = Quaternion.Euler(50f, -30f, 0f); break;
            }

            foreach (var s in _originals)
            {
                if (s.light == null) continue;
                s.light.color = sunColor;
                s.light.intensity = sunIntensity;
                s.light.transform.rotation = sunRot;
            }
        }

        public void Restore()
        {
            foreach (var s in _originals)
            {
                if (s.light == null) continue;
                s.light.color = s.color;
                s.light.intensity = s.intensity;
                s.light.transform.rotation = s.rotation;
                s.light.shadowStrength = s.shadowStrength;
            }
            if (_captured)
            {
                RenderSettings.ambientLight = _ambientColor;
                RenderSettings.ambientMode = _ambientMode;
                RenderSettings.ambientIntensity = _ambientIntensity;
            }
            _originals.Clear();
            _captured = false;
        }
    }
}
