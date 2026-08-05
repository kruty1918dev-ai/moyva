Shader "Moyva/UI/BackgroundBlur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Mask Texture", 2D) = "white" {}
        _BackgroundTex ("Background Texture", 2D) = "black" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _BlurSize ("Blur Size", Range(0, 20)) = 6
        _Downsample ("Downsample", Range(1, 8)) = 2
        _Opacity ("Opacity", Range(0, 1)) = 1
        _BackgroundAvailable ("Background Available", Float) = 0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UIBackgroundBlur"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4 screenPos     : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _BackgroundTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _BackgroundTex_TexelSize;

            float _BlurSize;
            float _Downsample;
            float _Opacity;
            float _BackgroundAvailable;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                OUT.screenPos = ComputeScreenPos(OUT.vertex);

                return OUT;
            }

            inline float GaussianWeight(float x, float sigma)
            {
                float sigma2 = max(0.0001, sigma * sigma);
                return exp(-(x * x) / (2.0 * sigma2));
            }

            float3 BlurHorizontal(float2 uv, float2 texel, float sigma)
            {
                const int radius = 4;

                float3 accum = 0;
                float weightSum = 0;

                [unroll]
                for (int x = -radius; x <= radius; x++)
                {
                    float w = GaussianWeight((float)x, sigma);
                    float2 sampleUv = saturate(uv + float2((float)x, 0) * texel);
                    accum += tex2D(_BackgroundTex, sampleUv).rgb * w;
                    weightSum += w;
                }

                return accum / max(weightSum, 1e-5);
            }

            float3 BlurVertical(float2 uv, float2 texel, float sigma)
            {
                const int radius = 4;

                float3 accum = 0;
                float weightSum = 0;

                [unroll]
                for (int y = -radius; y <= radius; y++)
                {
                    float w = GaussianWeight((float)y, sigma);
                    float2 sampleUv = saturate(uv + float2(0, (float)y) * texel);
                    accum += tex2D(_BackgroundTex, sampleUv).rgb * w;
                    weightSum += w;
                }

                return accum / max(weightSum, 1e-5);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 mask = tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd;
                float2 uv = IN.screenPos.xy / max(IN.screenPos.w, 1e-5);

                float4 outputColor;

                if (_BackgroundAvailable < 0.5)
                {
                    outputColor.rgb = mask.rgb * IN.color.rgb;
                    outputColor.a = mask.a * IN.color.a * _Opacity;
                }
                else
                {
                    float blurScale = max(0.0, _BlurSize) * max(1.0, _Downsample);
                    float sigma = max(0.6, _BlurSize * 0.25 + 0.8);
                    float2 texel = _BackgroundTex_TexelSize.xy * blurScale;

                    float3 horiz = BlurHorizontal(uv, texel, sigma);
                    float3 vert = BlurVertical(uv, texel, sigma);
                    float3 blurred = (horiz + vert) * 0.5;

                    outputColor.rgb = blurred * IN.color.rgb;
                    outputColor.a = mask.a * IN.color.a * _Opacity;
                }

                #ifdef UNITY_UI_CLIP_RECT
                outputColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(outputColor.a - 0.001);
                #endif

                return outputColor;
            }
            ENDCG
        }
    }
}
