Shader "Moyva/2D/InfluenceRadius"
{
    Properties
    {
        _Color       ("Border Color",        Color)  = (1.0, 1.0, 1.0, 1.0)
        _FillColor   ("Fill Color",          Color)  = (1.0, 1.0, 1.0, 0.04)
        _BorderWidth ("Ring Width (World)",  Float)  = 0.5
        _DashLen     ("Dash Length (World)", Float)  = 0.9
        _GapLen      ("Gap Length (World)",  Float)  = 0.55
        _Speed       ("Dash Speed",          Float)  = 1.25
        _MapRect     ("Map Rect XYXY",     Vector) = (-9999, -9999, 9999, 9999)
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "Queue"          = "Transparent+10"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "InfluenceRadius"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex   vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _FillColor;
                float  _BorderWidth;
                float  _DashLen;
                float  _GapLen;
                float  _Speed;
                float4 _MapRect;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float2 worldXY     : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.worldXY = worldPos.xy;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float insideMap = step(_MapRect.x, IN.worldXY.x)
                    * step(IN.worldXY.x, _MapRect.z)
                    * step(_MapRect.y, IN.worldXY.y)
                    * step(IN.worldXY.y, _MapRect.w);
                clip(insideMap - 0.5);

                float2 centerWS = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xy;
                float2 dir = IN.worldXY - centerWS;
                float distanceToCenter = length(dir);
                float ringRadius = min(
                    length(mul(unity_ObjectToWorld, float4(0.5, 0.0, 0.0, 0.0)).xy),
                    length(mul(unity_ObjectToWorld, float4(0.0, 0.5, 0.0, 0.0)).xy));

                float halfWidth = max(_BorderWidth * 0.5, 0.0001);
                float edgeDistance = abs(distanceToCenter - ringRadius);
                float edgeFeather = max(fwidth(edgeDistance), 0.001);
                float ringMask = 1.0 - smoothstep(halfWidth - edgeFeather, halfWidth + edgeFeather, edgeDistance);
                float fillMask = 1.0 - smoothstep(ringRadius - halfWidth - edgeFeather, ringRadius - halfWidth + edgeFeather, distanceToCenter);

                clip(max(ringMask, fillMask) - 0.001);

                if (ringMask <= 0.0)
                    return float4(_FillColor.rgb, _FillColor.a * fillMask);

                const float TAU = 6.28318530718;
                float angle = atan2(dir.y, dir.x);
                float normalizedAngle = frac((angle + PI) / TAU);
                float perimeterPosition = normalizedAngle * (TAU * max(ringRadius, 0.0001));

                float period = max(_DashLen + _GapLen, 0.0001);
                float dashPhase = fmod(perimeterPosition - _Time.y * _Speed + period * 1024.0, period);
                float onDash = step(dashPhase, _DashLen);

                return float4(_Color.rgb, _Color.a * ringMask * onDash);
            }
            ENDHLSL
        }
    }
}
