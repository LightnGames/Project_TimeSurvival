#ifndef REFLECTION_PROBE_FUNCTION_INCLUDED
#define REFLECTION_PROBE_FUNCTION_INCLUDED

#pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
#pragma multi_compile _ _FORWARD_PLUS
#ifndef SHADERGRAPH_PREVIEW
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"
#endif

void ReflectionProbe_half(in half3 ViewDirWS, in half3 NormalWS, in half3 PositionWS, in half PerceptualRoughness, in half2 ScreenPosition, out half3 Out)
{
#ifdef SHADERGRAPH_PREVIEW
     Out = half3(0, 0, 0);
#else
    // Defined when using Forward+
    half3 reflDir = reflect(-ViewDirWS, NormalWS);
    Out = GlossyEnvironmentReflection(reflDir, PositionWS, PerceptualRoughness, 1.0, ScreenPosition);
#endif
}

#endif