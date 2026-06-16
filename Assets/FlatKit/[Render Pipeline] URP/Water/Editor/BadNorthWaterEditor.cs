using UnityEngine;
using UnityEditor;

// Put this file inside an Editor folder, for example:
// Assets/Editor/BadNorthWaterEditor.cs
public class BadNorthWaterEditor : ShaderGUI
{
    private MaterialEditor _editor;
    private MaterialProperty[] _properties;

    private static bool _showColors = true;
    private static bool _showShore = true;
    private static bool _showVisibility = true;
    private static bool _showCrest = false;
    private static bool _showWaves = true;
    private static bool _showFoam = true;
    private static bool _showContactFoam = true;
    private static bool _showRefraction = false;
    private static bool _showReflection = true;
    private static bool _showTools = true;

    private static readonly string[] ColorSourceOptions = { "Linear", "Gradient Texture" };
    private static readonly string[] WaveShapeOptions = { "None", "Round", "Grid", "Pointy" };
    private static readonly string[] TilingSourceOptions = { "UV", "World Space" };
    private static readonly string[] FoamSourceOptions = { "None", "Gradient Noise", "Texture" };

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
        EditorGUILayout.LabelField("Bad North Water", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Stylized water setup for procedural islands: shallow/deep color, shore tint, ragged animated shoreline foam, contact foam, limited underwater visibility and planar reflections.",
            MessageType.None);

        EditorGUI.BeginChangeCheck();

        DrawTools(materialEditor);
        DrawColors();
        DrawShoreTint();
        DrawVisibility();
        DrawCrest();
        DrawWaves();
        DrawFoam();
        DrawContactFoam();
        DrawRefraction();
        DrawReflection();

        if (EditorGUI.EndChangeCheck())
        {
            foreach (Object target in materialEditor.targets)
            {
                if (target is Material targetMaterial)
                {
                    SetupKeywords(targetMaterial);
                    EditorUtility.SetDirty(targetMaterial);
                }
            }
        }
    }

    private void DrawTools(MaterialEditor materialEditor)
    {
        _showTools = BeginSection("Quick Tools", _showTools);
        if (!_showTools) return;

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Apply Bad North Base Preset"))
        {
            materialEditor.RegisterPropertyChangeUndo("Apply Bad North Base Preset");
            foreach (Object target in materialEditor.targets)
            {
                if (target is Material mat)
                {
                    ApplyBadNorthPreset(mat);
                    SetupKeywords(mat);
                    EditorUtility.SetDirty(mat);
                }
            }
        }

        if (GUILayout.Button("Debug Strong Reflection"))
        {
            materialEditor.RegisterPropertyChangeUndo("Debug Strong Reflection");
            foreach (Object target in materialEditor.targets)
            {
                if (target is Material mat)
                {
                    SetFloat(mat, "_ReflectionStrength", 0.35f);
                    SetFloat(mat, "_ReflectionDistortion", 0.0f);
                    SetFloat(mat, "_ReflectionFresnelPower", 1.0f);
                    SetFloat(mat, "_ReflectionDepthFade", 0.0f);
                    SetFloat(mat, "_ReflectionEdgeFade", 0.0f);
                    SetFloat(mat, "_ReflectionBackgroundFade", 1.0f);
                    EditorUtility.SetDirty(mat);
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox(
            "For normal gameplay, keep Reflection Strength low. Use Debug Strong Reflection only to verify that the planar reflection projection is aligned.",
            MessageType.Info);
        EndSection();
    }

    private void DrawColors()
    {
        _showColors = BeginSection("Colors / Depth", _showColors);
        if (!_showColors) return;

        EnumPopup("_ColorMode", "Source", ColorSourceOptions);
        int mode = Mathf.RoundToInt(Find("_ColorMode").floatValue);
        if (mode == 0)
        {
            ColorField("_ColorShallow", "Shallow");
            ColorField("_ColorDeep", "Deep");
        }
        else
        {
            TextureField("_ColorGradient", "Gradient");
        }

        FloatField("_FadeDistance", "Shallow Depth");
        FloatField("_WaterDepth", "Gradient Size");
        FloatField("_LightContribution", "Light Color Contribution");
        FloatField("_WaterClearness", "Transparency");
        FloatField("_OpenWaterDepth", "Open Water Depth");
        FloatField("_MaxWaterDepth", "Max Depth Clamp");
        FloatField("_ShadowStrength", "Shadow Strength");
        EndSection();
    }

    private void DrawShoreTint()
    {
        _showShore = BeginSection("Shore Tint", _showShore);
        if (!_showShore) return;

        ColorField("_ShoreTintColor", "Color");
        FloatField("_ShoreTintDistance", "Distance");
        FloatField("_ShoreTintSoftness", "Softness");
        FloatField("_ShoreTintStrength", "Strength");
        FloatField("_ShoreTintNoise", "Noise Amount");
        FloatField("_ShoreTintNoiseScale", "Noise Scale");
        EndSection();
    }

    private void DrawVisibility()
    {
        _showVisibility = BeginSection("Underwater Visibility", _showVisibility);
        if (!_showVisibility) return;

        FloatField("_VisibilityDepth", "Visibility Depth");
        FloatField("_VisibilityFalloff", "Visibility Falloff");
        EditorGUILayout.HelpBox("Lower Visibility Depth if submerged objects are visible too far below the water surface.", MessageType.None);
        EndSection();
    }

    private void DrawCrest()
    {
        _showCrest = BeginSection("Crest", _showCrest);
        if (!_showCrest) return;

        ColorField("_CrestColor", "Color");
        FloatField("_CrestSize", "Size");
        FloatField("_CrestSharpness", "Sharp transition");
        EndSection();
    }

    private void DrawWaves()
    {
        _showWaves = BeginSection("Wave Geometry", _showWaves);
        if (!_showWaves) return;

        EnumPopup("_WaveMode", "Shape", WaveShapeOptions);
        int mode = Mathf.RoundToInt(Find("_WaveMode").floatValue);
        if (mode != 0)
        {
            FloatField("_WaveSpeed", "Speed");
            FloatField("_WaveAmplitude", "Amplitude");
            FloatField("_WaveFrequency", "Frequency");
            FloatField("_WaveDirection", "Direction");
            EnumPopup("_NoiseSource", "Tiling Source", TilingSourceOptions);
            FloatField("_WaveNoise", "Noise");
        }
        EndSection();
    }

    private void DrawFoam()
    {
        _showFoam = BeginSection("Foam", _showFoam);
        if (!_showFoam) return;

        EnumPopup("_FoamMode", "Source", FoamSourceOptions);
        int mode = Mathf.RoundToInt(Find("_FoamMode").floatValue);
        if (mode != 0)
        {
            if (mode == 2)
            {
                TextureField("_NoiseMap", "Texture");
            }
            ColorField("_FoamColor", "Color");
            FloatField("_FoamDepth", "Shore Depth");
            FloatField("_FoamNoiseAmount", "Shore Blending");
            FloatField("_FoamAmount", "Amount");
            FloatField("_FoamScale", "Scale");
            FloatField("_FoamStretchX", "Stretch X");
            FloatField("_FoamStretchY", "Stretch Y");
            FloatField("_FoamSharpness", "Sharpness");
            FloatField("_FoamSpeed", "Speed");
            FloatField("_FoamDirection", "Direction");
            FloatField("_FoamEdgeWobble", "Edge Wobble");
            FloatField("_FoamEdgeWobbleScale", "Edge Wobble Scale");
            FloatField("_FoamBrokenness", "Brokenness");
            FloatField("_FoamBlobAmount", "Blob Amount");
            FloatField("_FoamBlobScale", "Blob Scale");
            FloatField("_FoamBlobDistance", "Blob Distance");
            EditorGUILayout.HelpBox("For a Bad North shoreline: keep Amount low, use Edge Wobble/Brokenness for a torn edge, and Blob Amount for small rounded foam pieces near the shore.", MessageType.None);
        }
        EndSection();
    }

    private void DrawContactFoam()
    {
        _showContactFoam = BeginSection("Contact Foam", _showContactFoam);
        if (!_showContactFoam) return;

        ColorField("_ContactFoamColor", "Color");
        FloatField("_ContactFoamDistance", "Distance");
        FloatField("_ContactFoamSoftness", "Softness");
        FloatField("_ContactFoamEdgeSize", "Edge Size");
        FloatField("_ContactFoamEdgeThreshold", "Edge Threshold");
        FloatField("_ContactFoamEdgeSoftness", "Edge Softness");
        FloatField("_ContactFoamNoiseScale", "Noise Scale");
        FloatField("_ContactFoamNoiseAmount", "Noise Amount");
        FloatField("_ContactFoamSpeed", "Speed");
        FloatField("_ContactFoamStrength", "Strength");
        EditorGUILayout.HelpBox("If foam appears on too much of the submerged object, raise Edge Threshold or lower Distance.", MessageType.None);
        EndSection();
    }

    private void DrawRefraction()
    {
        _showRefraction = BeginSection("Refraction", _showRefraction);
        if (!_showRefraction) return;

        FloatField("_RefractionFrequency", "Frequency");
        FloatField("_RefractionAmplitude", "Amplitude");
        FloatField("_RefractionSpeed", "Speed");
        FloatField("_RefractionScale", "Scale");
        EndSection();
    }

    private void DrawReflection()
    {
        _showReflection = BeginSection("Reflection", _showReflection);
        if (!_showReflection) return;

        TextureField("_WaterReflectionTexture", "Texture");
        FloatField("_ReflectionStrength", "Strength");
        FloatField("_ReflectionDistortion", "Distortion");
        FloatField("_ReflectionFresnelPower", "Fresnel Power");
        FloatField("_ReflectionDepthFade", "Depth Fade");
        FloatField("_ReflectionEdgeFade", "Edge Fade");
        FloatField("_ReflectionBackgroundFade", "Background Fade");
        FloatField("_ReflectionVerticalFlip", "Vertical Flip");
        EndSection();
    }

    private bool BeginSection(string title, bool state)
    {
        EditorGUILayout.Space(6);
        Rect rect = EditorGUILayout.GetControlRect(false, 22f);
        rect.x += 2f;
        rect.width -= 4f;
        EditorGUI.DrawRect(rect, new Color(0.18f, 0.22f, 0.25f, 0.22f));
        Rect foldoutRect = new Rect(rect.x + 8f, rect.y + 2f, rect.width - 16f, rect.height - 4f);
        state = EditorGUI.Foldout(foldoutRect, state, title, true, EditorStyles.boldLabel);
        if (state)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(3);
        }
        return state;
    }

    private void EndSection()
    {
        EditorGUI.indentLevel--;
    }

    private MaterialProperty Find(string propertyName)
    {
        return FindProperty(propertyName, _properties);
    }

    private void ColorField(string propertyName, string label)
    {
        MaterialProperty property = Find(propertyName);
        _editor.ColorProperty(property, label);
    }

    private void FloatField(string propertyName, string label)
    {
        MaterialProperty property = Find(propertyName);
        _editor.ShaderProperty(property, label);
    }

    private void TextureField(string propertyName, string label)
    {
        MaterialProperty property = Find(propertyName);
        _editor.TexturePropertySingleLine(new GUIContent(label), property);
    }

    private void EnumPopup(string propertyName, string label, string[] options)
    {
        MaterialProperty property = Find(propertyName);
        EditorGUI.showMixedValue = property.hasMixedValue;
        EditorGUI.BeginChangeCheck();
        int value = Mathf.Clamp(Mathf.RoundToInt(property.floatValue), 0, options.Length - 1);
        value = EditorGUILayout.Popup(label, value, options);
        if (EditorGUI.EndChangeCheck())
        {
            property.floatValue = value;
        }
        EditorGUI.showMixedValue = false;
    }

    private static void SetupKeywords(Material material)
    {
        int colorMode = Mathf.RoundToInt(material.GetFloat("_ColorMode"));
        SetKeyword(material, "_COLORMODE_LINEAR", colorMode == 0);
        SetKeyword(material, "_COLORMODE_GRADIENT_TEXTURE", colorMode == 1);

        int foamMode = Mathf.RoundToInt(material.GetFloat("_FoamMode"));
        SetKeyword(material, "_FOAMMODE_NONE", foamMode == 0);
        SetKeyword(material, "_FOAMMODE_GRADIENT_NOISE", foamMode == 1);
        SetKeyword(material, "_FOAMMODE_TEXTURE", foamMode == 2);

        int waveMode = Mathf.RoundToInt(material.GetFloat("_WaveMode"));
        SetKeyword(material, "_WAVEMODE_NONE", waveMode == 0);
        SetKeyword(material, "_WAVEMODE_ROUND", waveMode == 1);
        SetKeyword(material, "_WAVEMODE_GRID", waveMode == 2);
        SetKeyword(material, "_WAVEMODE_POINTY", waveMode == 3);

        int noiseSource = Mathf.RoundToInt(material.GetFloat("_NoiseSource"));
        SetKeyword(material, "_NOISESOURCE_WORLD_SPACE", noiseSource == 1);
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

    private static void ApplyBadNorthPreset(Material mat)
    {
        SetFloat(mat, "_ColorMode", 0f);
        SetColor(mat, "_ColorShallow", new Color(0.49f, 0.67f, 0.73f, 0.82f));
        SetColor(mat, "_ColorDeep", new Color(0.30f, 0.44f, 0.57f, 1.00f));
        SetFloat(mat, "_FadeDistance", 0.35f);
        SetFloat(mat, "_WaterDepth", 5.5f);
        SetFloat(mat, "_LightContribution", 0.08f);
        SetFloat(mat, "_WaterClearness", 0.16f);
        SetFloat(mat, "_OpenWaterDepth", 4.0f);
        SetFloat(mat, "_MaxWaterDepth", 10.0f);
        SetFloat(mat, "_ShadowStrength", 0.18f);

        SetColor(mat, "_ShoreTintColor", new Color(0.73f, 0.86f, 0.86f, 0.59f));
        SetFloat(mat, "_ShoreTintDistance", 1.25f);
        SetFloat(mat, "_ShoreTintSoftness", 0.85f);
        SetFloat(mat, "_ShoreTintStrength", 0.38f);
        SetFloat(mat, "_ShoreTintNoise", 0.22f);
        SetFloat(mat, "_ShoreTintNoiseScale", 5.5f);

        SetFloat(mat, "_VisibilityDepth", 0.22f);
        SetFloat(mat, "_VisibilityFalloff", 3.2f);

        SetColor(mat, "_CrestColor", new Color(0.92f, 0.96f, 0.94f, 0.43f));
        SetFloat(mat, "_CrestSize", 0.025f);
        SetFloat(mat, "_CrestSharpness", 0.15f);

        SetFloat(mat, "_WaveMode", 2f);
        SetFloat(mat, "_WaveSpeed", 0.18f);
        SetFloat(mat, "_WaveAmplitude", 0.035f);
        SetFloat(mat, "_WaveFrequency", 0.75f);
        SetFloat(mat, "_WaveDirection", 0.15f);
        SetFloat(mat, "_NoiseSource", 1f);
        SetFloat(mat, "_WaveNoise", 0.12f);

        SetFloat(mat, "_FoamMode", 1f);
        SetColor(mat, "_FoamColor", new Color(0.96f, 0.96f, 0.92f, 0.86f));
        SetFloat(mat, "_FoamDepth", 0.65f);
        SetFloat(mat, "_FoamNoiseAmount", 0.55f);
        SetFloat(mat, "_FoamAmount", 0.18f);
        SetFloat(mat, "_FoamScale", 0.8f);
        SetFloat(mat, "_FoamStretchX", 1f);
        SetFloat(mat, "_FoamStretchY", 1f);
        SetFloat(mat, "_FoamSharpness", 0.32f);
        SetFloat(mat, "_FoamSpeed", 0.055f);
        SetFloat(mat, "_FoamDirection", 0.1f);
        SetFloat(mat, "_FoamEdgeWobble", 0.28f);
        SetFloat(mat, "_FoamEdgeWobbleScale", 6.0f);
        SetFloat(mat, "_FoamBrokenness", 0.38f);
        SetFloat(mat, "_FoamBlobAmount", 0.32f);
        SetFloat(mat, "_FoamBlobScale", 3.5f);
        SetFloat(mat, "_FoamBlobDistance", 1.25f);

        SetColor(mat, "_ContactFoamColor", new Color(0.94f, 0.96f, 0.93f, 0.88f));
        SetFloat(mat, "_ContactFoamDistance", 0.24f);
        SetFloat(mat, "_ContactFoamSoftness", 0.09f);
        SetFloat(mat, "_ContactFoamEdgeSize", 1.6f);
        SetFloat(mat, "_ContactFoamEdgeThreshold", 0.035f);
        SetFloat(mat, "_ContactFoamEdgeSoftness", 0.075f);
        SetFloat(mat, "_ContactFoamNoiseScale", 8.5f);
        SetFloat(mat, "_ContactFoamNoiseAmount", 0.55f);
        SetFloat(mat, "_ContactFoamSpeed", 0.16f);
        SetFloat(mat, "_ContactFoamStrength", 0.8f);

        SetFloat(mat, "_RefractionFrequency", 24f);
        SetFloat(mat, "_RefractionAmplitude", 0.0025f);
        SetFloat(mat, "_RefractionSpeed", 0.045f);
        SetFloat(mat, "_RefractionScale", 1.2f);

        SetFloat(mat, "_ReflectionStrength", 0.07f);
        SetFloat(mat, "_ReflectionDistortion", 0.004f);
        SetFloat(mat, "_ReflectionFresnelPower", 4f);
        SetFloat(mat, "_ReflectionDepthFade", 1f);
        SetFloat(mat, "_ReflectionEdgeFade", 0.08f);
        SetFloat(mat, "_ReflectionBackgroundFade", 1f);
        SetFloat(mat, "_ReflectionVerticalFlip", 0f);
    }

    private static void SetFloat(Material mat, string name, float value)
    {
        if (mat.HasProperty(name)) mat.SetFloat(name, value);
    }

    private static void SetColor(Material mat, string name, Color value)
    {
        if (mat.HasProperty(name)) mat.SetColor(name, value);
    }
}