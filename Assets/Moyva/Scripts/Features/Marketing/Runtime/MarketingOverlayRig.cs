using Kruty1918.Moyva.Marketing.Contracts;
using Kruty1918.Moyva.Marketing.Text;
using UnityEngine;

namespace Kruty1918.Moyva.Marketing.Runtime
{
    /// <summary>
    /// World-space overlay parented to the capture camera: marketing text
    /// (TextMesh — zero package dependencies, deterministic), logo quad and
    /// a transition fade quad. Everything renders inside the capture camera,
    /// so screenshots/video pick it up without UI framework dependencies.
    /// </summary>
    public sealed class MarketingOverlayRig
    {
        private readonly Camera _camera;
        private readonly Transform _root;
        private readonly TextMesh _text;
        private readonly TextMesh _subText;
        private readonly GameObject _logoQuad;
        private readonly GameObject _fadeQuad;
        private readonly float _distance = 1.2f;
        private Font _font;
        private Color _textColor = Color.white;
        private Color _shadowColor = new Color(0f, 0f, 0f, 0.75f);

        public MarketingOverlayRig(Camera camera, Transform parent, Font font,
            Color textColor, Color shadowColor)
        {
            _camera = camera;
            _font = font;
            _textColor = textColor;
            _shadowColor = shadowColor;

            var rootGo = new GameObject("MarketingOverlay");
            _root = rootGo.transform;
            _root.SetParent(parent, false);
            _root.localPosition = new Vector3(0f, 0f, _distance);

            _text = CreateText("MarketingText", _root, 0f);
            _subText = CreateText("MarketingSubText", _root, 0f);
            _logoQuad = CreateQuad("MarketingLogo", _root, new Color(1f, 1f, 1f, 0f));
            _fadeQuad = CreateQuad("MarketingFade", _root, new Color(0f, 0f, 0f, 0f));
            _fadeQuad.transform.localPosition = new Vector3(0f, 0f, 0.02f);
        }

        private TextMesh CreateText(string name, Transform parent, float z)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 0f, z + 0.05f);
            var tm = go.AddComponent<TextMesh>();
            tm.font = _font;
            if (_font != null)
                go.GetComponent<MeshRenderer>().sharedMaterial = _font.material;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.characterSize = 0.05f;
            tm.color = _textColor;
            tm.text = string.Empty;
            // Render on top of the world but behind nothing else.
            go.GetComponent<MeshRenderer>().sortingOrder = 100;
            return tm;
        }

        private GameObject CreateQuad(string name, Transform parent, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.zero;
            var renderer = go.GetComponent<MeshRenderer>();
            var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            var mat = new Material(shader);
            // URP/Unlit defaults to opaque — alpha is ignored unless the
            // surface is switched to transparent.
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            mat.color = color;
            renderer.sharedMaterial = mat;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            go.SetActive(color.a > 0f);
            return go;
        }

        public void SetFont(Font font)
        {
            _font = font;
            if (_text != null) { _text.font = font; if (font) _text.GetComponent<MeshRenderer>().sharedMaterial = font.material; }
            if (_subText != null) { _subText.font = font; if (font) _subText.GetComponent<MeshRenderer>().sharedMaterial = font.material; }
        }

        /// <summary>Show a text line in a template slot. Viewport rect →
        /// world-space placement in front of the camera.</summary>
        public void ShowText(string text, TextTemplate template, float aspect, float safeMargin)
        {
            var layout = MarketingTypography.Resolve(template, aspect, safeMargin);
            if (_text == null) return;
            _text.text = text ?? string.Empty;
            float h = 2f * _distance * Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float w = h * aspect;
            var r = layout.anchorRect;
            // Anchor at the center of the layout rect (lower-third → y<0)
            float cx = (r.xMin + r.xMax) * 0.5f - 0.5f;
            float cy = (r.yMin + r.yMax) * 0.5f - 0.5f;
            _text.transform.localPosition = new Vector3(cx * w, cy * h, 0.05f);
            _text.characterSize = 0.02f * layout.fontScale * (_distance / 1.2f) * (_camera != null ? _camera.fieldOfView / 50f : 1f) * 2.4f;
            _text.anchor = layout.anchor;
            _text.color = _textColor;
        }

        public void HideText()
        {
            if (_text != null) _text.text = string.Empty;
            if (_subText != null) _subText.text = string.Empty;
        }

        /// <summary>End card: game title + optional sub-line centered.</summary>
        public void ShowEndCard(string title, string subline, float aspect, float safeMargin)
        {
            ShowText(title, TextTemplate.EndCard, aspect, safeMargin);
            if (_subText == null || string.IsNullOrEmpty(subline)) return;
            var layout = MarketingTypography.Resolve(TextTemplate.EndCard, aspect, safeMargin);
            float h = 2f * _distance * Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float w = h * aspect;
            _subText.text = subline;
            _subText.transform.localPosition = new Vector3(0f, (layout.anchorRect.yMin - 0.5f) * h * 0.6f, 0.05f);
            _subText.characterSize = _text.characterSize * 0.45f;
            _subText.color = _textColor;
        }

        /// <summary>Logo quad at corner or center scale. Texture applied via
        /// material mainTexture.</summary>
        public void ShowLogo(Texture2D logo, bool centerCard, float aspect, float safeMargin)
        {
            if (_logoQuad == null) return;
            _logoQuad.SetActive(logo != null);
            if (logo == null) return;
            var mat = _logoQuad.GetComponent<MeshRenderer>().sharedMaterial;
            mat.mainTexture = logo;
            mat.color = Color.white;

            float h = 2f * _distance * Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float w = h * aspect;
            float logoH = h * (centerCard ? 0.28f : 0.10f);
            float logoW = logoH * (logo != null ? (float)logo.width / logo.height : 1f);
            _logoQuad.transform.localScale = new Vector3(logoW, logoH, 1f);
            _logoQuad.transform.localPosition = centerCard
                ? new Vector3(0f, h * 0.12f, 0.04f)
                : new Vector3((0.5f - safeMargin) * w - logoW * 0.6f, -(0.5f - safeMargin) * h + logoH * 0.6f, 0.04f);
        }

        public void HideLogo()
        {
            if (_logoQuad != null) _logoQuad.SetActive(false);
        }

        /// <summary>Transition fade quad alpha (0 = clear).</summary>
        public void SetFade(float alpha)
        {
            if (_fadeQuad == null) return;
            _fadeQuad.SetActive(alpha > 0.001f);
            if (!_fadeQuad.activeSelf) return;
            var mat = _fadeQuad.GetComponent<MeshRenderer>().sharedMaterial;
            mat.color = new Color(0f, 0f, 0f, Mathf.Clamp01(alpha));
            float h = 2f * _distance * Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float w = h * _camera.aspect;
            _fadeQuad.transform.localScale = new Vector3(w * 1.1f, h * 1.1f, 1f);
        }

        public void Dispose()
        {
            if (_root != null) Object.Destroy(_root.gameObject);
        }
    }
}
