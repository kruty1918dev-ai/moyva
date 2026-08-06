Shader "Moyva/FogOfWar/SurfaceDepth"
{
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
        }

        Pass
        {
            Name "FogSurfaceDepth"

            ZWrite On
            ZTest LEqual
            Cull Off
            Blend Off

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.5
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float eyeDepth : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 positionWS =
                    TransformObjectToWorld(
                        input.positionOS.xyz);

                float3 positionVS =
                    TransformWorldToView(
                        positionWS);

                output.positionCS =
                    TransformWorldToHClip(
                        positionWS);

                output.eyeDepth =
                    max(
                        0.0,
                        -positionVS.z);

                return output;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                return float4(
                    input.eyeDepth,
                    0.0,
                    0.0,
                    1.0);
            }

            ENDHLSL
        }
    }

    FallBack Off
}
