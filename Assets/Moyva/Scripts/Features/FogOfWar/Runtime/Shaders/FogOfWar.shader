Shader "Moyva/FogOfWar"
{
    Properties
    {
        _FogTex             ("Fog Texture (R8)",    2D)     = "black" {}
        _UnexploredColor    ("Unexplored Color",    Color)  = (0.03, 0.03, 0.08, 1.0)
        _ExploredColor      ("Explored Color",      Color)  = (0.08, 0.10, 0.14, 0.65)
        _NoiseScaleA        ("Noise Scale A",       Float)  = 3.5
        _NoiseSpeedA        ("Noise Speed A",       Float)  = 0.04
        _NoiseStrengthA     ("Noise Strength A",    Float)  = 0.25
        _NoiseScaleB        ("Noise Scale B",       Float)  = 2.0
        _NoiseSpeedB        ("Noise Speed B",       Float)  = 0.02
        _NoiseStrengthB     ("Noise Strength B",    Float)  = 0.15
        _EdgeBleedRadius    ("Edge Bleed Radius",   Float)  = 0.35
        _EdgeBleedStrength  ("Edge Bleed Strength", Float)  = 0.40
        _TransitionSoftness ("Transition Softness", Float)  = 0.12
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "Queue"          = "Transparent+100"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // ── Textures & Samplers ──────────────────────────────────────────
            TEXTURE2D(_FogTex);
            SAMPLER(sampler_FogTex);

            // ── Constant Buffer ──────────────────────────────────────────────
            CBUFFER_START(UnityPerMaterial)
                float4 _FogTex_ST;
                float4 _FogTex_TexelSize;   // (1/w, 1/h, w, h)
                float4 _UnexploredColor;
                float4 _ExploredColor;
                float  _NoiseScaleA;
                float  _NoiseSpeedA;
                float  _NoiseStrengthA;
                float  _NoiseScaleB;
                float  _NoiseSpeedB;
                float  _NoiseStrengthB;
                float  _EdgeBleedRadius;
                float  _EdgeBleedStrength;
                float  _TransitionSoftness;
            CBUFFER_END

            // ── Vertex Data ──────────────────────────────────────────────────
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            // ── Hash & Gradient Noise ────────────────────────────────────────

            // Simple 2D hash → pseudo-random float in [-1,1]
            float2 Hash2(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)),
                           dot(p, float2(269.5, 183.3)));
                return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
            }

            // Gradient noise (Simplex-style, but on a grid)
            float GradNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f); // smoothstep

                return lerp(lerp(dot(Hash2(i + float2(0,0)), f - float2(0,0)),
                                 dot(Hash2(i + float2(1,0)), f - float2(1,0)), u.x),
                            lerp(dot(Hash2(i + float2(0,1)), f - float2(0,1)),
                                 dot(Hash2(i + float2(1,1)), f - float2(1,1)), u.x),
                            u.y);
            }

            // 2-octave fBm
            float fBm2(float2 p)
            {
                float v  = 0.0;
                float a  = 0.5;
                float2 s = float2(1.0, 1.0);
                for (int i = 0; i < 2; i++)
                {
                    v += a * GradNoise(p * s);
                    s *= 2.0;
                    a *= 0.5;
                }
                return v * 0.5 + 0.5; // remap [-1,1] → [0,1]
            }

            // ── Vertex Shader ────────────────────────────────────────────────
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _FogTex);
                return OUT;
            }

            // ── Fragment Shader ──────────────────────────────────────────────
            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // Sample centre fog value
                float fogVal = SAMPLE_TEXTURE2D(_FogTex, sampler_FogTex, uv).r;

                // ── Edge Bleeding ────────────────────────────────────────────
                // Sample 4 neighbours; if any neighbour is darker, pull fog in
                float2 ts = _FogTex_TexelSize.xy * _EdgeBleedRadius;
                float n0 = SAMPLE_TEXTURE2D(_FogTex, sampler_FogTex, uv + float2( ts.x, 0)).r;
                float n1 = SAMPLE_TEXTURE2D(_FogTex, sampler_FogTex, uv + float2(-ts.x, 0)).r;
                float n2 = SAMPLE_TEXTURE2D(_FogTex, sampler_FogTex, uv + float2(0,  ts.y)).r;
                float n3 = SAMPLE_TEXTURE2D(_FogTex, sampler_FogTex, uv + float2(0, -ts.y)).r;
                float minNeighbour = min(min(n0, n1), min(n2, n3));
                float bleed = saturate((fogVal - minNeighbour) * _EdgeBleedStrength);
                fogVal = fogVal - bleed;

                // ── Zone thresholds ──────────────────────────────────────────
                // R8: 0=Unexplored(0), 128/255≈0.502=Explored, 255/255=1=Visible
                float softness = _TransitionSoftness;
                float exploredLo = 0.502 - softness;
                float exploredHi = 0.502 + softness;
                float visibleLo  = 1.0   - softness;

                // Blend weights
                float wUnexplored = 1.0 - smoothstep(exploredLo, exploredHi, fogVal);
                float wVisible    = smoothstep(visibleLo, 1.0, fogVal);
                float wExplored   = 1.0 - wUnexplored - wVisible;
                wExplored = saturate(wExplored);

                // ── Perlin Noise ─────────────────────────────────────────────
                float t = _Time.y;

                float noiseA = fBm2(uv * _NoiseScaleA + t * _NoiseSpeedA);
                float noiseB = fBm2(uv * _NoiseScaleB + t * _NoiseSpeedB);

                // ── Colour Composition ───────────────────────────────────────
                float4 unexploredC = _UnexploredColor;
                unexploredC.a = saturate(unexploredC.a + noiseA * _NoiseStrengthA);

                float4 exploredC = _ExploredColor;
                exploredC.a = saturate(exploredC.a + noiseB * _NoiseStrengthB);

                float4 visibleC = float4(0, 0, 0, 0);

                // Weighted blend of the three zones
                float4 col = unexploredC * wUnexplored
                           + exploredC  * wExplored
                           + visibleC   * wVisible;

                return col;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/2D/Sprite-Unlit-Default"
}
