#ifndef CUSTOM_ADAPTIBE_PROBE_VOLUME_SAMPLE_FUNCTION_INCLUDED
#define CUSTOM_ADAPTIBE_PROBE_VOLUME_SAMPLE_FUNCTION_INCLUDED

#ifndef SHADERGRAPH_PREVIEW
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"
#endif

void AdaptiveProbeVolumeSample_float(in float3 ViewDirWS, in float3 NormalWS, in float3 PositionWS, in float2 PositionCS, in float3 Albedo, in float Metallic, out float3 Out)
{
    Out = float3(0, 0, 0);
    float reflectivity = (1.0 - Metallic) * 0.96;
#ifndef SHADERGRAPH_PREVIEW
#if defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2)
    float3 outColor;
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

#endif