#ifndef MOYVA_UBER_STYLIZED_WATER_CUSTOM_LIGHTING_INCLUDED
#define MOYVA_UBER_STYLIZED_WATER_CUSTOM_LIGHTING_INCLUDED

void MainLight_float(out float3 Direction, out float3 Color, out float DistanceAtten)
{
#if defined(SHADERGRAPH_PREVIEW)
    Direction = normalize(float3(0.4, 0.8, 0.2));
    Color = 1.0;
    DistanceAtten = 1.0;
#else
    Light mainLight = GetMainLight();
    Direction = mainLight.direction;
    Color = mainLight.color;
    DistanceAtten = mainLight.distanceAttenuation;
#endif
}

void MainLight_half(out half3 Direction, out half3 Color, out half DistanceAtten)
{
    float3 direction;
    float3 color;
    float distanceAtten;
    MainLight_float(direction, color, distanceAtten);
    Direction = (half3)direction;
    Color = (half3)color;
    DistanceAtten = (half)distanceAtten;
}

void MainLightShadows_float(float3 WorldPosition, half4 Shadowmask, out float ShadowAtten)
{
#if defined(SHADERGRAPH_PREVIEW)
    ShadowAtten = 1.0;
#else
    #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE) || defined(_MAIN_LIGHT_SHADOWS_SCREEN)
        #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
            float4 clipPos = TransformWorldToHClip(WorldPosition);
            float4 shadowCoord = ComputeScreenPos(clipPos);
        #else
            float4 shadowCoord = TransformWorldToShadowCoord(WorldPosition);
        #endif
        Light mainLight = GetMainLight(shadowCoord);
        ShadowAtten = mainLight.shadowAttenuation;
    #else
        ShadowAtten = 1.0;
    #endif
#endif
}

void MainLightShadows_half(half3 WorldPosition, half4 Shadowmask, out half ShadowAtten)
{
    float shadowAtten;
    MainLightShadows_float((float3)WorldPosition, Shadowmask, shadowAtten);
    ShadowAtten = (half)shadowAtten;
}

void AdditionalLights_float(float3 SpecColor, float Smoothness, float3 WorldPosition, float3 WorldNormal, float3 WorldView, half4 Shadowmask, out float3 Diffuse, out float3 Specular)
{
    Diffuse = 0.0;
    Specular = 0.0;

#if !defined(SHADERGRAPH_PREVIEW)
    float3 normalWS = SafeNormalize(WorldNormal);
    float3 viewWS = SafeNormalize(WorldView);
    float smoothness = exp2(10.0 * saturate(Smoothness) + 1.0);

    uint lightCount = GetAdditionalLightsCount();
    for (uint lightIndex = 0u; lightIndex < lightCount; ++lightIndex)
    {
        Light light = GetAdditionalLight(lightIndex, WorldPosition);
        float3 attenuatedColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
        Diffuse += LightingLambert(attenuatedColor, light.direction, normalWS);
        Specular += LightingSpecular(attenuatedColor, light.direction, normalWS, viewWS, float4(SpecColor, 0.0), smoothness);
    }
#endif
}

void AdditionalLights_half(half3 SpecColor, half Smoothness, half3 WorldPosition, half3 WorldNormal, half3 WorldView, half4 Shadowmask, out half3 Diffuse, out half3 Specular)
{
    float3 diffuse;
    float3 specular;
    AdditionalLights_float((float3)SpecColor, (float)Smoothness, (float3)WorldPosition, (float3)WorldNormal, (float3)WorldView, Shadowmask, diffuse, specular);
    Diffuse = (half3)diffuse;
    Specular = (half3)specular;
}

void Shadowmask_half(float2 UV, out half4 Mask)
{
    Mask = half4(1.0, 1.0, 1.0, 1.0);
}

void Shadowmask_float(float2 UV, out float4 Mask)
{
    Mask = float4(1.0, 1.0, 1.0, 1.0);
}

#endif
