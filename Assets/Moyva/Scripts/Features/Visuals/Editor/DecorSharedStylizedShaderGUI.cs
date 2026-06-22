using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Visuals.Editor
{
    public sealed class DecorSharedStylizedShaderGUI : ShaderGUI
    {
        private const float CardMode = 1f;
        private const float MeshMode = 0f;

        private MaterialEditor _materialEditor;
        private MaterialProperty[] _properties;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            _materialEditor = materialEditor;
            _properties = properties;
            EnsureRenderState(materialEditor.targets);

            EditorGUILayout.LabelField("Moyva Decor Shared Stylized", EditorStyles.boldLabel);
            DrawPresetBar(materialEditor.targets);
            EditorGUILayout.Space(4f);

            DrawSurface();
            DrawBillboard();
            DrawTextureFit();
            DrawLighting();
            DrawStylization();
            DrawTextureVolume();
            DrawOutline();
            DrawContactShadow();
        }

        private void DrawPresetBar(Object[] targets)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Preset: Card / Grass"))
                {
                    ApplyPreset(targets, ApplyCardPreset, "Apply Decor Card Preset");
                }

                if (GUILayout.Button("Preset: Mesh Decor"))
                {
                    ApplyPreset(targets, ApplyMeshPreset, "Apply Decor Mesh Preset");
                }

                if (GUILayout.Button("Preset: Billboard Card"))
                {
                    ApplyPreset(targets, ApplyBillboardCardPreset, "Apply Decor Billboard Card Preset");
                }
            }
        }

        private void DrawSurface()
        {
            DrawSection("Surface");
            MaterialProperty baseMap = Prop("_BaseMap");
            MaterialProperty baseColor = Prop("_BaseColor");
            if (baseMap != null)
            {
                _materialEditor.TexturePropertySingleLine(new GUIContent("Base Map / Color"), baseMap, baseColor);
                _materialEditor.TextureScaleOffsetProperty(baseMap);
            }
            else
            {
                Draw("_BaseColor", "Base Color");
            }

            Draw("_Alpha", "Alpha");
            bool alphaClipEnabled = DrawStrength("_AlphaClipEnabled", "Alpha Clip Strength");
            if (alphaClipEnabled)
                Draw("_AlphaClipThreshold", "Clip Threshold");

            Draw("_CullMode", "Cull Mode");
        }

        private void DrawBillboard()
        {
            DrawSection("Billboard");
            MaterialProperty billboard = Prop("_BillboardEnabled");
            if (billboard == null)
                return;

            _materialEditor.ShaderProperty(
                billboard,
                new GUIContent(
                    "Face Camera Billboard",
                    "Rotates the card vertices around world up so the texture keeps facing the camera."));

            MaterialProperty cull = Prop("_CullMode");
            if (billboard.hasMixedValue || billboard.floatValue <= 0.5f || cull == null || cull.hasMixedValue)
                return;

            if (!Mathf.Approximately(cull.floatValue, (float)CullMode.Off))
            {
                EditorGUILayout.HelpBox(
                    "Billboard cards should usually use Cull Off so they stay visible from every camera angle.",
                    MessageType.Info);
            }
        }

        private void DrawTextureFit()
        {
            DrawSection("Texture Fit / Size");
            DrawPositiveVector2(
                "_TextureFill",
                "Texture Fill XY",
                "Збільшує або зменшує видиму текстуру всередині mesh без зміни самого mesh. Значення більше 1 робить texture content більшим.");
            DrawVector2(
                "_TextureFillOffset",
                "Texture Offset XY",
                "Зміщує fitted texture content по UV, якщо альфа-силует у texture не по центру.");
            Draw(
                "_TextureFitClamp",
                "Clamp Outside Fill",
                "Коли увімкнено, shader не повторює texture за межами fitted UV, а робить alpha прозорою.");
        }

        private void DrawLighting()
        {
            DrawSection("Soft Lighting");
            Draw("_AmbientStrength", "Ambient Strength");
            Draw("_LightStrength", "Main Light Strength");
            Draw("_MinimumBrightness", "Minimum Brightness");
            Draw("_ShadowTint", "Self Shadow Tint");
            Draw("_ShadowSoftness", "Self Shadow Softness");
        }

        private void DrawStylization()
        {
            DrawSection("Stylization");
            Draw("_TextureSaturation", "Texture Saturation");
            Draw("_TextureContrast", "Texture Contrast");
            bool posterizeEnabled = DrawStrength("_PosterizeStrength", "Posterize Strength");
            if (posterizeEnabled)
                DrawInt("_ColorPosterizeSteps", "Color Posterize Steps", 2, 12);

            bool lightStepsEnabled = DrawStrength("_LightStepStrength", "Light Step Strength");
            if (lightStepsEnabled)
                DrawInt("_LightStepCount", "Light Step Count", 2, 6);

            EditorGUILayout.Space(2f);
            bool rimEnabled = DrawStrength("_RimEnabled", "Rim Light Strength");
            if (rimEnabled)
            {
                Draw("_StylizedRimColor", "Rim Color");
                Draw("_RimPower", "Rim Power");
            }

            EditorGUILayout.Space(2f);
            bool leafEnabled = DrawStrength("_LeafPlaneShading", "Leaf Plane Shading Strength");
            if (leafEnabled)
            {
                DrawVector2("_LeafPlaneDirection", "Leaf Plane Direction UV");
                Draw("_LeafShadeStrength", "Leaf Shade Strength");
                Draw("_LeafLightStrength", "Leaf Light Strength");
                Draw("_LeafPlaneSoftness", "Leaf Plane Softness");
                Draw("_LeafPlaneBalance", "Leaf Plane Balance");
            }
        }

        private void DrawTextureVolume()
        {
            DrawSection("Texture Volume");
            bool volumeEnabled = DrawStrength(
                "_TextureVolumeStrength",
                "Volume Strength",
                "Додає псевдо-об'єм на плоскій texture: одна сторона темніша, інша світліша, без зміни mesh.");
            if (!volumeEnabled)
                return;

            Draw("_TextureVolumeRoundness", "Volume Roundness", "Наскільки округлим виглядає світло/тінь на texture.");
            Draw("_TextureVolumeLightColor", "Volume Light Color", "Колір м'якого стилізованого підсвічування.");
            Draw("_TextureVolumeShadowColor", "Volume Shadow Color", "Колір м'якого затемнення всередині texture.");
            DrawVector2("_TextureVolumeDirection", "Volume Direction UV", "Напрямок світлої сторони у UV-просторі texture.");
        }

        private void DrawOutline()
        {
            DrawSection("Outline");
            bool outlineEnabled = DrawStrength("_OutlineEnabled", "Outline Strength");
            if (!outlineEnabled)
                return;

            Draw("_OutlineColor", "Outline Color");
            Draw("_OutlineScreenWidthPx", "Screen Width Pixels");
        }

        private void DrawContactShadow()
        {
            DrawSection("Contact Shadow");
            bool shadowEnabled = DrawStrength("_ContactShadowEnabled", "Contact Shadow Strength");
            if (!shadowEnabled)
                return;

            Draw("_ContactBlobMode", "Mode");
            Draw("_ContactColor", "Shadow Color");
            Draw("_ContactDarkness", "Darkness");
            Draw("_ContactRadius", "Radius");
            Draw("_ContactSoftness", "Edge Softness");
            DrawVector2(
                "_ContactBlobAspect",
                "Blob Aspect XZ",
                "Форма овальної плями тіні. Тепер впливає і на Mesh Footprint, і на UV Card Blob.");

            MaterialProperty mode = Prop("_ContactBlobMode");
            bool mixedMode = mode != null && mode.hasMixedValue;
            bool cardMode = !mixedMode && mode != null && mode.floatValue >= 0.5f;

            EditorGUILayout.Space(2f);
            if (mixedMode)
            {
                EditorGUILayout.HelpBox("Contact Shadow Mode differs across selected materials. Set one mode to show mode-specific controls.", MessageType.Info);
                return;
            }

            if (cardMode)
            {
                Draw("_ContactCameraBackOffset", "Camera Back Offset");
            }
            else
            {
                Draw("_ContactProjectionScale", "Mesh Projection Scale");
                Draw("_ContactLocalY", "Contact Local Y");
                DrawVector2("_ContactOffsetOS", "Object XZ Offset");
            }
        }

        private void DrawSection(string label)
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        }

        private void Draw(string propertyName, string label, string tooltip = null)
        {
            MaterialProperty property = Prop(propertyName);
            if (property == null)
                return;

            _materialEditor.ShaderProperty(property, new GUIContent(label, tooltip));
        }

        private void DrawInt(string propertyName, string label, int min, int max, string tooltip = null)
        {
            MaterialProperty property = Prop(propertyName);
            if (property == null)
                return;

            int currentValue = Mathf.Clamp(Mathf.RoundToInt(property.floatValue), min, max);
            if (!property.hasMixedValue && !Mathf.Approximately(property.floatValue, currentValue))
                property.floatValue = currentValue;

            EditorGUI.showMixedValue = property.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            int value = EditorGUILayout.IntSlider(new GUIContent(label, tooltip), currentValue, min, max);
            if (EditorGUI.EndChangeCheck())
                property.floatValue = value;
            EditorGUI.showMixedValue = false;
        }

        private void DrawVector2(string propertyName, string label, string tooltip = null)
        {
            MaterialProperty property = Prop(propertyName);
            if (property == null)
                return;

            Vector4 current = property.vectorValue;
            EditorGUI.showMixedValue = property.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            Vector2 value = EditorGUILayout.Vector2Field(new GUIContent(label, tooltip), new Vector2(current.x, current.y));
            if (EditorGUI.EndChangeCheck())
                property.vectorValue = new Vector4(value.x, value.y, current.z, current.w);
            EditorGUI.showMixedValue = false;
        }

        private void DrawPositiveVector2(string propertyName, string label, string tooltip = null)
        {
            MaterialProperty property = Prop(propertyName);
            if (property == null)
                return;

            Vector4 current = property.vectorValue;
            EditorGUI.showMixedValue = property.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            Vector2 value = EditorGUILayout.Vector2Field(new GUIContent(label, tooltip), new Vector2(current.x, current.y));
            value.x = Mathf.Clamp(value.x, 0.05f, 5f);
            value.y = Mathf.Clamp(value.y, 0.05f, 5f);
            if (EditorGUI.EndChangeCheck())
                property.vectorValue = new Vector4(value.x, value.y, current.z, current.w);
            EditorGUI.showMixedValue = false;
        }

        private bool DrawStrength(string propertyName, string label, string tooltip = null)
        {
            MaterialProperty property = Prop(propertyName);
            if (property == null)
                return false;

            _materialEditor.ShaderProperty(property, new GUIContent(label, tooltip));
            return property.hasMixedValue || property.floatValue > 0.001f;
        }

        private MaterialProperty Prop(string propertyName)
        {
            return FindProperty(propertyName, _properties, false);
        }

        private static void ApplyPreset(Object[] targets, System.Action<Material> apply, string undoName)
        {
            Undo.RecordObjects(targets, undoName);
            foreach (Object target in targets)
            {
                Material material = target as Material;
                if (material == null)
                    continue;

                apply(material);
                EditorUtility.SetDirty(material);
            }
        }

        private static void ApplyCardPreset(Material material)
        {
            ApplyRenderState(material);
            SetFloat(material, "_BillboardEnabled", 0f);
            SetFloat(material, "_CullMode", 0f);
            SetFloat(material, "_ContactBlobMode", CardMode);
            SetFloat(material, "_ContactShadowEnabled", 1f);
            SetFloat(material, "_ContactDarkness", 0.09f);
            SetFloat(material, "_ContactRadius", 0.46f);
            SetFloat(material, "_ContactSoftness", 0.70f);
            SetVector(material, "_ContactBlobAspect", new Vector4(1.25f, 0.55f, 0f, 0f));
            SetFloat(material, "_ContactCameraBackOffset", 0.10f);
            SetFloat(material, "_OutlineEnabled", 1f);
            SetFloat(material, "_OutlineWidth", 0f);
            SetFloat(material, "_OutlineScreenWidthPx", 1.5f);
            SetFloat(material, "_AlphaOutlineWidth", 0f);
            SetFloat(material, "_AlphaOutlineScreenWidthPx", 0f);
            SetFloat(material, "_LeafPlaneShading", 1f);
            SetVector(material, "_TextureFill", new Vector4(1f, 1f, 0f, 0f));
            SetVector(material, "_TextureFillOffset", Vector4.zero);
            SetFloat(material, "_TextureFitClamp", 1f);
            SetFloat(material, "_TextureVolumeStrength", 0.35f);
            SetFloat(material, "_TextureVolumeRoundness", 0.62f);
            SetVector(material, "_TextureVolumeDirection", new Vector4(-0.45f, 0.75f, 0f, 0f));
        }

        private static void ApplyBillboardCardPreset(Material material)
        {
            ApplyCardPreset(material);
            SetFloat(material, "_BillboardEnabled", 1f);
            SetFloat(material, "_CullMode", (float)CullMode.Off);
        }

        private static void ApplyMeshPreset(Material material)
        {
            ApplyRenderState(material);
            SetFloat(material, "_BillboardEnabled", 0f);
            SetFloat(material, "_CullMode", 2f);
            SetFloat(material, "_ContactBlobMode", MeshMode);
            SetFloat(material, "_ContactShadowEnabled", 1f);
            SetFloat(material, "_ContactDarkness", 0.14f);
            SetFloat(material, "_ContactRadius", 0.55f);
            SetFloat(material, "_ContactSoftness", 0.68f);
            SetVector(material, "_ContactBlobAspect", new Vector4(1f, 1f, 0f, 0f));
            SetFloat(material, "_ContactProjectionScale", 1.2f);
            SetFloat(material, "_OutlineEnabled", 1f);
            SetFloat(material, "_OutlineWidth", 0f);
            SetFloat(material, "_OutlineScreenWidthPx", 1.5f);
            SetFloat(material, "_AlphaOutlineWidth", 0f);
            SetFloat(material, "_AlphaOutlineScreenWidthPx", 0f);
            SetFloat(material, "_LeafPlaneShading", 0f);
            SetVector(material, "_TextureFill", new Vector4(1f, 1f, 0f, 0f));
            SetVector(material, "_TextureFillOffset", Vector4.zero);
            SetFloat(material, "_TextureFitClamp", 1f);
            SetFloat(material, "_TextureVolumeStrength", 0.18f);
            SetFloat(material, "_TextureVolumeRoundness", 0.45f);
        }

        private static void SetFloat(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
                material.SetFloat(propertyName, value);
        }

        private static void SetVector(Material material, string propertyName, Vector4 value)
        {
            if (material.HasProperty(propertyName))
                material.SetVector(propertyName, value);
        }

        private static void EnsureRenderState(Object[] targets)
        {
            foreach (Object target in targets)
            {
                Material material = target as Material;
                if (material == null)
                    continue;

                bool changed = false;
                changed |= SetFloatIfDifferent(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
                changed |= SetFloatIfDifferent(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                changed |= SetFloatIfDifferent(material, "_ZWrite", 1f);
                changed |= SetRenderQueueIfDifferent(material, (int)RenderQueue.AlphaTest + 40);

                if (changed)
                    EditorUtility.SetDirty(material);
            }
        }

        private static void ApplyRenderState(Material material)
        {
            SetFloat(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
            SetFloat(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            SetFloat(material, "_ZWrite", 1f);
            material.renderQueue = (int)RenderQueue.AlphaTest + 40;
        }

        private static bool SetFloatIfDifferent(Material material, string propertyName, float value)
        {
            if (!material.HasProperty(propertyName))
                return false;
            if (Mathf.Approximately(material.GetFloat(propertyName), value))
                return false;

            material.SetFloat(propertyName, value);
            return true;
        }

        private static bool SetRenderQueueIfDifferent(Material material, int renderQueue)
        {
            if (material.renderQueue == renderQueue)
                return false;

            material.renderQueue = renderQueue;
            return true;
        }
    }
}
