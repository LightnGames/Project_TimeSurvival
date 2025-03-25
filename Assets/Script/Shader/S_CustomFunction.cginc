#ifndef CUSTOM_FUNCTION_INCLUDED
#define CUSTOM_FUNCTION_INCLUDED

#ifndef SHADERGRAPH_PREVIEW
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"
#endif

void AdaptiveProbeVolumeSample_half(in half3 ViewDirWS, in half3 NormalWS, in half3 PositionWS, in half2 PositionCS, in half3 Albedo, in half Metallic, out half3 Out)
{
    Out = half3(0, 0, 0);
#ifndef SHADERGRAPH_PREVIEW
#if defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2)
    half reflectivity = (1.0 - Metallic) * 0.96;
    half3 outColor;
    EvaluateAdaptiveProbeVolume(
         PositionWS,
         NormalWS,
         ViewDirWS,
         PositionCS,
         outColor.rgb
     );
    Out = outColor * Albedo * reflectivity;
#endif
#endif
}

// https://www.cnblogs.com/hont/p/15221001.html
void SpecularOcclusionFromBentNormal_half(in half EffectScale, in half3 ViewDirWS, in half3 NormalWS, in half3 BentNormalWS, out half Out)
{
    half3 reflectVector = reflect(-ViewDirWS, NormalWS);
    half bentNormalIntensity = 1.0 - saturate(dot(NormalWS, BentNormalWS));
    half weight = max(dot(BentNormalWS, reflectVector), 0.0);
    //Out = weight;
    Out = saturate(lerp(1.0, weight, EffectScale * bentNormalIntensity));
}

void LightingSpotLight_half(in half3 Albedo, in half Smoothness, in half Metallic, in half3 PositionWS, in half3 NormalWS, in half3 ViewDirWS, out half3 Out)
{
#ifndef SHADERGRAPH_PREVIEW
    uint pixelLightCount = GetAdditionalLightsCount();
    if (pixelLightCount == 0)
    {
        Out = half3(0, 0, 0);
        return;
    }
    Light light = GetAdditionalPerObjectLight(0, PositionWS);
    
    half alpha = 1.0;
    BRDFData brdfData;
    InitializeBRDFData(Albedo, Metallic, 1, Smoothness, alpha, brdfData);
    
    half NdotL = saturate(dot(NormalWS, light.direction));
    half3 radiance = light.color * (light.distanceAttenuation * NdotL);
    half3 brdf = LightingLambert(half3(1, 1, 1), light.direction, NormalWS) + DirectBRDFSpecular(brdfData, NormalWS, light.direction, ViewDirWS);
    Out = radiance * brdf;
#else
    Out = half3(0, 0, 0);
#endif
}

#endif