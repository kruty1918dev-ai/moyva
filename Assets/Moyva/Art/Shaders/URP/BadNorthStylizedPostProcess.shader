Shader "Hidden/Moyva/BadNorth Stylized Post Process"
{
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
        }

        ZWrite Off
        ZTest Always
        Cull Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

        TEXTURE2D_X(_BlitTexture);
        SAMPLER(sampler_BlitTexture);
        TEXTURE2D_X(_BadNorthOcclusionTexture);
        SAMPLER(sampler_BadNorthOcclusionTexture);

        CBUFFER_START(UnityPerMaterial)
            float4 _OcclusionColor;
            float4 _ContactColor;
            float4 _CreaseColor;
            float4 _ShadowTint;
            float4 _HighlightTint;
            float4 _FogColor;
            float4 _VignetteColor;
            float4 _Lift;
            float4 _Gamma;
            float4 _Gain;

            float4 _BadNorth_TexelSize;
            float2 _BlurDirection;

            float _AOEnabled;
            float _AOIntensity;
            float _AORadius;
            float _AOSampleCount;
            float _AOPower;
            float _AOThickness;
            float _AODepthSensitivity;
            float _AONormalSensitivity;
            float _AODistanceFadeStart;
            float _AODistanceFadeEnd;
            float _AOAffectSky;
            float _AOAffectTransparent;

            float _ContactEnabled;
            float _ContactStrength;
            float _ContactRadius;
            float _ContactFalloff;
            float _SmallObjectBoost;
            float _GrassCardBoost;

            float _CreaseEnabled;
            float _CreaseStrength;
            float _CreaseRadius;
            float _CreaseThreshold;
            float _CreaseSoftness;

            float _PaletteEnabled;
            float _Saturation;
            float _Contrast;
            float _PaletteBlend;

            float _FogEnabled;
            float _FogStrength;
            float _FogStart;
            float _FogEnd;
            float _FogHeightInfluence;
            float _FogBlend;

            float _VignetteEnabled;
            float _VignetteStrength;
            float _VignetteRadius;
            float _VignetteSoftness;

            float _DebugView;
            float _BlurRadius;
            float _BlurDepthSensitivity;
            float _DepthAvailable;
            float _OcclusionMaskEnabled;
        CBUFFER_END

        struct Attributes
        {
            uint vertexID : SV_VertexID;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

        Varyings Vert(Attributes input)
        {
            Varyings output;
            output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
            output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
            return output;
        }

        bool IsBackgroundDepth(float rawDepth)
        {
        #if UNITY_REVERSED_Z
            return rawDepth <= 0.0001;
        #else
            return rawDepth >= 0.9999;
        #endif
        }

        float SampleRawDepth(float2 uv)
        {
            return SampleSceneDepth(UnityStereoTransformScreenSpaceTex(uv));
        }

        float SampleEyeDepth(float2 uv)
        {
            float rawDepth = SampleRawDepth(uv);
            if (IsBackgroundDepth(rawDepth))
                return _ProjectionParams.z;

            return LinearEyeDepth(rawDepth, _ZBufferParams);
        }

        float3 SampleSafeNormal(float2 uv)
        {
            float3 normalWS = SampleSceneNormals(UnityStereoTransformScreenSpaceTex(uv));
            float lenSq = dot(normalWS, normalWS);
            if (lenSq < 0.001)
                return float3(0.0, 1.0, 0.0);

            return normalize(normalWS);
        }

        float DistanceFade(float eyeDepth)
        {
            float start = min(_AODistanceFadeStart, _AODistanceFadeEnd);
            float end = max(_AODistanceFadeStart + 0.001, _AODistanceFadeEnd);
            return 1.0 - smoothstep(start, end, eyeDepth);
        }

        float SampleOccluder(float centerDepth, float sampleDepth, float thickness)
        {
            float closer = centerDepth - sampleDepth;
            float inRange = 1.0 - smoothstep(thickness, thickness * 3.0 + 0.001, abs(closer));
            return saturate(closer / max(0.001, thickness)) * inRange;
        }

        float4 FragOcclusion(Varyings input) : SV_Target
        {
            if (_OcclusionMaskEnabled < 0.5 || _DepthAvailable < 0.5)
                return float4(0.0, 0.0, 0.0, 1.0);

            float2 uv = input.uv;
            float rawDepth = SampleRawDepth(uv);
            bool background = IsBackgroundDepth(rawDepth);

            if (background && _AOAffectSky < 0.5)
                return float4(0.0, 0.0, 0.0, 1.0);

            float centerDepth = background ? _ProjectionParams.z : LinearEyeDepth(rawDepth, _ZBufferParams);
            float3 centerNormal = SampleSafeNormal(uv);
            float fade = DistanceFade(centerDepth);

            float2 dirs[12] =
            {
                float2( 1.000,  0.000),
                float2( 0.866,  0.500),
                float2( 0.500,  0.866),
                float2( 0.000,  1.000),
                float2(-0.500,  0.866),
                float2(-0.866,  0.500),
                float2(-1.000,  0.000),
                float2(-0.866, -0.500),
                float2(-0.500, -0.866),
                float2( 0.000, -1.000),
                float2( 0.500, -0.866),
                float2( 0.866, -0.500)
            };

            float sampleCount = clamp(_AOSampleCount, 4.0, 12.0);
            float depthScale = rcp(max(1.0, centerDepth));
            float2 baseRadius = _BadNorth_TexelSize.xy * (18.0 + _AORadius * 42.0) * lerp(1.0, depthScale * 10.0, 0.35);
            float2 contactRadius = _BadNorth_TexelSize.xy * (4.0 + _ContactRadius * 18.0) * lerp(1.0, depthScale * 10.0, 0.45);
            float2 creaseRadius = _BadNorth_TexelSize.xy * (2.0 + _CreaseRadius * 14.0);

            float ao = 0.0;
            float contact = 0.0;
            float crease = 0.0;
            float normalCrease = 0.0;
            float used = 0.0;

            UNITY_LOOP
            for (int i = 0; i < 12; i++)
            {
                if (i >= sampleCount)
                    break;

                float ring = 0.55 + 0.45 * ((i % 3) / 2.0);
                float2 dir = dirs[i];

                float2 aoUV = clamp(uv + dir * baseRadius * ring, _BadNorth_TexelSize.xy, 1.0 - _BadNorth_TexelSize.xy);
                float aoDepth = SampleEyeDepth(aoUV);
                float3 sampleNormal = SampleSafeNormal(aoUV);
                float normalWeight = pow(saturate(1.0 - dot(centerNormal, sampleNormal)), max(0.001, _AONormalSensitivity));
                float occluder = SampleOccluder(centerDepth, aoDepth, max(0.001, _AOThickness));
                ao += occluder * lerp(1.0, 1.0 - normalWeight, 0.45);

                float2 contactUV = clamp(uv + dir * contactRadius * ring, _BadNorth_TexelSize.xy, 1.0 - _BadNorth_TexelSize.xy);
                float contactDepth = SampleEyeDepth(contactUV);
                float contactDelta = saturate((centerDepth - contactDepth) / max(0.001, _ContactFalloff));
                float smallShape = saturate(abs(centerDepth - contactDepth) * _SmallObjectBoost);
                contact += saturate(contactDelta + smallShape * 0.15);

                float2 creaseUV = clamp(uv + dir * creaseRadius, _BadNorth_TexelSize.xy, 1.0 - _BadNorth_TexelSize.xy);
                float creaseDepth = SampleEyeDepth(creaseUV);
                float3 creaseNormal = SampleSafeNormal(creaseUV);
                float depthEdge = abs(creaseDepth - centerDepth);
                float creaseBand = smoothstep(_CreaseThreshold, _CreaseThreshold + max(0.001, _CreaseSoftness), depthEdge);
                float normalEdge = saturate(1.0 - dot(centerNormal, creaseNormal));
                crease += creaseBand;
                normalCrease += smoothstep(0.05, 0.55, normalEdge);

                used += 1.0;
            }

            used = max(1.0, used);
            ao = pow(saturate(ao / used), max(0.001, _AOPower)) * _AOIntensity * _AOEnabled;
            contact = pow(saturate(contact / used), 1.15) * _ContactStrength * _ContactEnabled;
            crease = saturate((crease + normalCrease * 0.65) / used) * _CreaseStrength * _CreaseEnabled;

            float distanceFade = background ? _AOAffectSky : fade;
            ao = saturate(ao * distanceFade);
            contact = saturate(contact * distanceFade * lerp(1.0, 1.25, _GrassCardBoost));
            crease = saturate(crease * distanceFade);

            return float4(ao, contact, crease, 1.0);
        }

        float4 FragBlur(Varyings input) : SV_Target
        {
            if (_OcclusionMaskEnabled < 0.5)
                return SAMPLE_TEXTURE2D_X(_BadNorthOcclusionTexture, sampler_BadNorthOcclusionTexture, UnityStereoTransformScreenSpaceTex(input.uv));

            float2 uv = input.uv;
            float centerDepth = _DepthAvailable > 0.5 ? SampleEyeDepth(uv) : _ProjectionParams.z;
            float radius = clamp(_BlurRadius, 0.0, 6.0);
            float4 sum = 0.0;
            float weightSum = 0.0;

            UNITY_UNROLL
            for (int i = -6; i <= 6; i++)
            {
                float a = abs((float)i);
                if (a > radius + 0.01)
                    continue;

                float2 sampleUV = clamp(uv + _BlurDirection * _BadNorth_TexelSize.xy * i, _BadNorth_TexelSize.xy, 1.0 - _BadNorth_TexelSize.xy);
                float sampleDepth = _DepthAvailable > 0.5 ? SampleEyeDepth(sampleUV) : centerDepth;
                float depthWeight = _DepthAvailable > 0.5 ? exp(-abs(sampleDepth - centerDepth) * max(0.01, _BlurDepthSensitivity)) : 1.0;
                float spatialWeight = exp(-(a * a) / max(0.001, radius * radius));
                float w = depthWeight * spatialWeight;
                sum += SAMPLE_TEXTURE2D_X(_BadNorthOcclusionTexture, sampler_BadNorthOcclusionTexture, UnityStereoTransformScreenSpaceTex(sampleUV)) * w;
                weightSum += w;
            }

            return sum / max(0.0001, weightSum);
        }

        float3 ApplySaturation(float3 color, float saturationPercent)
        {
            float saturation = 1.0 + saturationPercent * 0.01;
            float luma = dot(color, float3(0.2126, 0.7152, 0.0722));
            return lerp(luma.xxx, color, saturation);
        }

        float3 ApplyContrast(float3 color, float contrastPercent)
        {
            float contrast = 1.0 + contrastPercent * 0.01;
            return (color - 0.5) * contrast + 0.5;
        }

        float3 ApplyLiftGammaGain(float3 color)
        {
            color = max(0.0, color + _Lift.rgb);
            float3 gamma = max(float3(0.001, 0.001, 0.001), _Gamma.rgb);
            color = pow(saturate(color), rcp(gamma));
            color *= _Gain.rgb;
            return color;
        }

        float3 ApplyPalette(float3 color)
        {
            if (_PaletteEnabled < 0.5 || _PaletteBlend <= 0.001)
                return color;

            float3 graded = ApplySaturation(color, _Saturation);
            graded = ApplyContrast(graded, _Contrast);
            graded = ApplyLiftGammaGain(graded);

            float luma = dot(graded, float3(0.2126, 0.7152, 0.0722));
            float shadowMask = 1.0 - smoothstep(0.18, 0.68, luma);
            float highlightMask = smoothstep(0.42, 0.95, luma);
            graded = lerp(graded, graded * _ShadowTint.rgb, shadowMask * 0.42);
            graded = lerp(graded, graded * _HighlightTint.rgb, highlightMask * 0.22);

            return lerp(color, graded, saturate(_PaletteBlend));
        }

        float3 ApplyFog(float3 color, float2 uv, float rawDepth, bool background)
        {
            if (_FogEnabled < 0.5 || _FogStrength <= 0.001 || _DepthAvailable < 0.5 || (background && _AOAffectSky < 0.5))
                return color;

            float eyeDepth = background ? _ProjectionParams.z : LinearEyeDepth(rawDepth, _ZBufferParams);
            float fogRange = smoothstep(_FogStart, max(_FogStart + 0.001, _FogEnd), eyeDepth);
            float3 worldPos = ComputeWorldSpacePosition(uv, rawDepth, UNITY_MATRIX_I_VP);
            float heightFactor = lerp(1.0, saturate(1.0 - worldPos.y * 0.045), saturate(_FogHeightInfluence));
            float fog = saturate(fogRange * _FogStrength * _FogBlend * heightFactor);
            return lerp(color, _FogColor.rgb, fog);
        }

        float3 ApplyVignette(float3 color, float2 uv, bool background)
        {
            if (_VignetteEnabled < 0.5 || _VignetteStrength <= 0.001 || (background && _AOAffectSky < 0.5))
                return color;

            float2 centered = uv * 2.0 - 1.0;
            centered.x *= _ScreenParams.x / max(1.0, _ScreenParams.y);
            float dist = length(centered);
            float mask = smoothstep(_VignetteRadius, _VignetteRadius + max(0.001, _VignetteSoftness), dist);
            return lerp(color, color * _VignetteColor.rgb, mask * _VignetteStrength);
        }

        float4 FragComposite(Varyings input) : SV_Target
        {
            float2 uv = input.uv;
            float4 scene = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, UnityStereoTransformScreenSpaceTex(uv));
            float4 occ = SAMPLE_TEXTURE2D_X(_BadNorthOcclusionTexture, sampler_BadNorthOcclusionTexture, UnityStereoTransformScreenSpaceTex(uv));
            float rawDepth = 0.0;
            bool background = false;
            if (_DepthAvailable > 0.5)
            {
                rawDepth = SampleRawDepth(uv);
                background = IsBackgroundDepth(rawDepth);
            }

            float ao = saturate(occ.r);
            float contact = saturate(occ.g);
            float crease = saturate(occ.b);

            if (_DebugView > 0.5 && _DebugView < 1.5)
                return float4(ao.xxx, 1.0);
            if (_DebugView >= 1.5 && _DebugView < 2.5)
                return float4(contact.xxx, 1.0);
            if (_DebugView >= 2.5 && _DebugView < 3.5)
                return float4(crease.xxx, 1.0);
            if (_DebugView >= 3.5)
                return float4(saturate(float3(ao, contact, crease)), 1.0);

            float3 color = scene.rgb;
            if (!background || _AOAffectSky > 0.5)
            {
                color = lerp(color, color * _OcclusionColor.rgb, ao);
                color = lerp(color, color * _ContactColor.rgb, contact);
                color = lerp(color, color * _CreaseColor.rgb, crease);
            }

            if (!background || _AOAffectSky > 0.5)
                color = ApplyPalette(color);
            color = ApplyFog(color, uv, rawDepth, background);
            color = ApplyVignette(color, uv, background);

            return float4(saturate(color), scene.a);
        }

        float4 FragCopy(Varyings input) : SV_Target
        {
            return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, UnityStereoTransformScreenSpaceTex(input.uv));
        }
        ENDHLSL

        Pass
        {
            Name "OcclusionMask"
            Blend One Zero
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment FragOcclusion
            ENDHLSL
        }

        Pass
        {
            Name "EdgeAwareBlur"
            Blend One Zero
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment FragBlur
            ENDHLSL
        }

        Pass
        {
            Name "Composite"
            Blend One Zero
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment FragComposite
            ENDHLSL
        }

        Pass
        {
            Name "Copy"
            Blend One Zero
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment FragCopy
            ENDHLSL
        }
    }
    FallBack Off
}
