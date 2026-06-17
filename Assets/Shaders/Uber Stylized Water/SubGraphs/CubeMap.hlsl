#ifndef MOYVA_UBER_STYLIZED_WATER_CUBEMAP_INCLUDED
#define MOYVA_UBER_STYLIZED_WATER_CUBEMAP_INCLUDED

void GetCubemap_float(float3 ViewDirection, float3 PositionWS, float3 NormalWS, float Roughness, out float3 Cubemap)
{
#if defined(SHADERGRAPH_PREVIEW)
    Cubemap = float3(0.4, 0.55, 0.75);
#else
    float3 viewDir = SafeNormalize(ViewDirection);
    float3 normalWS = SafeNormalize(NormalWS);
    float3 reflectDir = reflect(-viewDir, normalWS);
    float mip = saturate(Roughness) * 6.0;
    float4 encoded = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectDir, mip);
    Cubemap = DecodeHDREnvironment(encoded, unity_SpecCube0_HDR);
#endif
}

void GetCubemap_half(half3 ViewDirection, half3 PositionWS, half3 NormalWS, half Roughness, out half3 Cubemap)
{
    float3 color;
    GetCubemap_float((float3)ViewDirection, (float3)PositionWS, (float3)NormalWS, (float)Roughness, color);
    Cubemap = (half3)color;
}

#endif
