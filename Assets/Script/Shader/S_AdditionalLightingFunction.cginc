#ifndef ADDITIONAL_LIGHTING_FUNCTION_INCLUDED
#define ADDITIONAL_LIGHTING_FUNCTION_INCLUDED

#ifndef SHADERGRAPH_PREVIEW
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
#endif

struct TempLightInputData
{
    half2 normalizedScreenSpaceUV;
    half3 positionWS;
};

void LightingSpotLight_half(in half3 Albedo, in half Smoothness, in half Metallic, in half3 PositionWS, in half3 NormalWS, in half3 ViewDirWS, in half2 NormalizedScreenSpaceUV, out half3 Out)
{
#ifndef SHADERGRAPH_PREVIEW
    half alpha = 1.0;
    BRDFData brdfData;
    InitializeBRDFData(Albedo, Metallic, 1, Smoothness, alpha, brdfData);
    
    half3 result = half3(0, 0, 0);
    uint lightsCount = GetAdditionalLightsCount();
    TempLightInputData inputData;
    inputData.normalizedScreenSpaceUV = NormalizedScreenSpaceUV;
    inputData.positionWS = PositionWS;
    LIGHT_LOOP_BEGIN(lightsCount)

    Light light = GetAdditionalLight(lightIndex, PositionWS);
    
    result += LightingPhysicallyBased(brdfData, brdfData, light, NormalWS, ViewDirWS, 0, 0);
    LIGHT_LOOP_END
    
    Out = result;
#else
    Out = half3(0, 0, 0);
#endif
}

#endif