#ifndef CUSTOM_FUNCTION_INCLUDED
#define CUSTOM_FUNCTION_INCLUDED

#ifndef SHADERGRAPH_PREVIEW
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#endif

void AdaptiveProbeVolumeSample_half(in half3 ViewDirWS, in half3 NormalWS, in float3 PositionWS, in half3 Albedo, in half Metallic, out half3 Out)
{
    Out = half3(1, 0, 0);
#ifndef SHADERGRAPH_PREVIEW
#if defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2)
    half2 ss= half2(0,0);
    half reflectivity = (1.0 - Metallic) * 0.96;
    half3 outColor;
    EvaluateAdaptiveProbeVolume(
         PositionWS,
         NormalWS,
         ViewDirWS,
         ss,
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

#endif