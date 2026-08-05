// Internal blit shader used by ShoreMaskPrepass.
// Samples _MainTex alpha and writes it into the R channel of the destination RT.
// Uses additive blending so multiple layers are accumulated (any land = 1).
Shader "Moyva/2D/Internal/AlphaToRed"
{
    Properties
    {
        _MainTex ("Source Texture", 2D) = "white" {}
        _UvOffset ("UV Offset", Vector) = (0,0,0,0)
        _UvScale ("UV Scale", Vector) = (1,1,0,0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Cull Off
        ZWrite Off
        ZTest Always

        // Additive on R: dst.r = saturate(dst.r + src.a)
        // Max is more correct (any land pixel → 1):
        BlendOp Max
        Blend One One

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _UvOffset;
            float4 _UvScale;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
                return o;
            }

            // Виводимо alpha у R, все інше = 0.
            // Soft edge fade замість hard if-branch для плавних переходів.
            half4 frag(Varyings i) : SV_Target
            {
                float2 scale = max(_UvScale.xy, float2(1e-5, 1e-5));
                float2 uv = (i.uv - _UvOffset.xy) / scale;

                // Плавне згасання на краях (1 texel feather) замість hard clip
                float2 texel = fwidth(uv);
                float edgeFade = smoothstep(0.0, texel.x, uv.x)
                               * smoothstep(0.0, texel.x, 1.0 - uv.x)
                               * smoothstep(0.0, texel.y, uv.y)
                               * smoothstep(0.0, texel.y, 1.0 - uv.y);

                half a = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, saturate(uv)).a;
                return half4(a * edgeFade, 0, 0, 1);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
