using UnityEditor;
using UnityEngine;

public sealed class BadNorthWaterEditor : ShaderGUI
{
    private static bool _showColors = true;
    private static bool _showShoreDistance = true;
    private static bool _showShoreFoam = true;
    private static bool _showShoreLine = true;
    private static bool _showReflection = true;
    private static bool _showSurfaceRipple = true;
    private static bool _showDebug = true;

    private static readonly string[] ColorModeOptions = { "Linear", "Gradient Texture" };
    private static readonly string[] DebugModeOptions =
    {
        "Final Color",
        "Shore Distance",
        "Shore Foam Mask",
        "Shoreline Mask",
        "Surface Ripple Mask",
        "Reflection Only"
    };

    private MaterialEditor _editor;
    private MaterialProperty[] _properties;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        _editor = materialEditor;
        _properties = properties;

        Material material = materialEditor.target as Material;
        if (material == null)
        {
            return;
        }

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Bad North No-Depth Planar Water", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "This shader does not sample scene depth or opaque scene color. Shore foam is driven by a top-down planar shore-distance texture sampled in world XZ.",
            MessageType.None);

        DrawPresetButtons(materialEditor);
        DrawWarnings(material);

        EditorGUI.BeginChangeCheck();
        DrawColors();
        DrawShoreDistance();
        DrawShoreFoam();
        DrawShoreLine();
        DrawReflection();
        DrawSurfaceRipple();
        DrawDebug();

        if (EditorGUI.EndChangeCheck())
        {
            foreach (Object target in materialEditor.targets)
            {
                Material targetMaterial = target as Material;
                if (targetMaterial == null)
                {
                    continue;
                }

                SetupKeywords(targetMaterial);
                EditorUtility.SetDirty(targetMaterial);
            }
        }
    }

    private void DrawPresetButtons(MaterialEditor materialEditor)
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("Presets", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Bad North Soft"))
                {
                    ApplyPresetToTargets(materialEditor, ApplyBadNorthSoft);
                }

                if (GUILayout.Button("Subtle Mobile"))
                {
                    ApplyPresetToTargets(materialEditor, ApplySubtleMobile);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Debug Ripple"))
                {
                    ApplyPresetToTargets(materialEditor, ApplyDebugRipple);
                }

                if (GUILayout.Button("Reset Defaults"))
                {
                    ApplyPresetToTargets(materialEditor, ApplyResetDefaults);
                }
            }
        }
    }

    private static void ApplyPresetToTargets(MaterialEditor materialEditor, System.Action<Material> apply)
    {
        materialEditor.RegisterPropertyChangeUndo("Apply Bad North Water Preset");
        foreach (Object target in materialEditor.targets)
        {
            Material material = target as Material;
            if (material == null)
            {
                continue;
            }

            apply(material);
            SetupKeywords(material);
            EditorUtility.SetDirty(material);
        }
    }

    private void DrawWarnings(Material material)
    {
        float reflectionStrength = GetFloat(material, "_ReflectionStrength");
        if (reflectionStrength <= 0.0001f)
        {
            EditorGUILayout.HelpBox("Planar reflection is disabled.", MessageType.Warning);
        }

        if (!material.HasProperty("_WaterReflectionTexture"))
        {
            EditorGUILayout.HelpBox(
                "Reflection texture property is missing. Planar reflection scripts require _WaterReflectionTexture.",
                MessageType.Warning);
        }
        else if (reflectionStrength > 0.0001f && GetTexture(material, "_WaterReflectionTexture") == null)
        {
            EditorGUILayout.HelpBox(
                "No planar reflection texture is assigned on the material. This is okay only if a reflection renderer sets it globally or through a property block at runtime.",
                MessageType.Info);
        }

        if (GetFloat(material, "_ReflectionDistortion") > 0.012f || GetFloat(material, "_RefractionAmplitude") > 0.008f)
        {
            EditorGUILayout.HelpBox(
                "High reflection distortion or surface ripple amplitude can be noisy on mobile.",
                MessageType.Warning);
        }

        float useShoreDistance = Mathf.Max(GetFloat(material, "_UseShoreDistanceTexture"), GetFloat(material, "_UseShoreDistanceTex"));
        if (useShoreDistance > 0.001f && GetTexture(material, "_ShoreDistanceTex") == null)
        {
            EditorGUILayout.HelpBox(
                "No Planar Shore Distance texture is assigned. Shore foam and shoreline bands will stay hidden instead of falling back to scene depth.",
                MessageType.Warning);
        }

        if (GetFloat(material, "_ContactFoamStrength") > 0.85f || GetColor(material, "_ContactFoamColor").a > 0.75f)
        {
            EditorGUILayout.HelpBox(
                "Shore foam is strong. Keep alpha and strength moderate for a Bad North style.",
                MessageType.Info);
        }

        EditorGUILayout.HelpBox(
            "Depth Texture and Opaque Texture are not required by this shader.",
            MessageType.None);
    }

    private void DrawColors()
    {
        _showColors = BeginSection("Colors", _showColors);
        if (!_showColors) return;

        EnumPopup("_ColorMode", "Source", ColorModeOptions, "Choose a simple shallow/deep blend or a custom gradient texture.");
        int mode = Mathf.RoundToInt(GetProperty("_ColorMode").floatValue);
        if (mode == 0)
        {
            PropertyField("_ColorShallow", "Shallow", "First water color.");
            PropertyField("_ColorDeep", "Deep", "Second water color.");
        }
        else
        {
            TextureField("_ColorGradient", "Gradient", "Horizontal 1D gradient sampled by Color Blend.");
        }

        PropertyField("_ColorBlend", "Color Blend", "Static blend across shallow/deep colors or gradient. No scene depth is sampled.");
        EndSection();
    }

    private void DrawShoreDistance()
    {
        _showShoreDistance = BeginSection("Planar Shore Distance", _showShoreDistance);
        if (!_showShoreDistance) return;

        PropertyField("_UseShoreDistanceTexture", "Enabled", "Uses a top-down planar distance texture. R channel: 0 at shore, 1 far from shore.");
        TextureField("_ShoreDistanceTex", "Distance Texture", "Planar data texture generated from the map, not a camera depth texture.");
        PropertyField("_ShoreDistanceWorldScale", "World Scale", "World XZ to texture UV scale. Usually 1 / world width and 1 / world length.");
        PropertyField("_ShoreDistanceWorldOffset", "World Offset", "World XZ to texture UV offset.");
        EndSection();
    }

    private void DrawShoreFoam()
    {
        _showShoreFoam = BeginSection("Shore Foam", _showShoreFoam);
        if (!_showShoreFoam) return;

        PropertyField("_ContactFoamColor", "Color", "Off-white foam tint. Alpha controls opacity.");
        PropertyField("_ContactFoamWidth", "Width", "Normalized planar distance from shore covered by foam.");
        PropertyField("_ContactFoamSmoothness", "Smoothness", "Soft falloff from shore into open water.");
        PropertyField("_ContactFoamDissolve", "Breakup", "Breaks the foam with world-space noise.");
        PropertyField("_ContactFoamEdgeFade", "Edge Fade", "Adds softness to the outer foam edge.");
        PropertyField("_ContactFoamNoiseScale", "Noise Scale", "World-space scale for foam breakup.");
        PropertyField("_ContactFoamNoiseSpeed", "Noise Speed", "Slow foam breakup animation speed.");
        PropertyField("_ContactFoamDistortion", "Distortion", "How much world-space noise bends the planar shore band.");
        PropertyField("_ContactFoamStrength", "Strength", "Overall foam mask strength.");
        EndSection();
    }

    private void DrawShoreLine()
    {
        _showShoreLine = BeginSection("Shoreline Bands", _showShoreLine);
        if (!_showShoreLine) return;

        PropertyField("_ShoreLineColor", "Color", "Subtle band/stroke tint. Alpha controls opacity.");
        PropertyField("_ShoreLineDepth", "Range", "Normalized planar distance range where shoreline bands can appear.");
        PropertyField("_ShoreLineSpeed", "Speed", "Animates bands outward across the planar shore distance field.");
        PropertyField("_ShoreLineAmount", "Amount", "Number of bands within the range.");
        PropertyField("_ShoreLineThickness", "Thickness", "Soft width of each band.");
        PropertyField("_ShoreLineCenterMask", "Center Mask", "Normalized distance where bands start fading.");
        PropertyField("_ShoreLineCenterFade", "Center Fade", "Softness of the far fade.");
        PropertyField("_ShoreLineTrailFade", "Trail Fade", "Fades one side of each moving band.");
        PropertyField("_ShoreLineDissolve", "Breakup", "Breaks bands into painterly fragments.");
        PropertyField("_ShoreLineNoiseScale", "Noise Scale", "World-space scale for band breakup and wobble.");
        PropertyField("_ShoreLineNoiseSpeed", "Noise Speed", "Slow pan speed for shoreline noise.");
        PropertyField("_ShoreLineStrength", "Strength", "Overall shoreline band strength.");
        EndSection();
    }

    private void DrawReflection()
    {
        _showReflection = BeginSection("Reflection", _showReflection);
        if (!_showReflection) return;

        TextureField("_WaterReflectionTexture", "Planar Texture", "Runtime texture set by the planar reflection renderer.");
        PropertyField("_ReflectionStrength", "Planar Strength", "Strength of island/object planar reflection.");
        PropertyField("_ReflectionDistortion", "Planar Distortion", "Small projected-UV distortion. Keep low on mobile.");
        PropertyField("_ReflectionFresnelPower", "Fresnel Power", "Higher values keep planar reflection softer and more visible at grazing angles only.");
        PropertyField("_ReflectionEdgeFade", "Edge Fade", "Soft guard around projected reflection UVs; keep small to avoid visible screen-edge borders.");
        PropertyField("_ReflectionVerticalFlip", "Vertical Flip", "Toggle if the reflection render texture is upside down.");
        PropertyField("_SkyboxReflectionStrength", "Skybox Strength", "Environment reflection amount for open water and grazing angles.");
        PropertyField("_SkyboxReflectionTint", "Skybox Tint", "Multiplies the real environment reflection.");
        PropertyField("_SkyboxReflectionRoughness", "Skybox Roughness", "Higher values give softer stylized sky reflection.");
        EndSection();
    }

    private void DrawSurfaceRipple()
    {
        _showSurfaceRipple = BeginSection("Surface Ripple", _showSurfaceRipple);
        if (!_showSurfaceRipple) return;

        PropertyField("_RefractionAmplitude", "Amplitude", "Surface-only shimmer/reflection distortion. Does not sample opaque scene color or depth.");
        EndSection();
    }

    private void DrawDebug()
    {
        _showDebug = BeginSection("Debug", _showDebug);
        if (!_showDebug) return;

        EnumPopup("_DebugMode", "Debug Mode", DebugModeOptions, "Visualize planar shore distance, foam, shoreline bands, surface ripple, or reflection only.");
        EditorGUILayout.HelpBox(
            "Mobile path: no scene depth sample and no opaque scene color. Shore masks come only from _ShoreDistanceTex.",
            MessageType.None);
        EndSection();
    }

    private bool BeginSection(string title, bool state)
    {
        EditorGUILayout.Space(6);
        state = EditorGUILayout.Foldout(state, title, true, EditorStyles.boldLabel);
        if (state)
        {
            EditorGUI.indentLevel++;
        }

        return state;
    }

    private static void EndSection()
    {
        EditorGUI.indentLevel--;
    }

    private MaterialProperty GetProperty(string propertyName)
    {
        return FindProperty(propertyName, _properties);
    }

    private MaterialProperty FindOptional(string propertyName)
    {
        return FindProperty(propertyName, _properties, false);
    }

    private void PropertyField(string propertyName, string label, string tooltip)
    {
        MaterialProperty property = FindOptional(propertyName);
        if (property != null)
        {
            _editor.ShaderProperty(property, new GUIContent(label, tooltip));
        }
    }

    private void TextureField(string propertyName, string label, string tooltip)
    {
        MaterialProperty property = FindOptional(propertyName);
        if (property != null)
        {
            _editor.TexturePropertySingleLine(new GUIContent(label, tooltip), property);
        }
    }

    private void EnumPopup(string propertyName, string label, string[] options, string tooltip)
    {
        MaterialProperty property = FindOptional(propertyName);
        if (property == null)
        {
            return;
        }

        EditorGUI.showMixedValue = property.hasMixedValue;
        EditorGUI.BeginChangeCheck();
        int value = Mathf.Clamp(Mathf.RoundToInt(property.floatValue), 0, options.Length - 1);
        value = EditorGUILayout.Popup(new GUIContent(label, tooltip), value, options);
        if (EditorGUI.EndChangeCheck())
        {
            property.floatValue = value;
        }
        EditorGUI.showMixedValue = false;
    }

    private static void SetupKeywords(Material material)
    {
        int colorMode = Mathf.RoundToInt(GetFloat(material, "_ColorMode"));
        SetKeyword(material, "_COLORMODE_LINEAR", colorMode == 0);
        SetKeyword(material, "_COLORMODE_GRADIENT_TEXTURE", colorMode == 1);
    }

    private static void SetKeyword(Material material, string keyword, bool enabled)
    {
        if (enabled)
        {
            material.EnableKeyword(keyword);
        }
        else
        {
            material.DisableKeyword(keyword);
        }
    }

    private static void ApplyBadNorthSoft(Material material)
    {
        SetFloat(material, "_ColorMode", 0f);
        SetColor(material, "_ColorShallow", new Color(0.56f, 0.71f, 0.72f, 0.82f));
        SetColor(material, "_ColorDeep", new Color(0.31f, 0.47f, 0.52f, 1f));
        SetFloat(material, "_ColorBlend", 0.58f);
        SetFloat(material, "_UseShoreDistanceTexture", 1f);
        SetFloat(material, "_UseShoreDistanceTex", 1f);
        SetVector(material, "_ShoreDistanceWorldScale", new Vector4(1f, 1f, 0f, 0f));
        SetVector(material, "_ShoreDistanceWorldOffset", Vector4.zero);
        SetColor(material, "_ContactFoamColor", new Color(0.88f, 0.91f, 0.88f, 0.42f));
        SetFloat(material, "_ContactFoamWidth", 0.10f);
        SetFloat(material, "_ContactFoamSmoothness", 0.18f);
        SetFloat(material, "_ContactFoamDissolve", 0.42f);
        SetFloat(material, "_ContactFoamEdgeFade", 0.35f);
        SetFloat(material, "_ContactFoamNoiseScale", 4.8f);
        SetFloat(material, "_ContactFoamNoiseSpeed", 0.035f);
        SetFloat(material, "_ContactFoamDistortion", 0.35f);
        SetFloat(material, "_ContactFoamStrength", 0.8f);
        SetColor(material, "_ShoreLineColor", new Color(0.78f, 0.86f, 0.86f, 0.18f));
        SetFloat(material, "_ShoreLineDepth", 0.75f);
        SetFloat(material, "_ShoreLineSpeed", 0.055f);
        SetFloat(material, "_ShoreLineAmount", 7.0f);
        SetFloat(material, "_ShoreLineThickness", 0.13f);
        SetFloat(material, "_ShoreLineCenterMask", 0.72f);
        SetFloat(material, "_ShoreLineCenterFade", 0.28f);
        SetFloat(material, "_ShoreLineTrailFade", 0.45f);
        SetFloat(material, "_ShoreLineDissolve", 0.54f);
        SetFloat(material, "_ShoreLineNoiseScale", 3.2f);
        SetFloat(material, "_ShoreLineNoiseSpeed", 0.025f);
        SetFloat(material, "_ShoreLineStrength", 1f);
        SetFloat(material, "_ReflectionStrength", 0.06f);
        SetFloat(material, "_ReflectionDistortion", 0.003f);
        SetFloat(material, "_ReflectionFresnelPower", 4f);
        SetFloat(material, "_ReflectionEdgeFade", 3f);
        SetFloat(material, "_ReflectionVerticalFlip", 0f);
        SetFloat(material, "_SkyboxReflectionStrength", 0.05f);
        SetColor(material, "_SkyboxReflectionTint", Color.white);
        SetFloat(material, "_SkyboxReflectionRoughness", 0.75f);
        SetFloat(material, "_RefractionAmplitude", 0.0015f);
        SetFloat(material, "_DebugMode", 0f);
    }

    private static void ApplySubtleMobile(Material material)
    {
        SetFloat(material, "_ColorMode", 0f);
        SetColor(material, "_ColorShallow", new Color(0.53f, 0.68f, 0.72f, 0.82f));
        SetColor(material, "_ColorDeep", new Color(0.30f, 0.45f, 0.54f, 1f));
        SetFloat(material, "_ColorBlend", 0.52f);
        SetFloat(material, "_UseShoreDistanceTexture", 1f);
        SetFloat(material, "_UseShoreDistanceTex", 1f);
        SetVector(material, "_ShoreDistanceWorldScale", new Vector4(1f, 1f, 0f, 0f));
        SetVector(material, "_ShoreDistanceWorldOffset", Vector4.zero);
        SetColor(material, "_ContactFoamColor", new Color(0.86f, 0.90f, 0.88f, 0.30f));
        SetFloat(material, "_ContactFoamWidth", 0.08f);
        SetFloat(material, "_ContactFoamSmoothness", 0.22f);
        SetFloat(material, "_ContactFoamDissolve", 0.58f);
        SetFloat(material, "_ContactFoamEdgeFade", 0.55f);
        SetFloat(material, "_ContactFoamNoiseScale", 4.2f);
        SetFloat(material, "_ContactFoamNoiseSpeed", 0.025f);
        SetFloat(material, "_ContactFoamDistortion", 0.26f);
        SetFloat(material, "_ContactFoamStrength", 0.55f);
        SetColor(material, "_ShoreLineColor", new Color(0.74f, 0.85f, 0.86f, 0.12f));
        SetFloat(material, "_ShoreLineDepth", 0.68f);
        SetFloat(material, "_ShoreLineSpeed", 0.04f);
        SetFloat(material, "_ShoreLineAmount", 5.5f);
        SetFloat(material, "_ShoreLineThickness", 0.09f);
        SetFloat(material, "_ShoreLineCenterMask", 0.68f);
        SetFloat(material, "_ShoreLineCenterFade", 0.34f);
        SetFloat(material, "_ShoreLineTrailFade", 0.55f);
        SetFloat(material, "_ShoreLineDissolve", 0.66f);
        SetFloat(material, "_ShoreLineNoiseScale", 2.7f);
        SetFloat(material, "_ShoreLineNoiseSpeed", 0.018f);
        SetFloat(material, "_ShoreLineStrength", 0.75f);
        SetFloat(material, "_ReflectionStrength", 0.04f);
        SetFloat(material, "_ReflectionDistortion", 0.002f);
        SetFloat(material, "_ReflectionFresnelPower", 4f);
        SetFloat(material, "_ReflectionEdgeFade", 3f);
        SetFloat(material, "_ReflectionVerticalFlip", 0f);
        SetFloat(material, "_SkyboxReflectionStrength", 0.035f);
        SetColor(material, "_SkyboxReflectionTint", Color.white);
        SetFloat(material, "_SkyboxReflectionRoughness", 0.82f);
        SetFloat(material, "_RefractionAmplitude", 0.001f);
        SetFloat(material, "_DebugMode", 0f);
    }

    private static void ApplyDebugRipple(Material material)
    {
        ApplyBadNorthSoft(material);
        SetFloat(material, "_DebugMode", 2f);
    }

    private static void ApplyResetDefaults(Material material)
    {
        ApplyBadNorthSoft(material);
    }

    private static float GetFloat(Material material, string name)
    {
        return material != null && material.HasProperty(name) ? material.GetFloat(name) : 0f;
    }

    private static Texture GetTexture(Material material, string name)
    {
        return material != null && material.HasProperty(name) ? material.GetTexture(name) : null;
    }

    private static Color GetColor(Material material, string name)
    {
        return material != null && material.HasProperty(name) ? material.GetColor(name) : Color.clear;
    }

    private static void SetFloat(Material material, string name, float value)
    {
        if (material.HasProperty(name))
        {
            material.SetFloat(name, value);
        }
    }

    private static void SetColor(Material material, string name, Color value)
    {
        if (material.HasProperty(name))
        {
            material.SetColor(name, value);
        }
    }

    private static void SetVector(Material material, string name, Vector4 value)
    {
        if (material.HasProperty(name))
        {
            material.SetVector(name, value);
        }
    }
}
