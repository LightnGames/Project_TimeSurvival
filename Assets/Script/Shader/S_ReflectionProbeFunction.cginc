#ifndef REFLECTION_PROBE_FUNCTION_INCLUDED
#define REFLECTION_PROBE_FUNCTION_INCLUDED

#pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
#pragma multi_compile _ _FORWARD_PLUS
#ifndef SHADERGRAPH_PREVIEW
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"
#endif

void ReflectionProbe_half(in half3 ViewDirWS, in half3 NormalWS, in half3 PositionWS, in half LOD, in half2 ScreenPosition, out half3 Out)
{
#ifdef SHADERGRAPH_PREVIEW
     Out = half3(0, 0, 0);
#else
    // Defined when using Forward+
    half3 reflDir = reflect(-ViewDirWS, NormalWS);
#if USE_FORWARD_PLUS
    Out = GlossyEnvironmentReflection(reflDir, PositionWS, LOD, ScreenPosition);
#else

    half3 factors = ((reflDir > 0 ? unity_SpecCube0_BoxMax.xyz : unity_SpecCube0_BoxMin.xyz) - PositionWS) / reflDir;
    half scalar = min(min(factors.x, factors.y), factors.z);
    half3 uvw = reflDir * scalar + (PositionWS - unity_SpecCube0_ProbePosition.xyz);

    half4 sampleRefl = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, uvw, LOD);
    half3 specCol = DecodeHDREnvironment(sampleRefl, unity_SpecCube0_HDR);
    Out = specCol;
#endif
#endif
}

#endif