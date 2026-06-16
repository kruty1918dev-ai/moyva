Shader "FlatKit/Water"
{
    Properties
    {
        [KeywordEnum(Linear, Gradient Texture)] _ColorMode ("[FOLDOUT(Colors){11}]Source{Colors}", Float) = 0.0
        _ColorShallow ("[_COLORMODE_LINEAR]Shallow", Color) = (0.35, 0.6, 0.75, 0.8) // Color alpha controls opacity
        _ColorDeep ("[_COLORMODE_LINEAR]Deep", Color) = (0.65, 0.9, 1.0, 1.0)
        [NoScaleOffset] _ColorGradient("[_COLORMODE_GRADIENT_TEXTURE]Gradient", 2D) = "white" {}
        _FadeDistance("Shallow Depth", Float) = 0.5
        _WaterDepth("Gradient Size", Float) = 5.0
        _OpenWaterDepth("Open Water Depth", Float) = 4.0
        _MaxWaterDepth("Max Depth Clamp", Float) = 20.0
        _LightContribution("Light Color Contribution", Range(0, 1)) = 0
        _WaterClearness("Transparency", Range(0, 1)) = 0.3
        _ShadowStrength("Shadow Strength", Range(0, 1)) = 0.35

        _CrestColor("[FOLDOUT(Crest){3}]Color{Crest}", Color) = (1.0, 1.0, 1.0, 0.9)
        _CrestSize("Size{Crest}", Range(0, 1)) = 0.1
        _CrestSharpness("Sharp transition{Crest}", Range(0, 1)) = 0.1

        [KeywordEnum(None, Round, Grid, Pointy)] _WaveMode ("[FOLDOUT(Wave Geometry){7}]Shape{Wave}", Float) = 1.0
        _WaveSpeed("[!_WAVEMODE_NONE]Speed{Wave}", Float) = 0.5
        _WaveAmplitude("[!_WAVEMODE_NONE]Amplitude{Wave}", Float) = 0.25
        _WaveFrequency("[!_WAVEMODE_NONE]Frequency{Wave}", Float) = 1.0
        _WaveDirection("[!_WAVEMODE_NONE]Direction{Wave}", Range(-1.0, 1.0)) = 0
        [KeywordEnum(UV, World Space)] _NoiseSource ("Tiling Source{Wave}", Float) = 1.0
        _WaveNoise("[!_WAVEMODE_NONE]Noise{Wave}", Range(0, 2)) = 0.25

        [KeywordEnum(None, Gradient Noise, Texture)] _FoamMode ("[FOLDOUT(Foam){19}]Source{Foam}", Float) = 1.0
        [NoScaleOffset] _NoiseMap("[_FOAMMODE_TEXTURE]Texture{Foam}", 2D) = "white" {}
        _FoamColor("[!_FOAMMODE_NONE]Color{Foam}", Color) = (0.960784, 0.960784, 0.921569, 0.862745)
        [Space]
        _FoamDepth("[!_FOAMMODE_NONE]Shore Depth{Foam}", Float) = 0.65
        _FoamNoiseAmount("[!_FOAMMODE_NONE]Shore Blending{Foam}", Range(0.0, 1.0)) = 0.55
        [Space]
        _FoamAmount("[!_FOAMMODE_NONE]Amount{Foam}", Range(0, 3)) = 0.18
        [Space]
        _FoamScale("[!_FOAMMODE_NONE]Scale{Foam}", Range(0, 3)) = 0.8
        _FoamStretchX("[!_FOAMMODE_NONE]Stretch X{Foam}", Range(0, 10)) = 1
        _FoamStretchY("[!_FOAMMODE_NONE]Stretch Y{Foam}", Range(0, 10)) = 1
        [Space]
        _FoamSharpness("[!_FOAMMODE_NONE]Sharpness{Foam}", Range(0, 1)) = 0.32
        [Space]
        _FoamSpeed("[!_FOAMMODE_NONE]Speed{Foam}", Float) = 0.055
        _FoamDirection("[!_FOAMMODE_NONE]Direction{Foam}", Range(-1.0, 1.0)) = 0.1
        _FoamEdgeWobble("[!_FOAMMODE_NONE]Edge Wobble{Foam}", Range(0, 1)) = 0.32
        _FoamEdgeWobbleScale("[!_FOAMMODE_NONE]Edge Wobble Scale{Foam}", Float) = 6
        _FoamEdgeWobbleDistance("[!_FOAMMODE_NONE]Edge Wobble Distance{Foam}", Float) = 1.0
        _FoamBrokenness("[!_FOAMMODE_NONE]Brokenness{Foam}", Range(0, 1)) = 0.38
        _FoamBlobAmount("[!_FOAMMODE_NONE]Blob Amount{Foam}", Range(0, 1)) = 0.32
        _FoamBlobScale("[!_FOAMMODE_NONE]Blob Scale{Foam}", Float) = 3.5
        _FoamBlobDistance("[!_FOAMMODE_NONE]Blob Distance{Foam}", Float) = 1.25

        _RefractionFrequency("[FOLDOUT(Refraction){4}]Frequency", Float) = 35
        _RefractionAmplitude("Amplitude", Range(0, 0.1)) = 0.01
        _RefractionSpeed("Speed", Float) = 0.1
        _RefractionScale("Scale", Float) = 1

        [NoScaleOffset] _WaterReflectionTexture("[FOLDOUT(Reflection){9}]Texture{Reflection}", 2D) = "black" {}
        _ReflectionStrength("Strength{Reflection}", Range(0, 1)) = 0
        _ReflectionDistortion("Distortion{Reflection}", Range(0, 0.1)) = 0.01
        _ReflectionFresnelPower("Fresnel Power{Reflection}", Range(0.1, 8)) = 2
        _ReflectionDepthFade("Depth Fade{Reflection}", Range(0, 1)) = 1
        _ReflectionVerticalFlip("Vertical Flip{Reflection}", Range(0, 1)) = 0
        _SkyboxReflectionStrength("Skybox Strength{Reflection}", Range(0, 1)) = 0.05
        [HDR] _SkyboxReflectionTint("Skybox Tint{Reflection}", Color) = (1, 1, 1, 1)
        _SkyboxReflectionRoughness("Skybox Roughness{Reflection}", Range(0, 1)) = 0.65

        /*
        _SpecularAmount("[FOLDOUT(Specular){2}]Amount{Specular}", Range(0, 1)) = 0.5
        [HDR] _SpecularColor("Color{Specular}", Color) = (1, 1, 1, 1)
        */

        [HideInInspector] [ToggleOff] _Opaque("Opaque", Float) = 0.0
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"
        }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        Lighting Off
        ZWrite[_ZWrite]

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Version.hlsl"
        ENDHLSL

        Pass
        {
            HLSLPROGRAM
    	    // #define FLAT_KIT_DOTS_INSTANCING_ON // Uncomment to enable DOTS instancing
            #pragma prefer_hlslcc gles
    	    
            #if defined(FLAT_KIT_DOTS_INSTANCING_ON)
            #pragma target 4.5
            #pragma multi_compile _ DOTS_INSTANCING_ON
            #else
            #pragma target 2.0
    	    #endif

            #pragma shader_feature_local _COLORMODE_LINEAR _COLORMODE_GRADIENT_TEXTURE
            #pragma shader_feature_local _FOAMMODE_NONE _FOAMMODE_GRADIENT_NOISE _FOAMMODE_TEXTURE
            #pragma shader_feature_local _WAVEMODE_NONE _WAVEMODE_ROUND _WAVEMODE_GRID _WAVEMODE_POINTY
            #pragma shader_feature_local __ _NOISESOURCE_WORLD_SPACE

            // -------------------------------------
            // Universal Pipeline keywords
            #if VERSION_GREATER_EQUAL(11, 0)
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #else
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #endif
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #if VERSION_GREATER_EQUAL(12, 0)
            #pragma multi_compile_fragment _ _LIGHT_LAYERS
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #endif
            #if UNITY_VERSION >= 202220 && UNITY_VERSION < 600000
            #pragma multi_compile _ _FORWARD_PLUS
            #endif
            #if UNITY_VERSION >= 600000
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
            #endif

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fog

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            #if defined(_COLORMODE_GRADIENT_TEXTURE)
            TEXTURE2D(_ColorGradient);
            SAMPLER(sampler_ColorGradient);
            #endif

            TEXTURE2D(_NoiseMap);
            SAMPLER(sampler_NoiseMap);
            TEXTURE2D(_WaterReflectionTexture);
            SAMPLER(sampler_WaterReflectionTexture);

            CBUFFER_START(UnityPerMaterial)
            float4x4 _ReflectionVP;
            float _FadeDistance, _WaterDepth, _OpenWaterDepth, _MaxWaterDepth;

            half _LightContribution;

            half _WaveFrequency, _WaveAmplitude, _WaveSpeed, _WaveDirection, _WaveNoise;
            half _WaterClearness, _CrestSize, _CrestSharpness, _ShadowStrength;

            half4 _CrestColor;
            half4 _FoamColor;
            half _FoamDepth, _FoamAmount, _FoamScale, _FoamSharpness, _FoamStretchX, _FoamStretchY, _FoamSpeed,
                 _FoamDirection, _FoamNoiseAmount, _FoamEdgeWobble, _FoamEdgeWobbleScale, _FoamEdgeWobbleDistance, _FoamBrokenness,
                 _FoamBlobAmount, _FoamBlobScale, _FoamBlobDistance, _RefractionFrequency, _RefractionAmplitude, _RefractionSpeed,
                 _RefractionScale, _ReflectionStrength, _ReflectionDistortion, _ReflectionFresnelPower,
                 _ReflectionDepthFade, _ReflectionVerticalFlip, _SkyboxReflectionStrength, _SkyboxReflectionRoughness,
                 _FresnelAmount, _FresnelSharpness, _SunReflection;

            half4 _SkyboxReflectionTint;

            /*
            half _SpecularAmount;
            half4 _SpecularColor;
            */

            float4 _NoiseMap_ST;

            // _COLORMODE_LINEAR:
            half4 _ColorShallow, _ColorDeep;
            // _COLORMODE_GRADIENT_TEXTURE:
            float4 _ColorGradient_ST;
            CBUFFER_END

            struct VertexInput
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD6;
                float2 uv : TEXCOORD0;
                float4 screenPosition : TEXCOORD1;
                float4 reflectionPositionCS : TEXCOORD7;
                float waveHeight : TEXCOORD2;

                float3 normal : TEXCOORD3; // World space.
                float3 viewDir : TEXCOORD4; // World space.

                half fogFactor : TEXCOORD5;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float2 GradientNoise_Dir(float2 p)
            {
                // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
                // 3d0a9085-1fec-441a-bba6-f1121cdbe3ba
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }

            float GradientNoise(float2 UV, float Scale)
            {
                const float2 p = UV * Scale;
                const float2 ip = floor(p);
                float2 fp = frac(p);
                const float d00 = dot(GradientNoise_Dir(ip), fp);
                const float d01 = dot(GradientNoise_Dir(ip + float2(0, 1)), fp - float2(0, 1));
                const float d10 = dot(GradientNoise_Dir(ip + float2(1, 0)), fp - float2(1, 0));
                const float d11 = dot(GradientNoise_Dir(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
            }

            inline float2 Rotate2D(float2 uv, float angle)
            {
                const float s = sin(angle);
                const float c = cos(angle);
                return float2(uv.x * c - uv.y * s, uv.x * s + uv.y * c);
            }

            inline half IsScreenUVInside(float2 uv)
            {
                return step(0.0f, uv.x) * step(uv.x, 1.0f) * step(0.0f, uv.y) * step(uv.y, 1.0f);
            }

            inline half HasValidSceneDepth(float rawDepth)
            {
                // In reversed-Z, far/background depth is close to 0.
                // In regular-Z, far/background depth is close to 1.
                #if defined(UNITY_REVERSED_Z)
                    return step(0.0001f, rawDepth);
                #else
                    return step(rawDepth, 0.9999f);
                #endif
            }

            inline half SampleHasVisibleColor(half3 color)
            {
                // Reject the default black texture and transparent/empty reflection-camera clears.
                // Real sky/object reflections have non-zero radiance; near-black samples are treated as
                // "no planar reflection here" and fall back to water color/environment reflection.
                const half luma = dot(color, half3(0.2126h, 0.7152h, 0.0722h));
                const half max_channel = max(color.r, max(color.g, color.b));
                return smoothstep(0.012h, 0.06h, max(luma, max_channel));
            }

            inline float ResolveOpenWaterDepthFade()
            {
                const float water_depth = max(_FadeDistance, _OpenWaterDepth);
                return saturate((water_depth - _FadeDistance) / max(_WaterDepth, 1e-4f));
            }

            inline float WaterDepthFromRaw(float rawDepth, VertexOutput i)
            {
                const float is_ortho = unity_OrthoParams.w;
                const float is_persp = 1.0 - unity_OrthoParams.w;

                const float scene_depth = lerp(_ProjectionParams.z, _ProjectionParams.y, rawDepth) * is_ortho +
                    LinearEyeDepth(rawDepth, _ZBufferParams) * is_persp;
                const float surface_depth = lerp(_ProjectionParams.z, _ProjectionParams.y, i.screenPosition.z) *
                    is_ortho + i.screenPosition.w * is_persp;

                return min(max(0.0f, scene_depth - surface_depth), max(_MaxWaterDepth, 0.0f));
            }

            inline float DepthFadeFromRaw(float rawDepth, VertexOutput i)
            {
                const float water_depth = WaterDepthFromRaw(rawDepth, i);
                return saturate((water_depth - _FadeDistance) / max(_WaterDepth, 1e-4f));
            }

            inline float DepthFade(float2 uv, VertexOutput i)
            {
                const half inside = IsScreenUVInside(uv);
                const float2 safe_uv = lerp(float2(0.5f, 0.5f), uv, inside);
                const float raw_depth = SampleSceneDepth(safe_uv);
                return lerp(ResolveOpenWaterDepthFade(), DepthFadeFromRaw(raw_depth, i), inside * HasValidSceneDepth(raw_depth));
            }

            inline float SineWave(float3 pos, float offset)
            {
                return sin(
                    offset + _Time.z * _WaveSpeed + (pos.x * sin(offset + _WaveDirection * PI) + pos.z *
                        cos(offset + _WaveDirection * PI)) * _WaveFrequency);
            }

            inline float WaveHeight(float2 texcoord, float3 position)
            {
                float s = 0;

                #if defined(_WAVEMODE_GRID)
                #if defined(_NOISESOURCE_WORLD_SPACE)
                    float2 noise_uv = position.xz * _WaveFrequency;
                #else // _NOISESOURCE_WORLD_SPACE
                    float2 noise_uv = texcoord * _WaveFrequency;
                #endif // _NOISESOURCE_WORLD_SPACE
                    float noise01 = GradientNoise(noise_uv, 1.0);
                    float noise = (noise01 * 2.0 - 1.0) * _WaveNoise;

                    s = SineWave(position, noise);

                #if defined(_WAVEMODE_GRID)
                        s *= SineWave(position, HALF_PI + noise);
                #endif

                #if defined(_WAVEMODE_POINTY)
                        s = 1.0 - abs(s);
                #endif
                #endif

                return s;
            }

            inline void AdditionalLights(float3 WorldPosition, out half3 Color, out half Attenuation) {
                Color = 0;
                Attenuation = 0;

                #ifdef _ADDITIONAL_LIGHTS
                const half4 shadowMask = half4(1, 1, 1, 1);
                const uint numAdditionalLights = GetAdditionalLightsCount();
                for (uint lightI = 0; lightI < numAdditionalLights; lightI++) {
                    Light light = GetAdditionalLight(lightI, WorldPosition, shadowMask);
                    Color += light.color;
                    Attenuation += light.distanceAttenuation * light.shadowAttenuation;
                }

                Attenuation = saturate(Attenuation);
                #endif
            }

            VertexOutput vert(VertexInput i)
            {
                #if defined(CURVEDWORLD_IS_INSTALLED) && !defined(CURVEDWORLD_DISABLED_ON)
                #ifdef CURVEDWORLD_NORMAL_TRANSFORMATION_ON
                    CURVEDWORLD_TRANSFORM_VERTEX_AND_NORMAL(i.positionOS, i.normalOS, i.tangentOS)
                #else
                    CURVEDWORLD_TRANSFORM_VERTEX(i.positionOS)
                #endif
                #endif

                VertexOutput o = (VertexOutput)0;

                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_TRANSFER_INSTANCE_ID(i, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // Vertex animation.
                const float3 originalPositionWS = TransformObjectToWorld(i.positionOS.xyz);
                const float s = WaveHeight(i.texcoord, originalPositionWS);
                o.waveHeight = s;
                o.positionWS = originalPositionWS;
                o.positionWS.y += s * _WaveAmplitude;

                o.positionHCS = TransformWorldToHClip(o.positionWS);
                o.screenPosition = ComputeScreenPos(o.positionHCS);
                o.reflectionPositionCS = mul(_ReflectionVP, float4(o.positionWS, 1.0));
                o.uv = i.texcoord;

                {
                    // Normals.
                    const float3 viewDirWS = GetCameraPositionWS() - o.positionWS;
                    o.viewDir = viewDirWS;

                    const VertexNormalInputs normalInput = GetVertexNormalInputs(i.normalOS, i.tangentOS);

                    const float sample_distance = 0.01;

                    float3 pos_tangent = originalPositionWS + normalInput.tangentWS * sample_distance;
                    pos_tangent.y += WaveHeight(i.texcoord, pos_tangent) * _WaveAmplitude;

                    float3 pos_bitangent = originalPositionWS + normalInput.bitangentWS * sample_distance;
                    pos_bitangent.y += WaveHeight(i.texcoord, pos_bitangent) * _WaveAmplitude;

                    const float3 modified_tangent = pos_tangent - o.positionWS;
                    const float3 modified_bitangent = pos_bitangent - o.positionWS;
                    const float3 modified_normal = cross(modified_tangent, modified_bitangent);

                    o.normal = normalize(modified_normal);
                }

                const half fogFactor = ComputeFogFactor(o.positionHCS.z);
                o.fogFactor = fogFactor;

                return o;
            }

            half4 frag(VertexOutput i) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // Refraction.
                const float2 noise_uv_refraction = i.uv * _RefractionFrequency + _Time.zz * _RefractionSpeed;
                const float noise01_refraction = GradientNoise(noise_uv_refraction, _RefractionScale);
                const float noise11_refraction = noise01_refraction * 2.0f - 1.0f;
                const float2 screen_uv = i.screenPosition.xy / i.screenPosition.w;

                const half screen_inside = IsScreenUVInside(screen_uv);
                const float2 safe_screen_uv = lerp(float2(0.5f, 0.5f), screen_uv, screen_inside);
                const float raw_depth_original = SampleSceneDepth(safe_screen_uv);
                const half original_depth_valid = screen_inside * HasValidSceneDepth(raw_depth_original);
                const float depth_fade_original = lerp(ResolveOpenWaterDepthFade(), DepthFadeFromRaw(raw_depth_original, i), original_depth_valid);

                float2 displaced_candidate_uv = screen_uv + noise11_refraction * _RefractionAmplitude * depth_fade_original;
                const half displaced_inside = screen_inside * IsScreenUVInside(displaced_candidate_uv);
                float2 displaced_uv = lerp(screen_uv, displaced_candidate_uv, displaced_inside);

                float2 safe_displaced_uv = lerp(float2(0.5f, 0.5f), displaced_uv, displaced_inside);
                float raw_depth_displaced = SampleSceneDepth(safe_displaced_uv);
                half displaced_depth_valid = displaced_inside * HasValidSceneDepth(raw_depth_displaced);

                // If refraction would sample the outside/background depth, fall back to the original screen UV.
                if (displaced_depth_valid <= 0.0h)
                {
                    displaced_uv = screen_uv;
                    raw_depth_displaced = raw_depth_original;
                    displaced_depth_valid = original_depth_valid;
                    safe_displaced_uv = safe_screen_uv;
                }

                float depth_fade = lerp(ResolveOpenWaterDepthFade(), DepthFadeFromRaw(raw_depth_displaced, i), displaced_depth_valid);

                if (displaced_depth_valid > 0.0h && depth_fade <= 0.0f) // If above water surface.
                {
                    displaced_uv = screen_uv;
                    raw_depth_displaced = raw_depth_original;
                    displaced_depth_valid = original_depth_valid;
                    safe_displaced_uv = safe_screen_uv;
                    depth_fade = lerp(ResolveOpenWaterDepthFade(), DepthFadeFromRaw(raw_depth_displaced, i), displaced_depth_valid);
                }

                const half scene_color_weight = displaced_depth_valid;
                const half3 scene_color = SampleSceneColor(safe_displaced_uv);
                half3 c = scene_color;

                // Water depth.
                half4 depth_color;
                half4 color_shallow;
                #if defined(_COLORMODE_LINEAR)
                depth_color = lerp(_ColorShallow, _ColorDeep, depth_fade);
                color_shallow = _ColorShallow;
                #endif

                #if defined(_COLORMODE_GRADIENT_TEXTURE)
                float2 gradient_uv = float2(depth_fade, 0.5f);
                depth_color = SAMPLE_TEXTURE2D(_ColorGradient, sampler_ColorGradient, gradient_uv);
                color_shallow = SAMPLE_TEXTURE2D(_ColorGradient, sampler_ColorGradient, float2(0.0f, 0.5f));
                #endif

                // Only blend opaque scene color where the depth texture actually contains geometry.
                // This prevents the water from pulling the camera/background row at screen edges,
                // which was the source of the bright bottom strip.
                c = lerp(depth_color.rgb, scene_color, _WaterClearness * depth_color.a * scene_color_weight);

                // Planar Reflection.
                // _WaterReflectionTexture must be rendered by a mirrored reflection camera.
                // _ReflectionVP must be set from C#:
                // GL.GetGPUProjectionMatrix(reflectionCamera.projectionMatrix, true) * reflectionCamera.worldToCameraMatrix.
                {
                    const float4 reflection_position_cs = i.reflectionPositionCS;
                    const float inv_w = rcp(max(abs(reflection_position_cs.w), 1e-5));
                    float2 reflection_uv = reflection_position_cs.xy * inv_w;
                    reflection_uv = reflection_uv * 0.5f + 0.5f;

                    // Different graphics APIs / RenderTexture setups may need a vertical flip.
                    reflection_uv.y = lerp(reflection_uv.y, 1.0f - reflection_uv.y, _ReflectionVerticalFlip);

                    // Apply a subtle water distortion in projected reflection space.
                    reflection_uv += float2(noise11_refraction, noise11_refraction) * _ReflectionDistortion;

                    // Avoid smearing the texture border when projected UVs go outside the RenderTexture.
                    const half inside_reflection =
                        step(0.0f, reflection_uv.x) *
                        step(reflection_uv.x, 1.0f) *
                        step(0.0f, reflection_uv.y) *
                        step(reflection_uv.y, 1.0f) *
                        step(1e-5f, reflection_position_cs.w);

                    const float2 safe_reflection_uv = lerp(float2(0.5f, 0.5f), reflection_uv, inside_reflection);

                    const half4 reflection_sample =
                        SAMPLE_TEXTURE2D(_WaterReflectionTexture, sampler_WaterReflectionTexture, safe_reflection_uv);
                    const half3 reflection_color = reflection_sample.rgb;
                    const half reflection_color_content = SampleHasVisibleColor(reflection_color);
                    const half reflection_alpha_content = smoothstep(0.02h, 0.2h, reflection_sample.a);
                    const half reflection_content =
                        reflection_color_content * max(reflection_alpha_content, step(0.04h, reflection_color_content));

                    const float3 viewDirWS = normalize(i.viewDir);
                    const float3 normalWS = normalize(i.normal);
                    const half fresnel = pow(1.0h - saturate(dot(normalWS, viewDirWS)), _ReflectionFresnelPower);
                    const half depth_mask = lerp(1.0h, depth_fade, _ReflectionDepthFade);
                    const half reflection = saturate(_ReflectionStrength * fresnel * depth_mask * inside_reflection * reflection_content);

                    c = lerp(c, reflection_color, reflection);
                }

                // Skybox / environment reflection.
                // This samples Unity's current environment reflection cubemap, which is generated from
                // the active Skybox / Reflection Probe setup. It is not a flat color tint.
                // Project Settings > Lighting: Environment Reflections should use Source = Skybox, then
                // Generate Lighting or provide/update a Reflection Probe for runtime skybox changes.
                {
                    const float3 viewDirWS = normalize(i.viewDir);
                    const float3 normalWS = normalize(i.normal);
                    const float3 reflectDirWS = reflect(-viewDirWS, normalWS);
                    const half fresnel = pow(1.0h - saturate(dot(normalWS, viewDirWS)), _ReflectionFresnelPower);
                    const half depth_mask = lerp(1.0h, depth_fade, _ReflectionDepthFade);
                    const half3 skybox_reflection = GlossyEnvironmentReflection(reflectDirWS, _SkyboxReflectionRoughness, 1.0h) * _SkyboxReflectionTint.rgb;
                    const half skybox_amount = saturate(_SkyboxReflectionStrength * fresnel * depth_mask);
                    c = lerp(c, skybox_reflection, skybox_amount);
                }

                // Crest.
                {
                    const half c_inv = 1.0f - _CrestSize;
                    c = lerp(c, _CrestColor.rgb,
                             smoothstep(c_inv, saturate(c_inv + (1.0f - _CrestSharpness)),
                                        i.waveHeight) * _CrestColor.a);
                }

                // Foam.
                #if !defined(_FOAMMODE_NONE)
                    const float foam_water_depth = lerp(_OpenWaterDepth, WaterDepthFromRaw(raw_depth_displaced, i), displaced_depth_valid);
                    const float foam_depth = max(_FoamDepth, 1e-4f);
                    const float foam_sharpness = saturate(_FoamSharpness);
                    const float foam_softness = lerp(0.65f, 0.12f, foam_sharpness);
                    const float foam_amount = saturate(_FoamAmount);
                    const float2 foam_direction = float2(sin(_FoamDirection * PI), cos(_FoamDirection * PI));
                    const float2 foam_motion = foam_direction * (_Time.y * _FoamSpeed);
                    const float2 stretch_factor = float2(max(_FoamStretchX, 1e-3f), max(_FoamStretchY, 1e-3f));
                    const float2 foam_world_uv = Rotate2D(i.positionWS.xz, _FoamDirection * PI) * stretch_factor;

                    float noise_foam_base = 1.0f;

                #if defined(_FOAMMODE_TEXTURE)
                    const float2 noise_uv_foam = foam_world_uv * max(_FoamScale, 1e-3f) + foam_motion * 0.25f;
                    noise_foam_base = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap,
                        noise_uv_foam).r;
                #endif

                #if defined(_FOAMMODE_GRADIENT_NOISE)
                    const float foam_noise_scale = max(_FoamScale, 1e-3f);
                    const float slow_noise = GradientNoise(foam_world_uv + foam_motion * 0.35f, foam_noise_scale * 0.65f);
                    const float detail_noise = GradientNoise(foam_world_uv + foam_motion * 0.18f + float2(17.31f, 23.97f), foam_noise_scale * 1.35f);
                    noise_foam_base = saturate(slow_noise * 0.7f + detail_noise * 0.3f);
                #endif

                    const float edge_wobble_scale = max(_FoamEdgeWobbleScale * 0.2f, 1e-3f);
                    const float edge_wobble_noise = GradientNoise(foam_world_uv + foam_motion * 0.12f + float2(41.7f, 9.43f), edge_wobble_scale) * 2.0f - 1.0f;
                    const float base_shore_distance = foam_water_depth / foam_depth;
                    const float base_shore_band = 1.0f - smoothstep(0.0f, 1.0f + foam_softness, base_shore_distance);
                    const float edge_width = foam_depth * max(0.25f, 1.0f + edge_wobble_noise * 0.85f);
                    const float wobbled_shore_distance = foam_water_depth / max(edge_width, 1e-4f);
                    const float wobbled_shore_band = 1.0f - smoothstep(0.0f, 1.0f + foam_softness, wobbled_shore_distance);
                    const float wobble_distance = max(_FoamEdgeWobbleDistance, 1e-4f);
                    const float wobble_distance_fade = 1.0f - smoothstep(0.0f, wobble_distance, foam_water_depth);
                    const float wobble_mix = saturate(_FoamEdgeWobble) * wobble_distance_fade;
                    const float shore_band = lerp(base_shore_band, wobbled_shore_band, wobble_mix);

                    const float break_threshold = lerp(-0.2f, 0.78f, saturate(_FoamBrokenness));
                    const float break_noise = saturate(noise_foam_base * 0.85f +
                        GradientNoise(foam_world_uv - foam_motion * 0.2f + float2(83.19f, 61.27f), max(_FoamScale * 1.75f, 1e-3f)) * 0.15f);
                    const float broken_mask = smoothstep(break_threshold, break_threshold + 0.28f + foam_softness * 0.35f, break_noise);
                    const float brokenness = saturate(_FoamBrokenness);
                    const float broken_floor = lerp(1.0f, 0.35f, brokenness);
                    const float shore_noise_mask = lerp(1.0f, max(broken_floor, broken_mask), brokenness);
                    const float shore_blend = lerp(0.55f, 1.25f, saturate(_FoamNoiseAmount));
                    const float foam_shore = shore_band * shore_noise_mask * saturate(foam_amount * 4.0f) * shore_blend;

                    const float blob_distance = max(_FoamBlobDistance, foam_depth + 1e-3f);
                    const float blob_inner = smoothstep(foam_depth * 0.2f, foam_depth * 1.15f, foam_water_depth);
                    const float blob_outer = 1.0f - smoothstep(blob_distance, blob_distance + foam_depth * 0.75f, foam_water_depth);
                    const float blob_near_shore = blob_inner * blob_outer;
                    const float blob_scale = max(_FoamBlobScale * 0.22f, 1e-3f);
                    const float blob_noise_a = GradientNoise(foam_world_uv + foam_motion * 0.08f + float2(131.11f, 47.53f), blob_scale);
                    const float blob_noise_b = GradientNoise(foam_world_uv * 1.9f - foam_motion * 0.05f + float2(19.73f, 97.41f), blob_scale * 1.7f);
                    const float blob_noise = saturate(blob_noise_a * 0.72f + blob_noise_b * 0.35f);
                    const float blob_softness = lerp(0.18f, 0.06f, foam_sharpness);
                    const float foam_blobs = smoothstep(0.62f, 0.62f + blob_softness, blob_noise) *
                        blob_near_shore * saturate(_FoamBlobAmount);

                    float foam = saturate(foam_shore + foam_blobs);
                    c = lerp(c, _FoamColor.rgb, foam * _FoamColor.a);
                #endif

                // Shadows.
                #ifndef _MAIN_LIGHT_SHADOWS
                #define _MAIN_LIGHT_SHADOWS  // Since URP 13 or 14 this is not defined by default.
                #endif
                #if defined(_MAIN_LIGHT_SHADOWS)
                    VertexPositionInputs vertexInput = (VertexPositionInputs)0;
                    vertexInput.positionWS = i.positionWS.xyz;
                    float4 shadowCoord = GetShadowCoord(vertexInput);
                    half shadowAttenutation = MainLightRealtimeShadow(shadowCoord);
                    c = lerp(c, c * color_shallow.rgb, _ShadowStrength * (1.0h - shadowAttenutation));
                #endif

                /*
                // Specular.
                {
                    const float3 viewDirWS = normalize(i.viewDir);
                    const float3 normalWS = normalize(i.normal);
                    const float3 lightDirWS = -GetMainLight().direction;
                    const float3 halfDirWS = normalize(viewDirWS + lightDirWS);
                    const float specular = pow(saturate(dot(normalWS, halfDirWS)), 1.0f);
                    c = lerp(c, c * _SpecularColor.rgb, specular * _SpecularAmount);
                }
                */

                c *= lerp(half3(1, 1, 1), _MainLightColor.rgb, _LightContribution);

                #if defined(_ADDITIONAL_LIGHTS)
                half3 lightColor;
                half lightAttenuation;
                AdditionalLights(i.positionWS, lightColor, lightAttenuation);
                lightColor = lerp(half3(1, 1, 1), lightColor, _LightContribution);
                c += lightColor * lightAttenuation;
                #if !defined(_FOAMMODE_NONE)
                    c = lerp(c, _FoamColor.rgb * lightColor, foam * _FoamColor.a * lightAttenuation);
                #endif
                #endif

                c = MixFog(c, i.fogFactor);

                return half4(c, 1);
            }
            ENDHLSL
        }
    }

    CustomEditor "FlatKitWaterEditor"
}
