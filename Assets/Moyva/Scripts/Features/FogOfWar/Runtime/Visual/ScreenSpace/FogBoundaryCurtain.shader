Shader "Moyva/FogOfWar/BoundaryCurtain"
{
    Properties
    {
        _TopColor(
            "Top Color",
            Color) =
            (0.03, 0.05, 0.06, 1)

        _BottomColor(
            "Bottom Color",
            Color) =
            (0.09, 0.14, 0.17, 1)

        _TopBandFraction(
            "Top Band Fraction",
            Range(0.01, 0.35)) =
            0.05

        _GradientPower(
            "Gradient Power",
            Range(0.25, 4.0)) =
            0.8

        [Enum(UnityEngine.Rendering.CullMode)]
        _CullMode(
            "Cull Mode",
            Float) =
            2

    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" =
                "UniversalPipeline"

            "RenderType" =
                "Opaque"

            "Queue" =
                "Geometry+100"
        }

        Pass
        {
            Name "MoyvaFogCurtain"

            Tags
            {
                "LightMode" =
                    "MoyvaFogCurtain"
            }

            Cull [_CullMode]

            /*
             * Boundary depth має бути поверх усієї world-сцени:
             * terrain, water, buildings, decals та transparent effects.
             *
             * Основний сірий fog рендериться окремим fullscreen pass
             * пізніше і тому перекриває curtain.
             */
            ZWrite Off
            ZTest Always
            Blend Off

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)

                float4 _TopColor;
                float4 _BottomColor;
                float _TopBandFraction;
                float _GradientPower;
                float _DebugVisualMode;

            CBUFFER_END

            struct Attributes
            {
                float4 positionOS
                    : POSITION;

                float2 uv
                    : TEXCOORD0;

                float4 color
                    : COLOR;
            };

            struct Varyings
            {
                float4 positionCS
                    : SV_POSITION;

                float2 uv
                    : TEXCOORD0;

                float4 color
                    : COLOR;
            };

            Varyings Vert(
                Attributes input)
            {
                Varyings output;

                output.positionCS =
                    TransformObjectToHClip(
                        input.positionOS.xyz);

                output.uv =
                    input.uv;

                output.color =
                    input.color;

                return output;
            }

            half4 Frag(
                Varyings input)
                : SV_Target
            {
                if (_DebugVisualMode > 0.5)
                {
                    return half4(
                        input.color.rgb,
                        1.0);
                }

                float vertical =
                    saturate(
                        input.uv.y);

                float gradient =
                    pow(
                        vertical,
                        max(
                            0.25,
                            _GradientPower));

                float4 color =
                    lerp(
                        _BottomColor,
                        _TopColor,
                        gradient);

                float topBandStart =
                    1.0
                    - saturate(
                        _TopBandFraction);

                float topBand =
                    smoothstep(
                        topBandStart,
                        1.0,
                        vertical);

                color.rgb =
                    lerp(
                        color.rgb,
                        _TopColor.rgb,
                        topBand);

                color.a =
                    1.0;

                return color;
            }

            ENDHLSL
        }
    }

    FallBack Off
}
