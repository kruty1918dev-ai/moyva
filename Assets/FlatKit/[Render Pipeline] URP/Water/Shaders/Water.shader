Shader "FlatKit/Water"
{
    Properties
    {
        [KeywordEnum(Linear, Gradient Texture)] _ColorMode("Source", Float) = 0
        _ColorShallow("Shallow", Color) = (0.56, 0.71, 0.72, 0.82)
        _ColorDeep("Deep", Color) = (0.31, 0.47, 0.52, 1)
        [NoScaleOffset] _ColorGradient("Gradient", 2D) = "white" {}
        _ColorBlend("Color Blend", Range(0, 1)) = 0.58

        _UseShoreDistanceTexture("Use Planar Shore Distance", Range(0, 1)) = 1
        [HideInInspector] _UseShoreDistanceTex("Use Planar Shore Distance Legacy", Float) = 1
        [NoScaleOffset] _ShoreDistanceTex("Planar Shore Distance", 2D) = "white" {}
        _ShoreDistanceWorldScale("Shore Distance World Scale", Vector) = (1, 1, 0, 0)
        _ShoreDistanceWorldOffset("Shore Distance World Offset", Vector) = (0, 0, 0, 0)

        _ContactFoamColor("Shore Foam Color", Color) = (0.88, 0.91, 0.88, 0.42)
        _ContactFoamWidth("Shore Foam Width", Range(0.001, 1)) = 0.10
        _ContactFoamSmoothness("Shore Foam Smoothness", Range(0.001, 1)) = 0.18
        _ContactFoamDissolve("Shore Foam Breakup", Range(0, 1)) = 0.42
        _ContactFoamEdgeFade("Shore Foam Edge Fade", Range(0, 1)) = 0.35
        _ContactFoamNoiseScale("Shore Foam Noise Scale", Float) = 4.8
        _ContactFoamNoiseSpeed("Shore Foam Noise Speed", Float) = 0.035
        _ContactFoamDistortion("Shore Foam Distortion", Range(0, 1)) = 0.35
        _ContactFoamStrength("Shore Foam Strength", Range(0, 1)) = 0.8

        _ShoreLineColor("Shoreline Color", Color) = (0.78, 0.86, 0.86, 0.18)
        _ShoreLineDepth("Shoreline Range", Range(0.001, 1)) = 0.75
        _ShoreLineSpeed("Shoreline Speed", Float) = 0.055
        _ShoreLineAmount("Shoreline Amount", Float) = 7.0
        _ShoreLineThickness("Shoreline Thickness", Range(0.01, 0.5)) = 0.13
        _ShoreLineCenterMask("Shoreline Center Mask", Range(0, 1)) = 0.72
        _ShoreLineCenterFade("Shoreline Center Fade", Range(0.001, 1)) = 0.28
        _ShoreLineTrailFade("Shoreline Trail Fade", Range(0, 1)) = 0.45
        _ShoreLineDissolve("Shoreline Breakup", Range(0, 1)) = 0.54
        _ShoreLineNoiseScale("Shoreline Noise Scale", Float) = 3.2
        _ShoreLineNoiseSpeed("Shoreline Noise Speed", Float) = 0.025
        _ShoreLineStrength("Shoreline Strength", Range(0, 1)) = 1

        [NoScaleOffset] _WaterReflectionTexture("Planar Reflection Texture", 2D) = "black" {}
        _ReflectionStrength("Reflection Strength", Range(0, 1)) = 0.06
        _ReflectionDistortion("Reflection Distortion", Range(0, 0.1)) = 0.003
        _ReflectionFresnelPower("Reflection Fresnel Power", Range(0.1, 8)) = 4
        _ReflectionEdgeFade("Reflection Edge Fade", Range(0, 16)) = 3
        _ReflectionVerticalFlip("Reflection Vertical Flip", Range(0, 1)) = 0
        _SkyboxReflectionStrength("Skybox Strength", Range(0, 1)) = 0.05
        [HDR] _SkyboxReflectionTint("Skybox Tint", Color) = (1, 1, 1, 1)
        _SkyboxReflectionRoughness("Skybox Roughness", Range(0, 1)) = 0.75

        _RefractionAmplitude("Surface Ripple Amplitude", Range(0, 0.1)) = 0.0015

        [Enum(Final Color, 0, Shore Distance, 1, Shore Foam Mask, 2, Shoreline Mask, 3, Surface Ripple Mask, 4, Reflection Only, 5)] _DebugMode("Debug Mode", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

        LOD 150
        Blend SrcAlpha OneMinusSrcAlpha
        Lighting Off
        ZWrite Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Version.hlsl"
        ENDHLSL

        Pass
        {
            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma target 2.0

            #pragma shader_feature_local _COLORMODE_LINEAR _COLORMODE_GRADIENT_TEXTURE

            #if VERSION_GREATER_EQUAL(11, 0)
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #else
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #endif
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            #if defined(_COLORMODE_GRADIENT_TEXTURE)
            TEXTURE2D(_ColorGradient);
            SAMPLER(sampler_ColorGradient);
            #endif

            TEXTURE2D(_WaterReflectionTexture);
            SAMPLER(sampler_WaterReflectionTexture);
            TEXTURE2D(_ShoreDistanceTex);
            SAMPLER(sampler_ShoreDistanceTex);

            CBUFFER_START(UnityPerMaterial)
            float4x4 _ReflectionVP;
            float4 _ShoreDistanceWorldScale;
            float4 _ShoreDistanceWorldOffset;

            half4 _ColorShallow;
            half4 _ColorDeep;
            half4 _SkyboxReflectionTint;
            half4 _ContactFoamColor;
            half4 _ShoreLineColor;

            half _ColorBlend;
            half _UseShoreDistanceTexture;
            half _UseShoreDistanceTex;
            half _ContactFoamWidth;
            half _ContactFoamSmoothness;
            half _ContactFoamDissolve;
            half _ContactFoamEdgeFade;
            half _ContactFoamNoiseScale;
            half _ContactFoamNoiseSpeed;
            half _ContactFoamDistortion;
            half _ContactFoamStrength;
            half _ShoreLineDepth;
            half _ShoreLineSpeed;
            half _ShoreLineAmount;
            half _ShoreLineThickness;
            half _ShoreLineCenterMask;
            half _ShoreLineCenterFade;
            half _ShoreLineTrailFade;
            half _ShoreLineDissolve;
            half _ShoreLineNoiseScale;
            half _ShoreLineNoiseSpeed;
            half _ShoreLineStrength;
            half _ReflectionStrength;
            half _ReflectionDistortion;
            half _ReflectionFresnelPower;
            half _ReflectionEdgeFade;
            half _ReflectionVerticalFlip;
            half _SkyboxReflectionStrength;
            half _SkyboxReflectionRoughness;
            half _RefractionAmplitude;
            half _DebugMode;
            CBUFFER_END

            struct VertexInput
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float4 reflectionPositionCS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
                half fogFactor : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float2 GradientNoise_Dir(float2 p)
            {
                p = fmod(p, 289.0f);
                float x = fmod(fmod((34.0f * p.x + 1.0f) * p.x, 289.0f) + p.y, 289.0f);
                x = fmod((34.0f * x + 1.0f) * x, 289.0f);
                x = frac(x / 41.0f) * 2.0f - 1.0f;
                return normalize(float2(x - floor(x + 0.5f), abs(x) - 0.5f));
            }

            float GradientNoise(float2 uv, float scale)
            {
                const float2 p = uv * max(scale, 1e-4f);
                const float2 ip = floor(p);
                float2 fp = frac(p);
                const float d00 = dot(GradientNoise_Dir(ip), fp);
                const float d01 = dot(GradientNoise_Dir(ip + float2(0.0f, 1.0f)), fp - float2(0.0f, 1.0f));
                const float d10 = dot(GradientNoise_Dir(ip + float2(1.0f, 0.0f)), fp - float2(1.0f, 0.0f));
                const float d11 = dot(GradientNoise_Dir(ip + float2(1.0f, 1.0f)), fp - float2(1.0f, 1.0f));
                fp = fp * fp * fp * (fp * (fp * 6.0f - 15.0f) + 10.0f);
                return saturate(lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5f);
            }

            half ScreenUVSoftMask(float2 uv, float softnessPixels)
            {
                const float2 edgeDistance = min(uv, 1.0f - uv);
                const float2 fadeWidth = max(softnessPixels / _ScreenParams.xy, 1e-5f);
                const float2 mask = smoothstep(float2(0.0f, 0.0f), fadeWidth, edgeDistance);
                return saturate((half)(mask.x * mask.y));
            }

            float2 SafeScreenUV(float2 uv)
            {
                return clamp(uv, float2(0.001f, 0.001f), float2(0.999f, 0.999f));
            }

            half SampleHasVisibleColor(half3 color)
            {
                const half luma = dot(color, half3(0.2126h, 0.7152h, 0.0722h));
                const half maxChannel = max(color.r, max(color.g, color.b));
                return smoothstep(0.012h, 0.06h, max(luma, maxChannel));
            }

            float SamplePlanarShoreDistance(float3 positionWS)
            {
                const float2 shoreUV = positionWS.xz * _ShoreDistanceWorldScale.xy + _ShoreDistanceWorldOffset.xy;
                const float textureDistance = SAMPLE_TEXTURE2D(_ShoreDistanceTex, sampler_ShoreDistanceTex, shoreUV).r;
                const float enabled = saturate(max(_UseShoreDistanceTexture, _UseShoreDistanceTex));
                return lerp(1.0f, saturate(textureDistance), enabled);
            }

            float ShoreFoamMask(float shoreDistance, float2 worldUV, float time)
            {
                const float width = max((float)_ContactFoamWidth, 0.001f);
                const float softness = max((float)_ContactFoamSmoothness + width * (float)_ContactFoamEdgeFade, 0.001f);
                const float2 noiseUV = worldUV + float2(time * (float)_ContactFoamNoiseSpeed, -time * (float)_ContactFoamNoiseSpeed * 0.63f);
                const float noiseA = GradientNoise(noiseUV + float2(21.17f, 8.31f), _ContactFoamNoiseScale);
                const float noiseB = GradientNoise(noiseUV * 1.91f + float2(74.2f, 31.9f), _ContactFoamNoiseScale * 1.8f);
                const float noise = saturate(noiseA * 0.68f + noiseB * 0.32f);
                const float signedNoise = noise * 2.0f - 1.0f;

                const float distortedDistance = saturate(shoreDistance + signedNoise * (float)_ContactFoamDistortion * width);
                const float shoreBand = 1.0f - smoothstep(width, width + softness, distortedDistance);
                const float dissolveCut = lerp(-0.18f, 0.78f, saturate((float)_ContactFoamDissolve));
                const float dissolveMask = lerp(
                    1.0f,
                    smoothstep(dissolveCut, dissolveCut + 0.32f, noise),
                    saturate((float)_ContactFoamDissolve));

                return saturate(shoreBand * dissolveMask * _ContactFoamStrength);
            }

            float ShoreLineMask(float shoreDistance, float2 worldUV, float time)
            {
                const float range = max((float)_ShoreLineDepth, 0.001f);
                const float normalizedDistance = saturate(shoreDistance / range);
                const float2 noiseUV = worldUV + float2(time * (float)_ShoreLineNoiseSpeed * 0.7f, time * (float)_ShoreLineNoiseSpeed);
                const float noiseA = GradientNoise(noiseUV + float2(9.2f, 57.4f), _ShoreLineNoiseScale);
                const float noiseB = GradientNoise(noiseUV * 2.23f + float2(83.1f, 17.5f), _ShoreLineNoiseScale * 2.1f);
                const float noise = saturate(noiseA * 0.6f + noiseB * 0.4f);
                const float signedNoise = noise * 2.0f - 1.0f;

                const float distortedDistance = saturate(normalizedDistance + signedNoise * saturate((float)_ShoreLineDissolve) * 0.12f);
                const float center = saturate((float)_ShoreLineCenterMask);
                const float centerFade = max((float)_ShoreLineCenterFade, 0.001f);
                const float shoreWindow = 1.0f - smoothstep(center, center + centerFade, distortedDistance);

                const float movingDistance = distortedDistance * max((float)_ShoreLineAmount, 0.01f) - time * (float)_ShoreLineSpeed;
                const float phase = frac(movingDistance);
                const float centerDistance = abs(phase - 0.5f) * 2.0f;
                const float thickness = saturate((float)_ShoreLineThickness);
                float band = 1.0f - smoothstep(thickness, min(thickness + 0.18f, 1.0f), centerDistance);
                const float trail = lerp(1.0f, saturate(1.0f - phase), saturate((float)_ShoreLineTrailFade));
                band *= trail;

                const float dissolveCut = lerp(-0.1f, 0.82f, saturate((float)_ShoreLineDissolve));
                const float dissolveMask = lerp(
                    1.0f,
                    smoothstep(dissolveCut, dissolveCut + 0.35f, noise),
                    saturate((float)_ShoreLineDissolve));

                return saturate(band * shoreWindow * dissolveMask * _ShoreLineStrength);
            }

            VertexOutput vert(VertexInput input)
            {
                VertexOutput output = (VertexOutput)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionHCS = TransformWorldToHClip(output.positionWS);
                output.reflectionPositionCS = mul(_ReflectionVP, float4(output.positionWS, 1.0f));

                const VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = normalize(normalInput.normalWS);
                output.viewDirWS = GetCameraPositionWS() - output.positionWS;
                output.fogFactor = ComputeFogFactor(output.positionHCS.z);

                return output;
            }

            half4 frag(VertexOutput i) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                const half colorBlend = saturate(_ColorBlend);

                half4 waterColor;
                #if defined(_COLORMODE_GRADIENT_TEXTURE)
                    waterColor = SAMPLE_TEXTURE2D(_ColorGradient, sampler_ColorGradient, float2(colorBlend, 0.5f));
                #else
                    waterColor = lerp(_ColorShallow, _ColorDeep, colorBlend);
                #endif

                half3 color = waterColor.rgb;

                const float2 worldUV = i.positionWS.xz;
                const float time = _Time.y;
                const half surfaceNoise = (half)(GradientNoise(worldUV + float2(time * 0.035f, -time * 0.021f), 0.85f) * 2.0f - 1.0f);
                const half rippleStrength = (half)((_RefractionAmplitude * 34.0f) / (1.0f + _RefractionAmplitude * 34.0f));
                color *= 1.0h + surfaceNoise * rippleStrength * 0.025h;
                const half surfaceRippleMask = saturate(surfaceNoise * 0.5h + 0.5h);
                const float shoreDistance = SamplePlanarShoreDistance(i.positionWS);
                const half shoreFoamMask = (half)ShoreFoamMask(shoreDistance, worldUV, time);
                const half shoreLineMask = (half)ShoreLineMask(shoreDistance, worldUV, time);

                half3 reflectionOnly = half3(0.0h, 0.0h, 0.0h);

                {
                    const float4 reflectionPositionCS = i.reflectionPositionCS;
                    const float invW = rcp(max(abs(reflectionPositionCS.w), 1e-5f));
                    float2 reflectionUV = reflectionPositionCS.xy * invW * 0.5f + 0.5f;
                    reflectionUV.y = lerp(reflectionUV.y, 1.0f - reflectionUV.y, _ReflectionVerticalFlip);
                    reflectionUV += float2(surfaceNoise, -surfaceNoise) * (_ReflectionDistortion + _RefractionAmplitude * 0.55h);

                    const half insideReflection =
                        ScreenUVSoftMask(reflectionUV, max((float)_ReflectionEdgeFade, 0.0f)) *
                        smoothstep(1e-5f, 0.02f, reflectionPositionCS.w);
                    const half4 reflectionSample = SAMPLE_TEXTURE2D(_WaterReflectionTexture, sampler_WaterReflectionTexture, SafeScreenUV(reflectionUV));
                    const half visibleReflection = SampleHasVisibleColor(reflectionSample.rgb);
                    const half reflectionContent = visibleReflection * max(smoothstep(0.02h, 0.2h, reflectionSample.a), visibleReflection);

                    const float3 viewDirWS = normalize(i.viewDirWS);
                    const float3 normalWS = normalize(i.normalWS);
                    const half fresnel = pow(1.0h - saturate(dot(normalWS, viewDirWS)), max(_ReflectionFresnelPower, 0.1h));
                    const half reflectionAmount = saturate(_ReflectionStrength * fresnel * insideReflection * reflectionContent);
                    reflectionOnly += reflectionSample.rgb * reflectionAmount;
                    color = lerp(color, reflectionSample.rgb, reflectionAmount);
                }

                {
                    const float3 viewDirWS = normalize(i.viewDirWS);
                    const float3 normalWS = normalize(i.normalWS);
                    const float3 reflectDirWS = reflect(-viewDirWS, normalWS);
                    const half fresnel = pow(1.0h - saturate(dot(normalWS, viewDirWS)), 4.0h);
                    const half3 skyboxReflection =
                        GlossyEnvironmentReflection(reflectDirWS, _SkyboxReflectionRoughness, 1.0h) * _SkyboxReflectionTint.rgb;
                    const half skyboxAmount = saturate(_SkyboxReflectionStrength * fresnel);
                    reflectionOnly += skyboxReflection * skyboxAmount;
                    color = lerp(color, skyboxReflection, skyboxAmount);
                }

                color = lerp(color, _ShoreLineColor.rgb, shoreLineMask * saturate(_ShoreLineColor.a));
                color = lerp(color, _ContactFoamColor.rgb, shoreFoamMask * saturate(_ContactFoamColor.a));

                #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE) || defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                    VertexPositionInputs vertexInput = (VertexPositionInputs)0;
                    vertexInput.positionWS = i.positionWS;
                    vertexInput.positionCS = i.positionHCS;
                    const float4 shadowCoord = GetShadowCoord(vertexInput);
                    const half shadowAttenuation = MainLightRealtimeShadow(shadowCoord);
                    color = lerp(color, color * _ColorShallow.rgb, 0.22h * (1.0h - shadowAttenuation));
                #endif

                color = MixFog(color, i.fogFactor);

                if (_DebugMode > 0.5h && _DebugMode < 1.5h)
                {
                    return half4(((half)shoreDistance).xxx, 1.0h);
                }

                if (_DebugMode >= 1.5h && _DebugMode < 2.5h)
                {
                    return half4(shoreFoamMask.xxx, 1.0h);
                }

                if (_DebugMode >= 2.5h && _DebugMode < 3.5h)
                {
                    return half4(shoreLineMask.xxx, 1.0h);
                }

                if (_DebugMode >= 3.5h && _DebugMode < 4.5h)
                {
                    return half4(surfaceRippleMask.xxx, 1.0h);
                }

                if (_DebugMode >= 4.5h)
                {
                    return half4(saturate(reflectionOnly), 1.0h);
                }

                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }

    CustomEditor "BadNorthWaterEditor"
}
