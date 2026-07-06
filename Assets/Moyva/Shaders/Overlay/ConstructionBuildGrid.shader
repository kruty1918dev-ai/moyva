Shader "Moyva/Overlay/ConstructionBuildGrid"
{
    Properties
    {
        _LineColor ("Line Color", Color) = (0.70, 0.95, 1.00, 0.22)
        _FillColor ("Fill Color", Color) = (0.70, 0.95, 1.00, 0.045)
        _LineWidth ("Line Width", Range(0.005, 0.49)) = 0.035
        _EdgeMask ("Edge Mask", Vector) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "Queue"          = "Transparent+990"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ConstructionBuildGrid"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            Offset -1, -1

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _LineColor;
                float4 _FillColor;
                float4 _EdgeMask;
                float _LineWidth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float feather = max(fwidth(IN.uv.x) + fwidth(IN.uv.y), 0.0015);
                float leftLine = 1.0 - smoothstep(_LineWidth - feather, _LineWidth + feather, IN.uv.x);
                float bottomLine = 1.0 - smoothstep(_LineWidth - feather, _LineWidth + feather, IN.uv.y);
                float rightLine = 1.0 - smoothstep(_LineWidth - feather, _LineWidth + feather, 1.0 - IN.uv.x);
                float topLine = 1.0 - smoothstep(_LineWidth - feather, _LineWidth + feather, 1.0 - IN.uv.y);
                float lineMask = max(max(leftLine * _EdgeMask.x, bottomLine * _EdgeMask.y), max(rightLine * _EdgeMask.z, topLine * _EdgeMask.w));

                float3 rgb = lerp(_FillColor.rgb, _LineColor.rgb, lineMask);
                float alpha = max(_FillColor.a, _LineColor.a * lineMask);
                return float4(rgb, alpha);
            }
            ENDHLSL
        }
    }
}
