Shader "Hidden/Moyva/Grass Card Preview"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        _MainTex("Main Tex", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _Color("Color", Color) = (1, 1, 1, 1)
        _Alpha("Alpha", Range(0, 1)) = 1
        _AlphaClipThreshold("Alpha Clip Threshold", Range(0, 1)) = 0.35
        _Cutoff("Cutoff", Range(0, 1)) = 0.35
        _TextureFill("Texture Fill XY", Vector) = (1, 1, 0, 0)
        _TextureFillOffset("Texture Fill Offset XY", Vector) = (0, 0, 0, 0)
        _TextureFitClamp("Clamp Outside Texture Fill", Range(0, 1)) = 1
        [Toggle] _BillboardEnabled("Face Camera Billboard", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull Mode", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "PreviewURP"
            Tags { "LightMode" = "UniversalForward" }

            Cull [_CullMode]
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _Color;
                half _Alpha;
                half _AlphaClipThreshold;
                half _Cutoff;
                half4 _TextureFill;
                half4 _TextureFillOffset;
                half _TextureFitClamp;
                half _BillboardEnabled;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float2 ApplyTextureFit(float2 uv)
            {
                float2 fill = max(abs(_TextureFill.xy), float2(0.001, 0.001));
                return (uv - 0.5) / fill + 0.5 + _TextureFillOffset.xy;
            }

            half TextureBoundsMask(float2 uv)
            {
                half2 aboveMin = step(float2(0.0, 0.0), uv);
                half2 belowMax = step(uv, float2(1.0, 1.0));
                half inside = aboveMin.x * aboveMin.y * belowMax.x * belowMax.y;
                return lerp(1.0, inside, saturate(_TextureFitClamp));
            }

            float3 GetBillboardRightWS()
            {
                float3 rightWS = float3(UNITY_MATRIX_I_V._m00, 0.0, UNITY_MATRIX_I_V._m20);
                float rightLenSq = dot(rightWS, rightWS);
                return rightLenSq > 0.0001 ? normalize(rightWS) : float3(1.0, 0.0, 0.0);
            }

            float3 TransformBillboardPositionWS(float3 positionOS)
            {
                float3 pivotWS = TransformObjectToWorld(float3(0.0, 0.0, 0.0));
                float rightScale = length(TransformObjectToWorld(float3(1.0, 0.0, 0.0)) - pivotWS);
                float upScale = length(TransformObjectToWorld(float3(0.0, 1.0, 0.0)) - pivotWS);
                return pivotWS
                    + GetBillboardRightWS() * positionOS.x * rightScale
                    + float3(0.0, 1.0, 0.0) * positionOS.y * upScale;
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 meshPositionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 billboardPositionWS = TransformBillboardPositionWS(input.positionOS.xyz);
                output.positionHCS = TransformWorldToHClip(lerp(meshPositionWS, billboardPositionWS, saturate(_BillboardEnabled)));
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 fittedUv = ApplyTextureFit(input.uv);
                half boundsMask = TextureBoundsMask(fittedUv);
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, fittedUv);
                half4 color = tex * _BaseColor * _Color;
                color.a *= _Alpha * boundsMask;
                clip(color.a - max(_AlphaClipThreshold, _Cutoff));
                return color;
            }
            ENDHLSL
        }
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "PreviewBuiltin"

            Cull Off
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _BaseMap;
            float4 _BaseMap_ST;
            fixed4 _BaseColor;
            fixed4 _Color;
            fixed _Alpha;
            fixed _AlphaClipThreshold;
            fixed _Cutoff;
            float4 _TextureFill;
            float4 _TextureFillOffset;
            fixed _TextureFitClamp;
            fixed _BillboardEnabled;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float2 ApplyTextureFit(float2 uv)
            {
                float2 fill = max(abs(_TextureFill.xy), float2(0.001, 0.001));
                return (uv - 0.5) / fill + 0.5 + _TextureFillOffset.xy;
            }

            fixed TextureBoundsMask(float2 uv)
            {
                fixed2 aboveMin = step(float2(0.0, 0.0), uv);
                fixed2 belowMax = step(uv, float2(1.0, 1.0));
                fixed inside = aboveMin.x * aboveMin.y * belowMax.x * belowMax.y;
                return lerp(1.0, inside, saturate(_TextureFitClamp));
            }

            float3 GetBillboardRightWS()
            {
                float3 rightWS = float3(UNITY_MATRIX_I_V._m00, 0.0, UNITY_MATRIX_I_V._m20);
                float rightLenSq = dot(rightWS, rightWS);
                return rightLenSq > 0.0001 ? normalize(rightWS) : float3(1.0, 0.0, 0.0);
            }

            float3 TransformObjectToWorldPoint(float3 positionOS)
            {
                return mul(unity_ObjectToWorld, float4(positionOS, 1.0)).xyz;
            }

            float3 TransformBillboardPositionWS(float3 positionOS)
            {
                float3 pivotWS = TransformObjectToWorldPoint(float3(0.0, 0.0, 0.0));
                float rightScale = length(TransformObjectToWorldPoint(float3(1.0, 0.0, 0.0)) - pivotWS);
                float upScale = length(TransformObjectToWorldPoint(float3(0.0, 1.0, 0.0)) - pivotWS);
                return pivotWS
                    + GetBillboardRightWS() * positionOS.x * rightScale
                    + float3(0.0, 1.0, 0.0) * positionOS.y * upScale;
            }

            v2f vert(appdata input)
            {
                v2f output;
                float3 meshPositionWS = TransformObjectToWorldPoint(input.vertex.xyz);
                float3 billboardPositionWS = TransformBillboardPositionWS(input.vertex.xyz);
                float3 positionWS = lerp(meshPositionWS, billboardPositionWS, saturate(_BillboardEnabled));
                output.vertex = mul(UNITY_MATRIX_VP, float4(positionWS, 1.0));
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float2 fittedUv = ApplyTextureFit(input.uv);
                fixed boundsMask = TextureBoundsMask(fittedUv);
                fixed4 tex = tex2D(_BaseMap, fittedUv);
                fixed4 color = tex * _BaseColor * _Color;
                color.a *= _Alpha * boundsMask;
                clip(color.a - max(_AlphaClipThreshold, _Cutoff));
                return color;
            }
            ENDCG
        }
    }

    Fallback Off
}
