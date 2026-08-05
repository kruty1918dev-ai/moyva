Shader "Moyva/2D/SpriteWaterOccluder"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [Toggle] _WriteWaterMask ("Write Water Mask (Depth+Stencil)", Float) = 1
        _WaterMaskCutoff ("Water Mask Alpha Cutoff", Range(0, 1)) = 0.1
        // Має збігатись з _ShoreStencilRef у LayerMipLod (дефолт 1),
        // щоб тест NotEqual у воді відсікав пікселі під цим об'єктом.
        _WaterStencilRef ("Water Stencil Ref", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "RenderPipeline"="UniversalPipeline"
        }

        Cull Off
        Lighting Off

        // Prepass: writes depth+stencil by sprite alpha.
        // LightMode=DepthOnly tells URP forward renderer to include this pass
        // in the depth prepass, so the object appears in _CameraDepthTexture
        // and ComputeDepthEdge / ComputeShoreDepthBand detect it.
        Pass
        {
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask 0
            Blend Off

            Stencil
            {
                Ref [_WaterStencilRef]
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment fragMask
            #pragma target 2.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float _WriteWaterMask;
            float _WaterMaskCutoff;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 fragMask(Varyings i) : SV_Target
            {
                clip(_WriteWaterMask - 0.5);
                half a = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).a;
                clip(a - _WaterMaskCutoff);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            ZWrite Off
            Blend One OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            half4 _Color;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * i.color;
                c.rgb *= c.a;
                return c;
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
