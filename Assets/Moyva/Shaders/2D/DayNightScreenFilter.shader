Shader "Moyva/2D/DayNight Screen Filter"
{
    Properties
    {
        _DayTint ("Day Tint", Color) = (1.00, 1.00, 1.00, 1)
        _NightTint ("Night Tint", Color) = (0.70, 0.78, 0.95, 1)
        _DawnTint ("Dawn Tint", Color) = (1.05, 0.92, 0.82, 1)
        _DuskTint ("Dusk Tint", Color) = (0.92, 0.82, 1.00, 1)

        _FilterStrength ("Filter Strength", Range(0,1)) = 0.5
        _DaySaturation ("Day Saturation", Range(0,2)) = 1.0
        _NightSaturation ("Night Saturation", Range(0,2)) = 0.82
        _DayContrast ("Day Contrast", Range(0.5,2)) = 1.0
        _NightContrast ("Night Contrast", Range(0.5,2)) = 1.06
        _DayExposure ("Day Exposure", Range(0,2)) = 1.0
        _NightExposure ("Night Exposure", Range(0,2)) = 0.5
        _PhaseTintStrength ("Phase Tint Strength", Range(0,1)) = 0.75
        _NightMinBrightness ("Night Min Brightness", Range(0,1)) = 0.08
        _ColorizeStrength ("Colorize Strength", Range(0,1)) = 0.7
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "DayNightFilter"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            ZWrite Off
            ZTest Always
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _DayTint;
                float4 _NightTint;
                float4 _DawnTint;
                float4 _DuskTint;
                float _FilterStrength;
                float _DaySaturation;
                float _NightSaturation;
                float _DayContrast;
                float _NightContrast;
                float _DayExposure;
                float _NightExposure;
                float _PhaseTintStrength;
                float _NightMinBrightness;
                float _ColorizeStrength;
            CBUFFER_END

            float _Moyva_DayNightLerp;
            float _Moyva_DayPhase;

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

            float3 ApplySaturation(float3 color, float saturation)
            {
                float luma = dot(color, float3(0.2126, 0.7152, 0.0722));
                return lerp(luma.xxx, color, saturation);
            }

            float3 ApplyContrast(float3 color, float contrast)
            {
                return (color - 0.5) * contrast + 0.5;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float4 scene = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv);

                float t = saturate(_Moyva_DayNightLerp);
                float phase = round(_Moyva_DayPhase);

                float3 baseTint = lerp(_NightTint.rgb, _DayTint.rgb, t);
                float dawnMask = 1.0 - step(0.5, abs(phase - 1.0));
                float duskMask = 1.0 - step(0.5, abs(phase - 3.0));
                float phaseStrength = saturate(_PhaseTintStrength);
                float3 tint = lerp(baseTint, _DawnTint.rgb, dawnMask * phaseStrength);
                tint = lerp(tint, _DuskTint.rgb, duskMask * phaseStrength);

                float sat = lerp(_NightSaturation, _DaySaturation, t);
                float contrast = lerp(_NightContrast, _DayContrast, t);
                float exposure = lerp(_NightExposure, _DayExposure, t);
                float nightWeight = 1.0 - t;
                float minBrightness = saturate(_NightMinBrightness);
                float brightness = lerp(1.0, minBrightness, nightWeight);

                float3 graded = scene.rgb * tint * exposure;
                graded = ApplySaturation(graded, sat);
                graded = ApplyContrast(graded, contrast);
                graded *= brightness;

                float colorizeStrength = saturate(_ColorizeStrength);
                float luminance = dot(graded, float3(0.2126, 0.7152, 0.0722));
                float3 targetColorized = luminance * tint;
                graded = lerp(graded, targetColorized, colorizeStrength);

                float alpha = saturate(_FilterStrength);
                return half4(lerp(scene.rgb, graded, alpha), scene.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "CopyBack"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            ZWrite Off
            ZTest Always
            Cull Off
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragCopy

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

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

            half4 FragCopy(Varyings input) : SV_Target
            {
                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, input.uv);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
