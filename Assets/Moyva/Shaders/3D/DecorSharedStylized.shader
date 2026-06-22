Shader "Moyva/3D/Decor Shared Stylized"
{
    Properties
    {
        [Header(Surface)]
        _BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Muted Base Color", Color) = (0.86, 0.86, 0.80, 1)
        _Alpha("Alpha", Range(0, 1)) = 1
        _AlphaClipEnabled("Alpha Clip Strength", Range(0, 1)) = 1
        _AlphaClipThreshold("Alpha Clip Threshold", Range(0, 1)) = 0.35
        [Toggle] _BillboardEnabled("Face Camera Billboard", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull Mode", Float) = 2
        [HideInInspector] _SrcBlend("__src", Float) = 5
        [HideInInspector] _DstBlend("__dst", Float) = 10
        [HideInInspector] _ZWrite("__zw", Float) = 1

        [Header(Texture Fit)]
        _TextureFill("Texture Fill XY", Vector) = (1, 1, 0, 0)
        _TextureFillOffset("Texture Fill Offset XY", Vector) = (0, 0, 0, 0)
        _TextureFitClamp("Clamp Outside Texture Fill", Range(0, 1)) = 1

        [Header(Texture Volume)]
        _TextureVolumeStrength("Texture Volume Strength", Range(0, 1)) = 0.25
        _TextureVolumeRoundness("Texture Volume Roundness", Range(0, 1)) = 0.55
        _TextureVolumeLightColor("Texture Volume Light Color", Color) = (1, 0.96, 0.82, 1)
        _TextureVolumeShadowColor("Texture Volume Shadow Color", Color) = (0.52, 0.56, 0.42, 1)
        _TextureVolumeDirection("Texture Volume Direction UV", Vector) = (-0.45, 0.75, 0, 0)

        [Header(Lighting)]
        _AmbientStrength("Ambient Strength", Range(0, 2)) = 0.58
        _LightStrength("Main Light Strength", Range(0, 2)) = 0.85
        _MinimumBrightness("Minimum Brightness", Range(0, 1)) = 0.68
        _ShadowTint("Self Shadow Tint", Color) = (0.52, 0.53, 0.48, 1)
        _ShadowSoftness("Self Shadow Softness", Range(0.001, 0.5)) = 0.08

        [Header(Stylization)]
        _TextureSaturation("Texture Saturation", Range(0, 2)) = 0.9
        _TextureContrast("Texture Contrast", Range(0, 2)) = 0.96
        _ColorPosterizeSteps("Color Posterize Steps", Range(2, 12)) = 5
        _PosterizeStrength("Posterize Strength", Range(0, 1)) = 0.18
        _LightStepCount("Light Step Count", Range(2, 6)) = 3
        _LightStepStrength("Light Step Strength", Range(0, 1)) = 0.75
        _RimEnabled("Rim Light Strength", Range(0, 1)) = 0
        _StylizedRimColor("Rim Light Color", Color) = (0.75, 0.9, 1, 1)
        _RimPower("Rim Light Power", Range(0.5, 8)) = 3
        _LeafPlaneShading("Leaf Plane Shading Strength", Range(0, 1)) = 0
        _LeafPlaneDirection("Leaf Plane Direction UV", Vector) = (1, 0, 0, 0)
        _LeafShadeStrength("Leaf Shade Strength", Range(0, 0.5)) = 0.12
        _LeafLightStrength("Leaf Light Strength", Range(0, 0.5)) = 0.06
        _LeafPlaneSoftness("Leaf Plane Softness", Range(0.001, 1)) = 0.25
        _LeafPlaneBalance("Leaf Plane Balance", Range(-1, 1)) = 0

        [Header(Outline)]
        _OutlineEnabled("Outline Strength", Range(0, 1)) = 1
        _OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
        [HideInInspector] _OutlineWidth("Outline Width World Units", Range(0, 0.12)) = 0
        _OutlineScreenWidthPx("Outline Screen Width Pixels", Range(0, 12)) = 1.5
        [HideInInspector] _AlphaOutlineWidth("Alpha Texture Outline Width Texels", Range(0, 6)) = 0
        [HideInInspector] _AlphaOutlineScreenWidthPx("Alpha Texture Outline Extra Pixels", Range(0, 4)) = 0
        [HideInInspector] [Enum(UnityEngine.Rendering.CompareFunction)] _OutlineZTest("Outline ZTest", Float) = 4
        [HideInInspector] _EntityStencilRef("Decor Stencil Ref", Range(1, 255)) = 61

        [Header(Contact Shadow)]
        _ContactShadowEnabled("Contact Shadow Strength", Range(0, 1)) = 1
        [Enum(Mesh Footprint,0,UV Card Blob,1)] _ContactBlobMode("Contact Shadow Mode", Float) = 0
        _ContactBlobAspect("Contact Blob Aspect XZ", Vector) = (1.25, 0.55, 0, 0)
        _ContactCameraBackOffset("Contact Camera Back Offset", Range(-2, 2)) = 0
        _ContactColor("Contact Color", Color) = (0.04, 0.035, 0.03, 1)
        _ContactDarkness("Contact Darkness", Range(0, 1)) = 0.14
        _ContactRadius("Contact Blob Radius", Range(0.01, 5)) = 0.55
        _ContactSoftness("Contact Edge Softness", Range(0.01, 1)) = 0.68
        _ContactProjectionScale("Mesh Contact Projection Scale", Range(0.01, 4)) = 1.2
        _ContactLocalY("Contact Local Y", Float) = 0
        [HideInInspector] _ContactLift("Contact Lift", Range(0, 0.1)) = 0.018
        _ContactOffsetOS("Contact Offset Object XZ", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest+40"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull [_CullMode]
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            ZTest LEqual

            Stencil
            {
                Ref [_EntityStencilRef]
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex LitVertex
            #pragma fragment LitFragment
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            float4 _BaseMap_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Alpha;
                half _AlphaClipEnabled;
                half _AlphaClipThreshold;
                half _BillboardEnabled;
                half4 _TextureFill;
                half4 _TextureFillOffset;
                half _TextureFitClamp;
                half _TextureVolumeStrength;
                half _TextureVolumeRoundness;
                half4 _TextureVolumeLightColor;
                half4 _TextureVolumeShadowColor;
                half4 _TextureVolumeDirection;
                half _TextureSaturation;
                half _TextureContrast;
                half _ColorPosterizeSteps;
                half _PosterizeStrength;
                half _LightStepCount;
                half _LightStepStrength;
                half _RimEnabled;
                half4 _StylizedRimColor;
                half _RimPower;
                half _LeafPlaneShading;
                half4 _LeafPlaneDirection;
                half _LeafShadeStrength;
                half _LeafLightStrength;
                half _LeafPlaneSoftness;
                half _LeafPlaneBalance;
                half4 _ShadowTint;
                half _AmbientStrength;
                half _LightStrength;
                half _MinimumBrightness;
                half _ShadowSoftness;
                half _OutlineEnabled;
                half4 _OutlineColor;
                half _OutlineWidth;
                half _OutlineScreenWidthPx;
                half _AlphaOutlineWidth;
                half _AlphaOutlineScreenWidthPx;
                half4 _ContactColor;
                half _ContactShadowEnabled;
                half _ContactBlobMode;
                half4 _ContactBlobAspect;
                half _ContactCameraBackOffset;
                half _ContactDarkness;
                half _ContactRadius;
                half _ContactSoftness;
                half _ContactProjectionScale;
                half _ContactLocalY;
                half _ContactLift;
                half4 _ContactOffsetOS;
            CBUFFER_END

            float2 ApplyTextureFitUV(float2 uv)
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

            half4 SampleBaseMapFitted(float2 uv)
            {
                half mask = TextureBoundsMask(uv);
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                tex *= mask;
                return tex;
            }

            float3 GetBillboardRightWS()
            {
                float3 rightWS = float3(UNITY_MATRIX_I_V._m00, 0.0, UNITY_MATRIX_I_V._m20);
                float rightLenSq = dot(rightWS, rightWS);
                return rightLenSq > 0.0001 ? normalize(rightWS) : float3(1.0, 0.0, 0.0);
            }

            float3 GetBillboardForwardWS(float3 rightWS)
            {
                return normalize(cross(float3(0.0, 1.0, 0.0), rightWS));
            }

            float3 TransformBillboardPositionWS(float3 positionOS)
            {
                float3 pivotWS = TransformObjectToWorld(float3(0.0, 0.0, 0.0));
                float rightScale = length(TransformObjectToWorld(float3(1.0, 0.0, 0.0)) - pivotWS);
                float upScale = length(TransformObjectToWorld(float3(0.0, 1.0, 0.0)) - pivotWS);
                float3 rightWS = GetBillboardRightWS();
                return pivotWS + rightWS * positionOS.x * rightScale + float3(0.0, 1.0, 0.0) * positionOS.y * upScale;
            }

            half3 ApplyStylizedTextureColor(half3 color)
            {
                half luminance = dot(color, half3(0.299, 0.587, 0.114));
                color = lerp(half3(luminance, luminance, luminance), color, _TextureSaturation);
                color = (color - 0.5) * _TextureContrast + 0.5;
                color = saturate(color);

                half posterSteps = max(2.0, _ColorPosterizeSteps);
                half3 posterized = floor(color * posterSteps + 0.5) / posterSteps;
                half posterizeMask = saturate(_PosterizeStrength) * step(1.5, _ColorPosterizeSteps);
                return lerp(color, posterized, posterizeMask);
            }

            half3 ApplyLeafPlaneShading(half3 color, float2 uv)
            {
                half enabled = saturate(_LeafPlaneShading);
                float2 direction = _LeafPlaneDirection.xy;
                float directionLenSq = dot(direction, direction);
                direction = directionLenSq > 0.0001 ? normalize(direction) : float2(1.0, 0.0);

                half plane = dot(uv * 2.0 - 1.0, direction) + _LeafPlaneBalance;
                half softness = max(0.001, _LeafPlaneSoftness);
                half shadeMask = 1.0 - smoothstep(-softness, 0.0, plane);
                half lightMask = smoothstep(0.0, softness, plane);
                half multiplier = 1.0
                    - shadeMask * saturate(_LeafShadeStrength)
                    + lightMask * saturate(_LeafLightStrength);

                return lerp(color, saturate(color * multiplier), enabled);
            }

            half3 ApplyTextureVolume(half3 color, float2 uv, half alpha)
            {
                half strength = saturate(_TextureVolumeStrength) * saturate(alpha * 2.0);
                float2 direction = _TextureVolumeDirection.xy;
                float directionLenSq = dot(direction, direction);
                direction = directionLenSq > 0.0001 ? normalize(direction) : float2(-0.45, 0.75);

                float2 centered = uv * 2.0 - 1.0;
                half roundness = lerp(0.08, 0.85, saturate(_TextureVolumeRoundness));
                half side = dot(centered, direction);
                half shadeMask = 1.0 - smoothstep(-roundness, roundness, side);
                half lightMask = smoothstep(-roundness * 0.5, roundness, side);
                half edgeMask = smoothstep(0.35, 1.25, length(centered)) * saturate(_TextureVolumeRoundness);

                half shadowAmount = saturate((shadeMask * 0.65 + edgeMask * 0.45) * _TextureVolumeShadowColor.a);
                half3 shadowed = lerp(color, color * _TextureVolumeShadowColor.rgb, shadowAmount);
                half3 highlighted = saturate(shadowed + _TextureVolumeLightColor.rgb * _TextureVolumeLightColor.a * lightMask * 0.22);
                return lerp(color, highlighted, strength);
            }

            half ApplyStylizedLightSteps(half value)
            {
                half denom = max(1.0, _LightStepCount - 1.0);
                half stepped = floor(saturate(value) * denom + 0.5) / denom;
                return saturate(lerp(value, stepped, saturate(_LightStepStrength)));
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings LitVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                half billboardEnabled = saturate(_BillboardEnabled);
                float3 billboardRightWS = GetBillboardRightWS();
                float3 billboardPositionWS = TransformBillboardPositionWS(input.positionOS.xyz);
                float3 positionWS = lerp(positionInputs.positionWS, billboardPositionWS, billboardEnabled);
                float3 normalWS = lerp(normalize(normalInputs.normalWS), GetBillboardForwardWS(billboardRightWS), billboardEnabled);

                output.positionCS = TransformWorldToHClip(positionWS);
                output.positionWS = positionWS;
                output.normalWS = normalize(normalWS);
                output.uv = ApplyTextureFitUV(TRANSFORM_TEX(input.uv, _BaseMap));
                output.shadowCoord = TransformWorldToShadowCoord(positionWS);
                return output;
            }

            half4 LitFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 tex = SampleBaseMapFitted(input.uv);
                half4 albedo = tex * _BaseColor;
                albedo.a *= _Alpha;
                albedo.rgb = ApplyStylizedTextureColor(albedo.rgb);
                albedo.rgb = ApplyLeafPlaneShading(albedo.rgb, input.uv);
                albedo.rgb = ApplyTextureVolume(albedo.rgb, input.uv, albedo.a);

                half clipThreshold = lerp(-0.001, _AlphaClipThreshold, saturate(_AlphaClipEnabled));
                half coreMask = step(_AlphaClipThreshold, albedo.a);

                half outlineStrength = saturate(_OutlineEnabled);
                half alphaOutlineWidth = _AlphaOutlineWidth * outlineStrength;
                half alphaOutlineScreenWidthPx = max(_AlphaOutlineScreenWidthPx, _OutlineScreenWidthPx) * outlineStrength;
                float2 alphaOutlineTexelStep = _BaseMap_TexelSize.xy * alphaOutlineWidth;
                float2 alphaOutlineScreenDx = ddx(input.uv) * alphaOutlineScreenWidthPx;
                float2 alphaOutlineScreenDy = ddy(input.uv) * alphaOutlineScreenWidthPx;
                half neighborAlpha = 0.0;
                half texelNeighborAlpha = 0.0;
                texelNeighborAlpha = max(texelNeighborAlpha, SampleBaseMapFitted(input.uv + float2(alphaOutlineTexelStep.x, 0.0)).a);
                texelNeighborAlpha = max(texelNeighborAlpha, SampleBaseMapFitted(input.uv - float2(alphaOutlineTexelStep.x, 0.0)).a);
                texelNeighborAlpha = max(texelNeighborAlpha, SampleBaseMapFitted(input.uv + float2(0.0, alphaOutlineTexelStep.y)).a);
                texelNeighborAlpha = max(texelNeighborAlpha, SampleBaseMapFitted(input.uv - float2(0.0, alphaOutlineTexelStep.y)).a);
                texelNeighborAlpha = max(texelNeighborAlpha, SampleBaseMapFitted(input.uv + alphaOutlineTexelStep).a);
                texelNeighborAlpha = max(texelNeighborAlpha, SampleBaseMapFitted(input.uv - alphaOutlineTexelStep).a);
                texelNeighborAlpha = max(texelNeighborAlpha, SampleBaseMapFitted(input.uv + float2(alphaOutlineTexelStep.x, -alphaOutlineTexelStep.y)).a);
                texelNeighborAlpha = max(texelNeighborAlpha, SampleBaseMapFitted(input.uv + float2(-alphaOutlineTexelStep.x, alphaOutlineTexelStep.y)).a);

                half screenNeighborAlpha = 0.0;
                screenNeighborAlpha = max(screenNeighborAlpha, SampleBaseMapFitted(input.uv + alphaOutlineScreenDx).a);
                screenNeighborAlpha = max(screenNeighborAlpha, SampleBaseMapFitted(input.uv - alphaOutlineScreenDx).a);
                screenNeighborAlpha = max(screenNeighborAlpha, SampleBaseMapFitted(input.uv + alphaOutlineScreenDy).a);
                screenNeighborAlpha = max(screenNeighborAlpha, SampleBaseMapFitted(input.uv - alphaOutlineScreenDy).a);
                screenNeighborAlpha = max(screenNeighborAlpha, SampleBaseMapFitted(input.uv + alphaOutlineScreenDx + alphaOutlineScreenDy).a);
                screenNeighborAlpha = max(screenNeighborAlpha, SampleBaseMapFitted(input.uv - alphaOutlineScreenDx - alphaOutlineScreenDy).a);
                screenNeighborAlpha = max(screenNeighborAlpha, SampleBaseMapFitted(input.uv + alphaOutlineScreenDx - alphaOutlineScreenDy).a);
                screenNeighborAlpha = max(screenNeighborAlpha, SampleBaseMapFitted(input.uv - alphaOutlineScreenDx + alphaOutlineScreenDy).a);

                neighborAlpha = max(
                    texelNeighborAlpha * step(0.001, alphaOutlineWidth),
                    screenNeighborAlpha * step(0.001, alphaOutlineScreenWidthPx));
                neighborAlpha *= _BaseColor.a * _Alpha;

                half alphaNeighborThreshold = max(0.001, _AlphaClipThreshold);
                half alphaNeighborMask = smoothstep(alphaNeighborThreshold * 0.35, alphaNeighborThreshold, neighborAlpha);
                half alphaOutlineMask = (1.0 - coreMask)
                    * alphaNeighborMask
                    * saturate(step(0.001, alphaOutlineWidth) + step(0.001, alphaOutlineScreenWidthPx))
                    * saturate(_AlphaClipEnabled)
                    * step(0.001, outlineStrength);

                if (alphaOutlineMask > 0.0)
                {
                    half alphaOutlineIntensity = saturate(alphaOutlineMask * outlineStrength * _OutlineColor.a);
                    half3 alphaOutlineColor = lerp(_BaseColor.rgb, _OutlineColor.rgb, alphaOutlineIntensity);
                    return half4(alphaOutlineColor, max(albedo.a, alphaOutlineIntensity));
                }

                clip(albedo.a - clipThreshold);

                float normalLenSq = dot(input.normalWS, input.normalWS);
                half3 normalWS = normalLenSq > 0.0001 ? normalize(input.normalWS) : half3(0.0, 1.0, 0.0);
                Light mainLight = GetMainLight(input.shadowCoord);
                half ndotl = saturate(dot(normalWS, mainLight.direction));
                half lightRamp = smoothstep(0.5 - _ShadowSoftness, 0.5 + _ShadowSoftness, ndotl);
                half shadowedRamp = lightRamp * mainLight.shadowAttenuation;
                shadowedRamp = ApplyStylizedLightSteps(shadowedRamp);

                half3 minimumLight = half3(_MinimumBrightness, _MinimumBrightness, _MinimumBrightness);
                half3 ambient = max(SampleSH(normalWS), half3(0.0, 0.0, 0.0)) * _AmbientStrength;
                half3 shadowTint = lerp(half3(1.0, 1.0, 1.0), _ShadowTint.rgb, saturate(_ShadowTint.a));
                half3 directTint = lerp(shadowTint, half3(1.0, 1.0, 1.0), shadowedRamp);
                half directBrightness = lerp(0.72, 1.0, shadowedRamp);
                half3 direct = mainLight.color * directTint * directBrightness * _LightStrength;
                half3 color = albedo.rgb * saturate(minimumLight + (1.0 - minimumLight) * (ambient + direct));
                half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half rim = pow(saturate(1.0 - dot(normalWS, viewDirWS)), _RimPower) * saturate(_RimEnabled);
                color = saturate(color + _StylizedRimColor.rgb * _StylizedRimColor.a * rim);
                return half4(color, albedo.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ContactShadow"
            Tags { "LightMode" = "DecorContactShadow" }

            Cull Off
            ZWrite Off
            ZTest LEqual
            Blend DstColor Zero

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex ContactVertex
            #pragma fragment ContactFragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Alpha;
                half _AlphaClipEnabled;
                half _AlphaClipThreshold;
                half _BillboardEnabled;
                half4 _TextureFill;
                half4 _TextureFillOffset;
                half _TextureFitClamp;
                half _TextureVolumeStrength;
                half _TextureVolumeRoundness;
                half4 _TextureVolumeLightColor;
                half4 _TextureVolumeShadowColor;
                half4 _TextureVolumeDirection;
                half _TextureSaturation;
                half _TextureContrast;
                half _ColorPosterizeSteps;
                half _PosterizeStrength;
                half _LightStepCount;
                half _LightStepStrength;
                half _RimEnabled;
                half4 _StylizedRimColor;
                half _RimPower;
                half _LeafPlaneShading;
                half4 _LeafPlaneDirection;
                half _LeafShadeStrength;
                half _LeafLightStrength;
                half _LeafPlaneSoftness;
                half _LeafPlaneBalance;
                half4 _ShadowTint;
                half _AmbientStrength;
                half _LightStrength;
                half _MinimumBrightness;
                half _ShadowSoftness;
                half _OutlineEnabled;
                half4 _OutlineColor;
                half _OutlineWidth;
                half _OutlineScreenWidthPx;
                half _AlphaOutlineWidth;
                half _AlphaOutlineScreenWidthPx;
                half4 _ContactColor;
                half _ContactShadowEnabled;
                half _ContactBlobMode;
                half4 _ContactBlobAspect;
                half _ContactCameraBackOffset;
                half _ContactDarkness;
                half _ContactRadius;
                half _ContactSoftness;
                half _ContactProjectionScale;
                half _ContactLocalY;
                half _ContactLift;
                half4 _ContactOffsetOS;
            CBUFFER_END

            float2 ApplyTextureFitUV(float2 uv)
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

            half4 SampleBaseMapFitted(float2 uv)
            {
                half mask = TextureBoundsMask(uv);
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                tex *= mask;
                return tex;
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 contactUV : TEXCOORD0;
                float2 uv : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings ContactVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 contactOS = input.positionOS.xyz;
                float2 offsetOS = _ContactOffsetOS.xy;
                half blobMode = saturate(max(_ContactBlobMode, _BillboardEnabled));
                float2 projectedXZ = (contactOS.xz - offsetOS) * _ContactProjectionScale + offsetOS;
                float2 blobUV = input.uv * 2.0 - 1.0;
                float2 blobAspect = max(_ContactBlobAspect.xy, float2(0.01, 0.01));

                contactOS.xz = projectedXZ;
                contactOS.y = _ContactLocalY;

                float3 projectedWS = TransformObjectToWorld(contactOS);
                float3 centerWS = TransformObjectToWorld(float3(offsetOS.x, _ContactLocalY, offsetOS.y));
                float3 cameraBackWS = centerWS - GetCameraPositionWS();
                cameraBackWS.y = 0.0;
                float cameraBackLenSq = dot(cameraBackWS, cameraBackWS);
                cameraBackWS = cameraBackLenSq > 0.0001 ? normalize(cameraBackWS) : float3(0.0, 0.0, 1.0);
                float3 cameraRightWS = normalize(cross(float3(0.0, 1.0, 0.0), cameraBackWS));
                float3 blobOffsetWS =
                    cameraRightWS * blobUV.x * _ContactRadius * blobAspect.x +
                    cameraBackWS * blobUV.y * _ContactRadius * blobAspect.y +
                    cameraBackWS * _ContactCameraBackOffset;
                float3 blobWS = centerWS + blobOffsetWS;
                float3 contactWS = lerp(projectedWS, blobWS, blobMode);
                contactWS.y += _ContactLift;

                output.positionCS = TransformWorldToHClip(contactWS);
                float2 meshContactUV = ((projectedXZ - offsetOS) / blobAspect) / max(0.001, _ContactRadius);
                output.contactUV = lerp(meshContactUV, blobUV, blobMode);
                output.uv = ApplyTextureFitUV(TRANSFORM_TEX(input.uv, _BaseMap));
                return output;
            }

            half4 ContactFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half blobMode = saturate(max(_ContactBlobMode, _BillboardEnabled));
                half alpha = SampleBaseMapFitted(input.uv).a * _BaseColor.a * _Alpha;
                clip(lerp(alpha - lerp(-0.001, _AlphaClipThreshold, saturate(_AlphaClipEnabled)), 1.0, blobMode));

                half radius = length(input.contactUV);
                half edgeStart = saturate(1.0 - _ContactSoftness);
                half disk = 1.0 - smoothstep(edgeStart, 1.0, radius);
                half core = saturate(1.0 - radius * 0.55);
                half contactAlpha = lerp(alpha, 1.0, blobMode);
                half shadow = saturate(disk * core * _ContactDarkness * _ContactShadowEnabled * _ContactColor.a * contactAlpha);
                half3 multiplier = lerp(half3(1.0, 1.0, 1.0), _ContactColor.rgb, shadow);
                return half4(multiplier, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "DecorOutline" }

            Cull Front
            ZWrite Off
            ZTest [_OutlineZTest]
            Offset 1, 1
            Blend SrcAlpha OneMinusSrcAlpha

            Stencil
            {
                Ref [_EntityStencilRef]
                Comp NotEqual
                Pass Keep
            }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex OutlineVertex
            #pragma fragment OutlineFragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Alpha;
                half _AlphaClipEnabled;
                half _AlphaClipThreshold;
                half _BillboardEnabled;
                half4 _TextureFill;
                half4 _TextureFillOffset;
                half _TextureFitClamp;
                half _TextureVolumeStrength;
                half _TextureVolumeRoundness;
                half4 _TextureVolumeLightColor;
                half4 _TextureVolumeShadowColor;
                half4 _TextureVolumeDirection;
                half _TextureSaturation;
                half _TextureContrast;
                half _ColorPosterizeSteps;
                half _PosterizeStrength;
                half _LightStepCount;
                half _LightStepStrength;
                half _RimEnabled;
                half4 _StylizedRimColor;
                half _RimPower;
                half _LeafPlaneShading;
                half4 _LeafPlaneDirection;
                half _LeafShadeStrength;
                half _LeafLightStrength;
                half _LeafPlaneSoftness;
                half _LeafPlaneBalance;
                half4 _ShadowTint;
                half _AmbientStrength;
                half _LightStrength;
                half _MinimumBrightness;
                half _ShadowSoftness;
                half _OutlineEnabled;
                half4 _OutlineColor;
                half _OutlineWidth;
                half _OutlineScreenWidthPx;
                half _AlphaOutlineWidth;
                half _AlphaOutlineScreenWidthPx;
                half4 _ContactColor;
                half _ContactShadowEnabled;
                half _ContactBlobMode;
                half4 _ContactBlobAspect;
                half _ContactCameraBackOffset;
                half _ContactDarkness;
                half _ContactRadius;
                half _ContactSoftness;
                half _ContactProjectionScale;
                half _ContactLocalY;
                half _ContactLift;
                half4 _ContactOffsetOS;
            CBUFFER_END

            float2 ApplyTextureFitUV(float2 uv)
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

            half4 SampleBaseMapFitted(float2 uv)
            {
                half mask = TextureBoundsMask(uv);
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                tex *= mask;
                return tex;
            }

            float3 GetBillboardRightWS()
            {
                float3 rightWS = float3(UNITY_MATRIX_I_V._m00, 0.0, UNITY_MATRIX_I_V._m20);
                float rightLenSq = dot(rightWS, rightWS);
                return rightLenSq > 0.0001 ? normalize(rightWS) : float3(1.0, 0.0, 0.0);
            }

            float3 GetBillboardForwardWS(float3 rightWS)
            {
                return normalize(cross(float3(0.0, 1.0, 0.0), rightWS));
            }

            float3 TransformBillboardPositionWS(float3 positionOS)
            {
                float3 pivotWS = TransformObjectToWorld(float3(0.0, 0.0, 0.0));
                float rightScale = length(TransformObjectToWorld(float3(1.0, 0.0, 0.0)) - pivotWS);
                float upScale = length(TransformObjectToWorld(float3(0.0, 1.0, 0.0)) - pivotWS);
                float3 rightWS = GetBillboardRightWS();
                return pivotWS + rightWS * positionOS.x * rightScale + float3(0.0, 1.0, 0.0) * positionOS.y * upScale;
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings OutlineVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                half billboardEnabled = saturate(_BillboardEnabled);
                float3 billboardRightWS = GetBillboardRightWS();
                float3 meshPositionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 positionWS = lerp(meshPositionWS, TransformBillboardPositionWS(input.positionOS.xyz), billboardEnabled);
                float3 normalWS = lerp(
                    normalize(TransformObjectToWorldNormal(input.normalOS)),
                    GetBillboardForwardWS(billboardRightWS),
                    billboardEnabled);
                normalWS = normalize(normalWS);

                half outlineStrength = saturate(_OutlineEnabled);
                half outlineScreenWidthPx = _OutlineScreenWidthPx * outlineStrength;
                half screenOutlineEnabled = step(0.001, outlineScreenWidthPx);
                float3 worldOutlinePositionWS = positionWS + normalWS * _OutlineWidth * outlineStrength;
                output.positionCS = TransformWorldToHClip(worldOutlinePositionWS);

                float4 normalPositionCS = TransformWorldToHClip(worldOutlinePositionWS + normalWS);
                float2 positionNDC = output.positionCS.xy / max(0.0001, abs(output.positionCS.w));
                float2 normalNDC = normalPositionCS.xy / max(0.0001, abs(normalPositionCS.w));
                float2 outlineDirection = normalNDC - positionNDC;
                float outlineDirectionLengthSq = dot(outlineDirection, outlineDirection);
                outlineDirection = outlineDirectionLengthSq > 0.000001 ? normalize(outlineDirection) : float2(0.0, 1.0);
                float2 pixelToClip = 2.0 / _ScreenParams.xy;
                output.positionCS.xy += outlineDirection * pixelToClip * outlineScreenWidthPx * output.positionCS.w * screenOutlineEnabled;
                output.uv = ApplyTextureFitUV(TRANSFORM_TEX(input.uv, _BaseMap));
                return output;
            }

            half4 OutlineFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half outlineStrength = saturate(_OutlineEnabled);
                clip(outlineStrength - 0.001);
                clip(0.5 - saturate(_ContactBlobMode));

                half alpha = SampleBaseMapFitted(input.uv).a * _BaseColor.a * _Alpha;
                clip(alpha - lerp(-0.001, _AlphaClipThreshold, saturate(_AlphaClipEnabled)));

                return half4(_OutlineColor.rgb, _OutlineColor.a * outlineStrength);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            Cull [_CullMode]
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex ShadowVertex
            #pragma fragment ShadowFragment
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            float3 _LightDirection;
            float3 _LightPosition;

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Alpha;
                half _AlphaClipEnabled;
                half _AlphaClipThreshold;
                half _BillboardEnabled;
                half4 _TextureFill;
                half4 _TextureFillOffset;
                half _TextureFitClamp;
                half _TextureVolumeStrength;
                half _TextureVolumeRoundness;
                half4 _TextureVolumeLightColor;
                half4 _TextureVolumeShadowColor;
                half4 _TextureVolumeDirection;
                half _TextureSaturation;
                half _TextureContrast;
                half _ColorPosterizeSteps;
                half _PosterizeStrength;
                half _LightStepCount;
                half _LightStepStrength;
                half _RimEnabled;
                half4 _StylizedRimColor;
                half _RimPower;
                half _LeafPlaneShading;
                half4 _LeafPlaneDirection;
                half _LeafShadeStrength;
                half _LeafLightStrength;
                half _LeafPlaneSoftness;
                half _LeafPlaneBalance;
                half4 _ShadowTint;
                half _AmbientStrength;
                half _LightStrength;
                half _MinimumBrightness;
                half _ShadowSoftness;
                half _OutlineEnabled;
                half4 _OutlineColor;
                half _OutlineWidth;
                half _OutlineScreenWidthPx;
                half _AlphaOutlineWidth;
                half _AlphaOutlineScreenWidthPx;
                half4 _ContactColor;
                half _ContactShadowEnabled;
                half _ContactBlobMode;
                half4 _ContactBlobAspect;
                half _ContactCameraBackOffset;
                half _ContactDarkness;
                half _ContactRadius;
                half _ContactSoftness;
                half _ContactProjectionScale;
                half _ContactLocalY;
                half _ContactLift;
                half4 _ContactOffsetOS;
            CBUFFER_END

            float2 ApplyTextureFitUV(float2 uv)
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

            half4 SampleBaseMapFitted(float2 uv)
            {
                half mask = TextureBoundsMask(uv);
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                tex *= mask;
                return tex;
            }

            float3 GetBillboardRightWS()
            {
                float3 rightWS = float3(UNITY_MATRIX_I_V._m00, 0.0, UNITY_MATRIX_I_V._m20);
                float rightLenSq = dot(rightWS, rightWS);
                return rightLenSq > 0.0001 ? normalize(rightWS) : float3(1.0, 0.0, 0.0);
            }

            float3 GetBillboardForwardWS(float3 rightWS)
            {
                return normalize(cross(float3(0.0, 1.0, 0.0), rightWS));
            }

            float3 TransformBillboardPositionWS(float3 positionOS)
            {
                float3 pivotWS = TransformObjectToWorld(float3(0.0, 0.0, 0.0));
                float rightScale = length(TransformObjectToWorld(float3(1.0, 0.0, 0.0)) - pivotWS);
                float upScale = length(TransformObjectToWorld(float3(0.0, 1.0, 0.0)) - pivotWS);
                float3 rightWS = GetBillboardRightWS();
                return pivotWS + rightWS * positionOS.x * rightScale + float3(0.0, 1.0, 0.0) * positionOS.y * upScale;
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings ShadowVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                half billboardEnabled = saturate(_BillboardEnabled);
                float3 billboardRightWS = GetBillboardRightWS();
                float3 meshPositionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 billboardPositionWS = TransformBillboardPositionWS(input.positionOS.xyz);
                float3 positionWS = lerp(meshPositionWS, billboardPositionWS, billboardEnabled);

                float3 meshNormalWS = TransformObjectToWorldNormal(input.normalOS);
                float3 billboardNormalWS = GetBillboardForwardWS(billboardRightWS);
                float3 normalWS = normalize(lerp(meshNormalWS, billboardNormalWS, billboardEnabled));

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
                output.positionCS = ApplyShadowClamping(output.positionCS);
                output.uv = ApplyTextureFitUV(TRANSFORM_TEX(input.uv, _BaseMap));
                return output;
            }

            half4 ShadowFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half alpha = SampleBaseMapFitted(input.uv).a * _BaseColor.a * _Alpha;
                clip(alpha - lerp(-0.001, _AlphaClipThreshold, saturate(_AlphaClipEnabled)));

                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            Cull [_CullMode]
            ZWrite [_ZWrite]
            ColorMask 0

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthVertex
            #pragma fragment DepthFragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Alpha;
                half _AlphaClipEnabled;
                half _AlphaClipThreshold;
                half _BillboardEnabled;
                half4 _TextureFill;
                half4 _TextureFillOffset;
                half _TextureFitClamp;
                half _TextureVolumeStrength;
                half _TextureVolumeRoundness;
                half4 _TextureVolumeLightColor;
                half4 _TextureVolumeShadowColor;
                half4 _TextureVolumeDirection;
                half _TextureSaturation;
                half _TextureContrast;
                half _ColorPosterizeSteps;
                half _PosterizeStrength;
                half _LightStepCount;
                half _LightStepStrength;
                half _RimEnabled;
                half4 _StylizedRimColor;
                half _RimPower;
                half _LeafPlaneShading;
                half4 _LeafPlaneDirection;
                half _LeafShadeStrength;
                half _LeafLightStrength;
                half _LeafPlaneSoftness;
                half _LeafPlaneBalance;
                half4 _ShadowTint;
                half _AmbientStrength;
                half _LightStrength;
                half _MinimumBrightness;
                half _ShadowSoftness;
                half _OutlineEnabled;
                half4 _OutlineColor;
                half _OutlineWidth;
                half _OutlineScreenWidthPx;
                half _AlphaOutlineWidth;
                half _AlphaOutlineScreenWidthPx;
                half4 _ContactColor;
                half _ContactShadowEnabled;
                half _ContactBlobMode;
                half4 _ContactBlobAspect;
                half _ContactCameraBackOffset;
                half _ContactDarkness;
                half _ContactRadius;
                half _ContactSoftness;
                half _ContactProjectionScale;
                half _ContactLocalY;
                half _ContactLift;
                half4 _ContactOffsetOS;
            CBUFFER_END

            float2 ApplyTextureFitUV(float2 uv)
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

            half4 SampleBaseMapFitted(float2 uv)
            {
                half mask = TextureBoundsMask(uv);
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                tex *= mask;
                return tex;
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
                float3 rightWS = GetBillboardRightWS();
                return pivotWS + rightWS * positionOS.x * rightScale + float3(0.0, 1.0, 0.0) * positionOS.y * upScale;
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings DepthVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                half billboardEnabled = saturate(_BillboardEnabled);
                float3 meshPositionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 billboardPositionWS = TransformBillboardPositionWS(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(lerp(meshPositionWS, billboardPositionWS, billboardEnabled));
                output.uv = ApplyTextureFitUV(TRANSFORM_TEX(input.uv, _BaseMap));
                return output;
            }

            half4 DepthFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half alpha = SampleBaseMapFitted(input.uv).a * _BaseColor.a * _Alpha;
                clip(alpha - lerp(-0.001, _AlphaClipThreshold, saturate(_AlphaClipEnabled)));

                return 0;
            }
            ENDHLSL
        }
    }

    CustomEditor "Kruty1918.Moyva.Visuals.Editor.DecorSharedStylizedShaderGUI"
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
