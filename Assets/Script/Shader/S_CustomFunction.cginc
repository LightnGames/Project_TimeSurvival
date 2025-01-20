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

//void ReflectionProbe_half(in half3 ViewDirWS, in half3 NormalWS, in half3 PositionWS, in half LOD, out half3 Out)
//{
//#ifdef SHADERGRAPH_PREVIEW
//     Out = half3(0, 0, 0);
//#else
//half3 viewDirWS = ViewDirWS;
//half3 normalWS = NormalWS;
//half3 reflDir = reflect(-viewDirWS, normalWS);

//half3 factors = ((reflDir > 0 ? unity_SpecCube0_BoxMax.xyz : unity_SpecCube0_BoxMin.xyz) - PositionWS) / reflDir;
//half scalar = min(min(factors.x, factors.y), factors.z);
//half3 uvw = reflDir * scalar + (PositionWS - unity_SpecCube0_ProbePosition.xyz);

//half4 sampleRefl = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, uvw, LOD);
//half3 specCol = DecodeHDREnvironment(sampleRefl, unity_SpecCube0_HDR);
//    Out = specCol;
//#endif
//}

#endif