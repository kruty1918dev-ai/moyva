Shader "Moyva/UI/GaussianBlur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _BlurSourceTex ("Blur Source", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _BlurStrength ("Blur Strength", Range(0,4)) = 1
        _BlurRadius ("Blur Radius", Range(0,3)) = 2
        _Sigma ("Sigma", Range(0.1,4)) = 1.2
        _BlurSourceAvailable ("Blur Source Available", Float) = 0

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
            Name "Default"
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
            sampler2D _BlurSourceTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _BlurSourceTex_TexelSize;

            float _BlurStrength;
            int _BlurRadius;
            float _Sigma;
            float _BlurSourceAvailable;

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

            inline float GaussianWeight(int x, int y, float sigma)
            {
                float2 p = float2((float)x, (float)y);
                float sigma2 = sigma * sigma;
                return exp(-dot(p, p) / (2.0 * sigma2));
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 mask = tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd;

                if (_BlurSourceAvailable < 0.5)
                {
                    fixed4 fallbackColor = mask * IN.color;

                    #ifdef UNITY_UI_CLIP_RECT
                    fallbackColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                    #endif

                    #ifdef UNITY_UI_ALPHACLIP
                    clip(fallbackColor.a - 0.001);
                    #endif

                    return fallbackColor;
                }

                const int maxRadius = 3;
                int radius = clamp(_BlurRadius, 0, maxRadius);
                float sigma = max(_Sigma, 0.1);
                float2 screenUv = IN.screenPos.xy / max(IN.screenPos.w, 0.0001);
                float2 texel = _BlurSourceTex_TexelSize.xy * _BlurStrength;

                float4 accum = 0;
                float weightSum = 0;

                [loop]
                for (int y = -maxRadius; y <= maxRadius; y++)
                {
                    [loop]
                    for (int x = -maxRadius; x <= maxRadius; x++)
                    {
                        if (abs(x) > radius || abs(y) > radius)
                            continue;

                        float w = GaussianWeight(x, y, sigma);
                        float2 sampleUv = saturate(screenUv + float2((float)x, (float)y) * texel);
                        accum += tex2D(_BlurSourceTex, sampleUv) * w;
                        weightSum += w;
                    }
                }

                fixed4 color;
                color.rgb = (accum.rgb / max(weightSum, 1e-5)) * IN.color.rgb;
                color.a = mask.a * IN.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}