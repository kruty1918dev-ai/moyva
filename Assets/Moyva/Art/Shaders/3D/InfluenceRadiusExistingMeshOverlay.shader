Shader "Moyva/3D/InfluenceRadiusExistingMeshOverlay"
{
    Properties
    {
        _Color       ("Border Color",        Color)  = (1.0, 1.0, 1.0, 1.0)
        _FillColor   ("Fill Color",          Color)  = (1.0, 1.0, 1.0, 0.055)
        _BorderWidth ("Ring Width (World)",  Float)  = 0.5
        _DashLen     ("Dash Length (World)", Float)  = 0.9
        _GapLen      ("Gap Length (World)",  Float)  = 0.55
        _Speed       ("Dash Speed",          Float)  = 1.25
        _CenterXZ    ("Center XZ",           Vector) = (0, 0, 0, 0)
        _HalfExtent  ("Half Extent",         Float)  = 1.5
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "Queue"          = "Transparent+30"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "InfluenceRadiusExistingMeshOverlay"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            Offset -2, -2

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _FillColor;
                float  _BorderWidth;
                float  _DashLen;
                float  _GapLen;
                float  _Speed;
                float4 _CenterXZ;
                float  _HalfExtent;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 worldXZ     : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.worldXZ = worldPos.xz;
                return OUT;
            }

            float ResolveSquarePerimeterPosition(float2 rel, float halfExtent)
            {
                float2 absRel = abs(rel);
                if (absRel.x >= absRel.y)
                {
                    if (rel.x >= 0.0)
                        return halfExtent + rel.y;

                    return 5.0 * halfExtent - rel.y;
                }

                if (rel.y >= 0.0)
                    return 3.0 * halfExtent - rel.x;

                return 7.0 * halfExtent + rel.x;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float halfExtent = max(_HalfExtent, 0.0001);
                float2 rel = IN.worldXZ - _CenterXZ.xy;
                float2 absRel = abs(rel);
                float squareDistance = max(absRel.x, absRel.y);
                float inside = 1.0 - step(halfExtent, squareDistance);
                clip(inside - 0.001);

                float edgeDistance = halfExtent - squareDistance;
                float borderWidth = max(_BorderWidth, 0.0001);
                float edgeFeather = max(fwidth(edgeDistance), 0.001);
                float ringMask = 1.0 - smoothstep(borderWidth - edgeFeather, borderWidth + edgeFeather, edgeDistance);
                float fillMask = 1.0 - ringMask;

                if (ringMask <= 0.001)
                    return float4(_FillColor.rgb, _FillColor.a * fillMask);

                float period = max(_DashLen + _GapLen, 0.0001);
                float perimeterPosition = ResolveSquarePerimeterPosition(rel, halfExtent);
                float dashPhase = fmod(perimeterPosition - _Time.y * _Speed + period * 1024.0, period);
                float onDash = step(dashPhase, _DashLen);

                return float4(_Color.rgb, _Color.a * ringMask * onDash);
            }
            ENDHLSL
        }
    }
}