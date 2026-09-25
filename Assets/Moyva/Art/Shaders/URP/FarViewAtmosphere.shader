Shader "Hidden/Moyva/FarViewAtmosphere"
{
    // High-altitude atmosphere composite: aerial haze, map-like flattening,
    // procedural cloud veil, palette unification. Driven entirely by global
    // uniforms pushed by FarViewAtmosphereDriver (shared ICameraZoomState).
    // No textures, no raymarching — one fullscreen pass + copy back.

    Properties
    {
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "MoyvaFarViewComposite"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            ZWrite Off
            ZTest Always
            Cull Off
            Blend Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragComposite
            #pragma target 3.5
            #pragma multi_compile _ MOYVA_FARVIEW_QUALITY_LOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            // Fog of War globals (published by FogScreenSpaceTextureUpdater).
            // R = hidden (unexplored + explored-but-covered); G = unexplored.
            // Atmosphere must never restyle hidden pixels or pull them into
            // neighbouring ones through the flatten blur taps.
            TEXTURE2D(_MoyvaFogStateTexture);
            SAMPLER(sampler_MoyvaFogStateTexture);
            float4 _MoyvaFogMapSize;
            float4 _MoyvaFogGridOrigin;
            float4 _MoyvaFogWorldToGrid;
            float _MoyvaFogEnabled;
            float _MoyvaFogFlipY;

            // Globals pushed by FarViewAtmosphereDriver (no per-material data).
            float _MoyvaFarViewWeight;
            float _MoyvaFarViewDepthAvailable;
            float _MoyvaFarViewDebug;
            float4 _MoyvaFarViewHazeColor;
            float4 _MoyvaFarViewSkyColor;
            float4 _MoyvaFarViewHaze;      // x:strength y:depthStart z:depthEnd w:gamma
            float4 _MoyvaFarViewPalette;   // x:saturation y:contrast z:shadowLift w:highlightCompress
            float4 _MoyvaFarViewFlatten;   // x:strength y:radiusPixels z:w reserved
            float4 _MoyvaFarViewVeil;      // x:strength y:scale z:speed w:altitude
            float4 _MoyvaFarViewVeil2;     // x:coverage y:unused z:vignetteStrength w:vignetteRadius
            float4 _MoyvaFarViewWind;      // xy:windDir z:dither w:skyFill

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewRayWS : TEXCOORD1; // direction from camera to this pixel
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.uv = GetFullScreenTriangleTexCoord(input.vertexID);

                // Reconstruct a mid-depth world point to obtain the view ray.
                // Works for perspective and orthographic via inverse VP.
                float3 midWS = ComputeWorldSpacePosition(output.uv, 0.5, UNITY_MATRIX_I_VP);
                output.viewRayWS = midWS - _WorldSpaceCameraPos.xyz;
                return output;
            }

            static const float3 LumaW = float3(0.2126, 0.7152, 0.0722);

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = Hash21(i);
                float b = Hash21(i + float2(1, 0));
                float c = Hash21(i + float2(0, 1));
                float d = Hash21(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float3 ApplySaturation(float3 color, float saturation)
            {
                float luma = dot(color, LumaW);
                return lerp(luma.xxx, color, saturation);
            }

            // ---- fog-of-war visibility gate ----
            // Same world->grid transform as the fog composite so the mask is
            // evaluated at the real surface position, not a screen guess.

            float2 MoyvaWorldToFogGrid(float3 worldPosition)
            {
                float2 deltaXZ = worldPosition.xz - _MoyvaFogGridOrigin.xy;
                float2 gridPosition;
                gridPosition.x = dot(deltaXZ, _MoyvaFogWorldToGrid.xy);
                gridPosition.y = dot(deltaXZ, _MoyvaFogWorldToGrid.zw);
                if (_MoyvaFogFlipY > 0.5)
                    gridPosition.y = (_MoyvaFogMapSize.y - 1.0) - gridPosition.y;
                return gridPosition;
            }

            float2 MoyvaSampleFogMasks(float2 cell)
            {
                float2 mapSize = max(_MoyvaFogMapSize.xy, 1.0.xx);
                float2 inside =
                    step(0.0.xx, cell) * step(cell, mapSize - 1.0.xx);
                float insideMap = inside.x * inside.y;
                float2 uv = (clamp(cell, 0.0.xx, mapSize - 1.0.xx) + 0.5.xx) / mapSize;
                float2 sampled = SAMPLE_TEXTURE2D_LOD(
                    _MoyvaFogStateTexture,
                    sampler_MoyvaFogStateTexture,
                    uv,
                    0).rg;
                // Outside the logical map counts as hidden, matching the
                // fog composite's own convention.
                return lerp(1.0.xx, sampled, insideMap);
            }

            // Bilinear hidden weight (fog R channel) at the pixel's real
            // surface position. Sky and missing depth stay fully visible.
            float MoyvaHiddenWeight(float2 uv)
            {
                if (_MoyvaFogEnabled < 0.5 || _MoyvaFarViewDepthAvailable < 0.5)
                    return 0.0;

                float rawDepth = SampleSceneDepth(UnityStereoTransformScreenSpaceTex(uv));
                if (rawDepth <= 0.00001)
                    return 0.0;

                float3 worldPos = ComputeWorldSpacePosition(uv, rawDepth, UNITY_MATRIX_I_VP);
                float2 gridPos = MoyvaWorldToFogGrid(worldPos);
                float2 baseCell = floor(gridPos);
                float2 f = frac(gridPos);
                float u00 = MoyvaSampleFogMasks(baseCell).r;
                float u10 = MoyvaSampleFogMasks(baseCell + float2(1, 0)).r;
                float u01 = MoyvaSampleFogMasks(baseCell + float2(0, 1)).r;
                float u11 = MoyvaSampleFogMasks(baseCell + float2(1, 1)).r;
                return lerp(lerp(u00, u10, f.x), lerp(u01, u11, f.x), f.y);
            }

            float4 FragComposite(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float w = saturate(_MoyvaFarViewWeight);
                float4 scene = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv);
                int debugMode = (int)(_MoyvaFarViewDebug + 0.5);

                if (debugMode == 1) // weight visualization
                    return float4(w.xxx, 1);

                // Fog of War gate: hidden pixels (unexplored or explored-but-
                // covered) keep the fog composite's authored color — the
                // atmosphere must not lift them or leak terrain silhouettes
                // through. Partial edges fade smoothly.
                float fogVisibility = 1.0 - MoyvaHiddenWeight(uv);
                w *= fogVisibility;

                if (w <= 0.001 && debugMode == 0)
                    return scene;

                // ---- depth ----
                bool hasDepth = _MoyvaFarViewDepthAvailable > 0.5;
                float rawDepth = hasDepth
                    ? SampleSceneDepth(UnityStereoTransformScreenSpaceTex(uv))
                    : 0.0;
                float eyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                bool isSky = !hasDepth || rawDepth <= 0.00001;

                if (debugMode == 5) // eye-depth visualization
                    return float4(saturate(eyeDepth / 200.0).xxx, 1);

                float3 color = scene.rgb;

                // ---- map-like flattening: suppress local contrast ----
                // Local mean via cross taps; detail = color - mean is scaled
                // down so micro geometry reads as flat map strokes.
                float flattenK = _MoyvaFarViewFlatten.x * w;
                if (debugMode == 4 || (flattenK > 0.001 && debugMode == 0))
                {
                    float2 texel = 1.0 / _ScreenParams.xy;
                    float2 off = _MoyvaFarViewFlatten.y * texel;
                    // Each tap is weighted by its own fog visibility so the
                    // local mean never drags hidden darkness into visible
                    // pixels or vice versa across the fog boundary.
                    bool fogActive = _MoyvaFogEnabled > 0.5
                        && _MoyvaFarViewDepthAvailable > 0.5;
#if defined(MOYVA_FARVIEW_QUALITY_LOW)
                    // Performance tier: 2 taps + center instead of 4 taps.
                    float2 tapA = uv + float2(off.x, 0);
                    float2 tapB = uv - float2(off.x, 0);
                    float wA = fogActive ? 1.0 - MoyvaHiddenWeight(tapA) : 1.0;
                    float wB = fogActive ? 1.0 - MoyvaHiddenWeight(tapB) : 1.0;
                    float wC = fogActive ? fogVisibility : 1.0;
                    float wSum = wA + wB + wC;
                    float3 mean = wSum > 0.0001
                        ? (SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, tapA).rgb * wA
                         + SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, tapB).rgb * wB
                         + scene.rgb * wC) / wSum
                        : scene.rgb;
#else
                    float2 tapA = uv + float2(off.x, 0);
                    float2 tapB = uv - float2(off.x, 0);
                    float2 tapC = uv + float2(0, off.y);
                    float2 tapD = uv - float2(0, off.y);
                    float wA = fogActive ? 1.0 - MoyvaHiddenWeight(tapA) : 1.0;
                    float wB = fogActive ? 1.0 - MoyvaHiddenWeight(tapB) : 1.0;
                    float wC = fogActive ? 1.0 - MoyvaHiddenWeight(tapC) : 1.0;
                    float wD = fogActive ? 1.0 - MoyvaHiddenWeight(tapD) : 1.0;
                    float wSum = wA + wB + wC + wD;
                    float3 mean = wSum > 0.0001
                        ? (SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, tapA).rgb * wA
                         + SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, tapB).rgb * wB
                         + SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, tapC).rgb * wC
                         + SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, tapD).rgb * wD) / wSum
                        : scene.rgb;
#endif
                    float3 detail = color - mean;
                    float k = debugMode == 4 ? _MoyvaFarViewFlatten.x : flattenK;
                    color = mean + detail * (1.0 - saturate(k));
                    if (debugMode == 4)
                        return float4(color, 1);
                }

                // ---- palette unification ----
                float sat = lerp(1.0, _MoyvaFarViewPalette.x, w);
                float contrast = lerp(1.0, _MoyvaFarViewPalette.y, w);
                color = ApplySaturation(color, sat);
                float luma = dot(color, LumaW);
                color = (color - luma.xxx) * contrast + luma.xxx; // contrast around own luma

                // shadow lift toward atmosphere color + highlight compression
                float lift = _MoyvaFarViewPalette.z * w;
                float liftMask = saturate(1.0 - luma * 2.0); // darks only
                color = lerp(color, max(color, _MoyvaFarViewHazeColor.rgb * luma.xxx * 1.15), lift * liftMask);
                float hi = _MoyvaFarViewPalette.w * w;
                float hiMask = saturate((luma - 0.72) * 3.0);
                color = lerp(color, color * 0.86 + _MoyvaFarViewHazeColor.rgb * 0.14, hi * hiMask);

                // ---- aerial haze by view distance ----
                float depthT = 0.0;
                if (!isSky)
                {
                    float dStart = _MoyvaFarViewHaze.y;
                    float dEnd = max(_MoyvaFarViewHaze.z, dStart + 0.01);
                    depthT = saturate((eyeDepth - dStart) / (dEnd - dStart));
                    depthT = pow(depthT, max(_MoyvaFarViewHaze.w, 0.05));
                }
                float haze = depthT * _MoyvaFarViewHaze.x * w;
                if (isSky)
                    haze = max(haze, _MoyvaFarViewWind.w * w);

                if (debugMode == 2) // haze only
                    return float4(lerp(scene.rgb, isSky ? _MoyvaFarViewSkyColor.rgb : _MoyvaFarViewHazeColor.rgb, saturate(haze)), 1);

                float3 hazeCol = isSky ? _MoyvaFarViewSkyColor.rgb : _MoyvaFarViewHazeColor.rgb;
                color = lerp(color, hazeCol, saturate(haze));

                // ---- procedural cloud veil (world-anchored, parallax) ----
                float veilStrength = _MoyvaFarViewVeil.x * w;
                if (debugMode == 3 || (veilStrength > 0.001 && debugMode == 0))
                {
                    // Intersect view ray with a virtual cloud-altitude plane.
                    float3 rayDir = normalize(input.viewRayWS);
                    float planeY = _MoyvaFarViewVeil.w; // world units above y=0
                    float t = (planeY - _WorldSpaceCameraPos.y) / min(rayDir.y, -0.02);
                    t = max(t, 0.0);
                    float3 anchor = _WorldSpaceCameraPos.xyz + rayDir * t;

                    float2 vuv = anchor.xz * _MoyvaFarViewVeil.y
                               + _MoyvaFarViewWind.xy * (_Time.y * _MoyvaFarViewVeil.z);
                    float n = ValueNoise(vuv);
#if !defined(MOYVA_FARVIEW_QUALITY_LOW)
                    n = n * 0.68 + ValueNoise(vuv * 2.17 + 19.7) * 0.32;
#endif
                    float cov = _MoyvaFarViewVeil2.x;
                    float veil = smoothstep(cov - 0.30, cov + 0.30, n) * veilStrength;
                    // Veil is air between camera and ground: never stronger on sky
                    // than on the world below, and fades where the eye ray is level.
                    veil *= saturate(-rayDir.y * 4.0 + 0.25);

                    if (debugMode == 3)
                        return float4(veil.xxx, 1);
                    color = lerp(color, _MoyvaFarViewSkyColor.rgb, saturate(veil));
                }

                // ---- soft edge atmosphere ----
                float vigStr = _MoyvaFarViewVeil2.z * w;
                if (vigStr > 0.001)
                {
                    float2 centered = uv - 0.5;
                    float edge = length(centered) * 2.0; // 0 center → ~1.41 corner
                    float vig = smoothstep(_MoyvaFarViewVeil2.w - 0.45,
                                           _MoyvaFarViewVeil2.w + 0.45, edge);
                    color = lerp(color, _MoyvaFarViewSkyColor.rgb, vig * vigStr);
                }

                // ---- dither against gradient banding ----
                float dither = _MoyvaFarViewWind.z;
                if (dither > 0.0001)
                {
                    float n2 = Hash21(uv * _ScreenParams.xy + frac(_Time.y) * 61.7);
                    color += (n2 - 0.5) * dither;
                }

                return float4(color, scene.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "MoyvaFarViewCopyBack"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            ZWrite Off
            ZTest Always
            Cull Off
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragCopy

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
                return output;
            }

            float4 FragCopy(Varyings input) : SV_Target
            {
                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, input.uv);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
