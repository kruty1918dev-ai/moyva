Shader "Moyva/2D/SelectionSprite"
{
    Properties
    {
        _MainTex ("Diffuse", 2D) = "white" {}
        _MaskTex ("Mask", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        [MaterialToggle] _ZWrite ("ZWrite", Float) = 0

        _OutlineColor ("Outline Color A", Color) = (0.2, 0.95, 0.3, 1.0)
        _OutlineColorSecondary ("Outline Color B", Color) = (1.0, 1.0, 1.0, 1.0)
        _OutlineSize ("Outline Thickness (px)", Float) = 2.0
        _AnimationSpeed ("Animation Speed", Float) = 2.5
        _AnimationMin ("Animation Min", Range(0, 1)) = 0.0
        _AnimationMax ("Animation Max", Range(0, 1)) = 1.0
        _DashLen ("Dash Length", Float) = 0.2
        _GapLen ("Gap Length", Float) = 0.12

        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _AlphaTex ("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        [HideInInspector] _OutlineWidthOS ("Outline Width OS", Float) = 0.02
        [HideInInspector] _SpriteLocalMin ("Sprite Local Min", Vector) = (-0.5, -0.5, 0, 0)
        [HideInInspector] _SpriteLocalMax ("Sprite Local Max", Vector) = (0.5, 0.5, 0, 0)
        [HideInInspector] _SpriteUvMinMax ("Sprite UV MinMax", Vector) = (0, 0, 1, 1)
        [HideInInspector] _MainTexTexelSize ("Main Texel Size", Vector) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite [_ZWrite]

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex LitVertex
            #pragma fragment LitFragment

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"

            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY
            #pragma multi_compile _ SKINNED_SPRITE

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_LIT_OUTPUTS
                half4 color : COLOR;
                float2 localPos : TEXCOORD4;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Lit2DCommon.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _OutlineColor;
                half4 _OutlineColorSecondary;
                float _OutlineSize;
                float _AnimationSpeed;
                float _AnimationMin;
                float _AnimationMax;
                float _DashLen;
                float _GapLen;
                float _OutlineWidthOS;
                float4 _SpriteLocalMin;
                float4 _SpriteLocalMax;
                float4 _SpriteUvMinMax;
                float4 _MainTexTexelSize;
            CBUFFER_END

            float2 ExpandLocalPosition(float2 localPos)
            {
                float2 center = (_SpriteLocalMin.xy + _SpriteLocalMax.xy) * 0.5;
                float2 direction = float2(localPos.x >= center.x ? 1.0 : -1.0, localPos.y >= center.y ? 1.0 : -1.0);
                return localPos + direction * _OutlineWidthOS;
            }

            float2 GetSelectionUv(float2 localPos)
            {
                float2 spriteSize = max(_SpriteLocalMax.xy - _SpriteLocalMin.xy, float2(0.0001, 0.0001));
                float2 rectUv = saturate((localPos - _SpriteLocalMin.xy) / spriteSize);
                return lerp(_SpriteUvMinMax.xy, _SpriteUvMinMax.zw, rectUv);
            }

            bool IsOutsideSpriteRect(float2 localPos)
            {
                return any(localPos < _SpriteLocalMin.xy) || any(localPos > _SpriteLocalMax.xy);
            }

            half ComputeDashMask(float2 localPos)
            {
                float2 mn = _SpriteLocalMin.xy;
                float2 mx = _SpriteLocalMax.xy;
                float w = max(mx.x - mn.x, 0.0001);
                float h = max(mx.y - mn.y, 0.0001);
                float2 center = (mn + mx) * 0.5;
                float2 halfSize = float2(w, h) * 0.5;
                float2 rel = localPos - center;

                float anx = abs(rel.x) / max(halfSize.x, 0.0001);
                float anyN = abs(rel.y) / max(halfSize.y, 0.0001);

                float cx = clamp(localPos.x, mn.x, mx.x);
                float cy = clamp(localPos.y, mn.y, mx.y);

                float p;
                if (anyN >= anx)
                {
                    p = (rel.y < 0.0)
                        ? (cx - mn.x)
                        : (w + h + (mx.x - cx));
                }
                else
                {
                    p = (rel.x > 0.0)
                        ? (w + (cy - mn.y))
                        : (2.0 * w + h + (mx.y - cy));
                }

                float period = max(_DashLen + _GapLen, 0.0001);
                float t = fmod(p + _Time.y * _AnimationSpeed + period * 1024.0, period);
                return (half)step(t, _DashLen);
            }

            float2 ClampToSpriteUv(float2 uv)
            {
                return clamp(uv, _SpriteUvMinMax.xy, _SpriteUvMinMax.zw);
            }

            half SampleSpriteAlpha(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, ClampToSpriteUv(uv)).a;
            }

            half ComputeOutlineAlpha(float2 selectionUv)
            {
                float2 texel = _MainTexTexelSize.xy;
                int r = clamp((int)_OutlineSize, 1, 6);
                half maxAlpha = 0.0h;

                for (int dy = -r; dy <= r; dy++)
                {
                    for (int dx = -r; dx <= r; dx++)
                    {
                        float2 sampleUv = selectionUv + texel * float2((float)dx, (float)dy);
                        maxAlpha = max(maxAlpha, SampleSpriteAlpha(sampleUv));
                        if (maxAlpha > 0.99h) return 1.0h;
                    }
                }

                return maxAlpha;
            }

            Varyings LitVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);
                input.positionOS.xy = ExpandLocalPosition(input.positionOS.xy);

                Varyings o = CommonLitVertex(input);
                o.color = input.color * _Color * unity_SpriteColor;
                o.localPos = input.positionOS.xy;
                return o;
            }

            half4 LitFragment(Varyings input) : SV_Target
            {
                bool outside = IsOutsideSpriteRect(input.localPos);
                float2 selectionUv = GetSelectionUv(input.localPos);
                half4 sampledMain = input.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, selectionUv);
                half4 sampledMask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, selectionUv);
                half3 sampledNormal = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, selectionUv));

                half4 composed;
                if (outside)
                {
                    half outlineAlpha = ComputeOutlineAlpha(selectionUv);
                    half dashMask = ComputeDashMask(input.localPos);
                    composed = half4((half3)1.0, outlineAlpha * dashMask);
                }
                else
                {
                    composed = sampledMain;
                }

                SurfaceData2D surfaceData;
                InputData2D inputData;

                InitializeSurfaceData(composed.rgb, composed.a, sampledMask, sampledNormal, surfaceData);
                InitializeInputData(selectionUv, input.lightingUV, inputData);

#if defined(DEBUG_DISPLAY)
                SETUP_DEBUG_TEXTURE_DATA_2D_NO_TS(inputData, input.positionWS, input.positionCS, _MainTex);
                surfaceData.normalWS = input.normalWS;
#endif

                return CombinedShapeLightShared(surfaceData, inputData);
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "NormalsRendering" }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex NormalsRenderingVertex
            #pragma fragment NormalsRenderingFragment

            #pragma multi_compile_instancing
            #pragma multi_compile _ SKINNED_SPRITE

            struct Attributes
            {
                COMMON_2D_NORMALS_INPUTS
                float4 color : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_NORMALS_OUTPUTS
                half4 color : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Normals2DCommon.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _OutlineColor;
                half4 _OutlineColorSecondary;
                float _OutlineSize;
                float _AnimationSpeed;
                float _AnimationMin;
                float _AnimationMax;
                float _DashLen;
                float _GapLen;
                float _OutlineWidthOS;
                float4 _SpriteLocalMin;
                float4 _SpriteLocalMax;
                float4 _SpriteUvMinMax;
                float4 _MainTexTexelSize;
            CBUFFER_END

            float2 ExpandLocalPosition(float2 localPos)
            {
                float2 center = (_SpriteLocalMin.xy + _SpriteLocalMax.xy) * 0.5;
                float2 direction = float2(localPos.x >= center.x ? 1.0 : -1.0, localPos.y >= center.y ? 1.0 : -1.0);
                return localPos + direction * _OutlineWidthOS;
            }

            Varyings NormalsRenderingVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);
                input.positionOS.xy = ExpandLocalPosition(input.positionOS.xy);

                Varyings o = CommonNormalsVertex(input);
                o.color = input.color * _Color * unity_SpriteColor;
                return o;
            }

            half4 NormalsRenderingFragment(Varyings input) : SV_Target
            {
                return CommonNormalsFragment(input, input.color);
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" "Queue" = "Transparent" "RenderType" = "Transparent" }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment

            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_OUTPUTS
                half4 color : COLOR;
                float2 localPos : TEXCOORD4;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/2DCommon.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _OutlineColor;
                half4 _OutlineColorSecondary;
                float _OutlineSize;
                float _AnimationSpeed;
                float _AnimationMin;
                float _AnimationMax;
                float _DashLen;
                float _GapLen;
                float _OutlineWidthOS;
                float4 _SpriteLocalMin;
                float4 _SpriteLocalMax;
                float4 _SpriteUvMinMax;
                float4 _MainTexTexelSize;
            CBUFFER_END

            float2 ExpandLocalPosition(float2 localPos)
            {
                float2 center = (_SpriteLocalMin.xy + _SpriteLocalMax.xy) * 0.5;
                float2 direction = float2(localPos.x >= center.x ? 1.0 : -1.0, localPos.y >= center.y ? 1.0 : -1.0);
                return localPos + direction * _OutlineWidthOS;
            }

            float2 GetSelectionUv(float2 localPos)
            {
                float2 spriteSize = max(_SpriteLocalMax.xy - _SpriteLocalMin.xy, float2(0.0001, 0.0001));
                float2 rectUv = saturate((localPos - _SpriteLocalMin.xy) / spriteSize);
                return lerp(_SpriteUvMinMax.xy, _SpriteUvMinMax.zw, rectUv);
            }

            bool IsOutsideSpriteRect(float2 localPos)
            {
                return any(localPos < _SpriteLocalMin.xy) || any(localPos > _SpriteLocalMax.xy);
            }

            half ComputeDashMask(float2 localPos)
            {
                float2 mn = _SpriteLocalMin.xy;
                float2 mx = _SpriteLocalMax.xy;
                float w = max(mx.x - mn.x, 0.0001);
                float h = max(mx.y - mn.y, 0.0001);
                float2 center = (mn + mx) * 0.5;
                float2 halfSize = float2(w, h) * 0.5;
                float2 rel = localPos - center;

                float anx = abs(rel.x) / max(halfSize.x, 0.0001);
                float anyN = abs(rel.y) / max(halfSize.y, 0.0001);

                float cx = clamp(localPos.x, mn.x, mx.x);
                float cy = clamp(localPos.y, mn.y, mx.y);

                float p;
                if (anyN >= anx)
                {
                    p = (rel.y < 0.0)
                        ? (cx - mn.x)
                        : (w + h + (mx.x - cx));
                }
                else
                {
                    p = (rel.x > 0.0)
                        ? (w + (cy - mn.y))
                        : (2.0 * w + h + (mx.y - cy));
                }

                float period = max(_DashLen + _GapLen, 0.0001);
                float t = fmod(p + _Time.y * _AnimationSpeed + period * 1024.0, period);
                return (half)step(t, _DashLen);
            }

            float2 ClampToSpriteUv(float2 uv)
            {
                return clamp(uv, _SpriteUvMinMax.xy, _SpriteUvMinMax.zw);
            }

            half SampleSpriteAlpha(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, ClampToSpriteUv(uv)).a;
            }

            half ComputeOutlineAlpha(float2 selectionUv)
            {
                float2 texel = _MainTexTexelSize.xy;
                int r = clamp((int)_OutlineSize, 1, 6);
                half maxAlpha = 0.0h;

                for (int dy = -r; dy <= r; dy++)
                {
                    for (int dx = -r; dx <= r; dx++)
                    {
                        float2 sampleUv = selectionUv + texel * float2((float)dx, (float)dy);
                        maxAlpha = max(maxAlpha, SampleSpriteAlpha(sampleUv));
                        if (maxAlpha > 0.99h) return 1.0h;
                    }
                }

                return maxAlpha;
            }

            Varyings UnlitVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);
                input.positionOS.xy = ExpandLocalPosition(input.positionOS.xy);

                Varyings o = CommonUnlitVertex(input);
                o.color = input.color * _Color * unity_SpriteColor;
                o.localPos = input.positionOS.xy;
                return o;
            }

            half4 UnlitFragment(Varyings input) : SV_Target
            {
                bool outside = IsOutsideSpriteRect(input.localPos);
                float2 selectionUv = GetSelectionUv(input.localPos);
                half4 sampledMain = input.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, selectionUv);
                half outlineAlpha = ComputeOutlineAlpha(selectionUv);

                if (!outside)
                    return sampledMain;

                half dashMask = ComputeDashMask(input.localPos);
                return half4((half3)1.0, outlineAlpha * dashMask);
            }
            ENDHLSL
        }
    }
}