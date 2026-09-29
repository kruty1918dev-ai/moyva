Shader "Moyva/Waterfall Flow"
{
    Properties
    {
        _BaseColor ("Water", Color) = (0.18, 0.55, 0.61, 1)
        _FoamColor ("Foam", Color) = (0.8, 0.94, 0.93, 1)
        _FoamTex ("SW3 foam", 2D) = "white" {}
        _Direction ("Flow direction and speed", Vector) = (0,-1,0,0)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+2" }
        Pass
        {
            Name "Waterfall"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_FoamTex);
            SAMPLER(sampler_FoamTex);
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _FoamColor;
                float4 _Direction;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; float2 flow : TEXCOORD1; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 normalWS : TEXCOORD0; float2 uv : TEXCOORD1; float2 flow : TEXCOORD2; half fog : TEXCOORD3; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = position.positionCS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                output.flow = input.flow;
                output.fog = ComputeFogFactor(position.positionCS.z);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                // Longitudinal UV decreases down the fall. Advect both foam
                // layers downward at different speeds, in world metre units.
                float speed = max(0.05, length(_Direction.xy));
                float2 p = input.uv * float2(0.85, 0.3);
                float time = _Time.y * speed;
                half a = SAMPLE_TEXTURE2D(_FoamTex, sampler_FoamTex, p + float2(0, time * 0.7)).r;
                half b = SAMPLE_TEXTURE2D(_FoamTex, sampler_FoamTex,
                    p * float2(1.8, 1.4) + float2(0.31, time * 1.15)).g;
                half streak = smoothstep(0.3, 0.78, a * 0.7 + b * 0.3);
                half lip = 1 - smoothstep(0, 0.18, abs(input.flow.y));
                half impact = 1 - smoothstep(0, 0.3, abs(input.flow.y - 1));
                half foam = saturate(streak * 0.65 + lip * 0.3 + impact * (0.25 + streak * 0.3));
                Light light = GetMainLight();
                half illumination = 0.78 + 0.22 * abs(dot(normalize(input.normalWS), light.direction));
                half3 color = lerp(_BaseColor.rgb * illumination, _FoamColor.rgb, foam);
                color = MixFog(color, input.fog);
                // No scene-depth tint or refraction: those make a vertical
                // waterfall read as a dark solid wall against the cliff.
                half surfaceWing = step(input.flow.y, 0) + step(1, input.flow.y);
                half edgeFade = smoothstep(-0.3, -0.04, input.flow.y)
                    * (1 - smoothstep(1.04, 1.3, input.flow.y));
                half wingAlpha = saturate(streak * 0.7 + max(lip, impact) * 0.4);
                color = lerp(color, _FoamColor.rgb, saturate(surfaceWing) * 0.7);
                return half4(color, lerp(lerp(0.88, 0.98, foam), wingAlpha,
                    saturate(surfaceWing)) * edgeFade);
            }
            ENDHLSL
        }
    }
}
