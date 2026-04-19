Shader "Moyva/FogOfWar"
{
    Properties
    {
        _FogTex          ("Fog Texture (R8)", 2D)    = "black" {}
        _UnexploredColor ("Unexplored Color", Color) = (0, 0, 0, 1)
        _ExploredColor   ("Explored Color",   Color) = (0, 0, 0, 0.5)
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "Queue"          = "Overlay"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "FogOverlay"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_FogTex);
            SAMPLER(sampler_FogTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _FogTex_ST;
                float4 _UnexploredColor;
                float4 _ExploredColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _FogTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // R8: 0 = Unexplored, 128/255 ≈ 0.502 = Explored, 255/255 = Visible
                float fogVal = SAMPLE_TEXTURE2D(_FogTex, sampler_FogTex, IN.uv).r;

                // Visible → fully transparent
                float isVisible = step(0.9, fogVal);

                // Explored → semi-transparent fog
                float isExplored = step(0.3, fogVal) * (1.0 - isVisible);

                // Unexplored → fully opaque fog
                float isUnexplored = 1.0 - step(0.3, fogVal);

                half4 col;
                col.rgb = _UnexploredColor.rgb * isUnexplored + _ExploredColor.rgb * isExplored;
                col.a   = _UnexploredColor.a   * isUnexplored + _ExploredColor.a   * isExplored;

                return col;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
