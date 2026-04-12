Shader "Moyva/2D/InfluenceRadius"
{
    Properties
    {
        _Color       ("Border Color",      Color)  = (0.35, 1.0, 0.35, 1.0)
        _FillColor   ("Fill Color",        Color)  = (0.35, 1.0, 0.35, 0.06)
        _BorderWidth ("Border Width (UV)", Float)  = 0.04
        _DashLen     ("Dash Length",       Float)  = 0.08
        _GapLen      ("Gap Length",        Float)  = 0.04
        _Speed       ("Animation Speed",   Float)  = 0.25
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
                float2 uv = IN.uv;

                // Маска меж мапи: обрізаємо все поза прямокутником тайлів
                if (IN.worldXY.x < _MapRect.x || IN.worldXY.x > _MapRect.z ||
                    IN.worldXY.y < _MapRect.y || IN.worldXY.y > _MapRect.w)
                    return float4(0, 0, 0, 0);

                // Відстань до кожного краю квадрата (0 = на краю, 0.5 = центр)
                float2 fromEdge = min(uv, 1.0 - uv);
                float  edgeDist = min(fromEdge.x, fromEdge.y);

                // Заливка всередині — повертаємо напівпрозорий колір
                if (edgeDist >= _BorderWidth)
                    return _FillColor;

                // Визначаємо позицію вздовж периметру [0..4) за годинниковою стрілкою:
                // нижній: 0→1, правий: 1→2, верхній: 2→3, лівий: 3→4
                float p;
                float ex = fromEdge.x, ey = fromEdge.y;

                if (ey < ex)
                {
                    // горизонтальний край
                    if (uv.y < 0.5) p = uv.x;                 // нижній: зліва→праворуч
                    else            p = 2.0 + (1.0 - uv.x);   // верхній: праворуч→зліва
                }
                else
                {
                    // вертикальний край
                    if (uv.x >= 0.5) p = 1.0 + uv.y;          // правий: знизу→вгору
                    else             p = 3.0 + (1.0 - uv.y);  // лівий: вгору→знизу
                }

                // Анімовані штрихи: зміщення периметра в часі
                float period = _DashLen + _GapLen;
                float t      = fmod(p - _Time.y * _Speed + 1000.0, period);
                float onDash = step(t, _DashLen);

                return float4(_Color.rgb, _Color.a * onDash);
            }
            ENDHLSL
        }
    }
}
