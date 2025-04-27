#ifndef SDF_FUNCTION_INCLUDED
#define SDF_FUNCTION_INCLUDED

half3 calcNormal(half3 pos, float eps, in UnityTexture3D SdfTexture, in UnitySamplerState Sampler)
{
    const half3 v1 = half3(1.0, -1.0, -1.0);
    const half3 v2 = half3(-1.0, -1.0, 1.0);
    const half3 v3 = half3(-1.0, 1.0, -1.0);
    const half3 v4 = half3(1.0, 1.0, 1.0);

    return normalize(v1 * SAMPLE_TEXTURE3D(SdfTexture, Sampler, pos + v1 * eps).x +
                    v2 * SAMPLE_TEXTURE3D(SdfTexture, Sampler, pos + v2 * eps).x +
                    v3 * SAMPLE_TEXTURE3D(SdfTexture, Sampler, pos + v3 * eps).x +
                    v4 * SAMPLE_TEXTURE3D(SdfTexture, Sampler, pos + v4 * eps).x);
}

void SdfStep_half(in UnityTexture3D SdfTexture, in UnityTextureCube ProbeTexture, in half3 ViewDirWS, in half3 NormalWS, in half3 PositionWS, in half PerceptualRoughness, in half2 ScreenPosition, in UnitySamplerState Sampler, out half3 Out, out half Ao)
{
#ifdef SHADERGRAPH_PREVIEW
     Out = half3(0, 0, 0);
    Ao = 1.0;
#else
    // Defined when using Forward+
    half3 reflDir = reflect(-ViewDirWS, NormalWS);
#if USE_FORWARD_PLUS
#endif
    half3 centerOffset = half3(-0.09, -1.6, -6.1);
    half3 boxExtent = half3(10.0, 5.0, 25.0);
    half boxExtentMax = max(boxExtent.x, max(boxExtent.y, boxExtent.z));
    half3 currentPosition = PositionWS;
    half3 uvw = half3(0, 0, 0);
    half rayDistance = 0.05;
    for (int i = 0; i < 10; ++i)
    {
        currentPosition = PositionWS + reflDir * rayDistance;
        uvw = (currentPosition + centerOffset + boxExtent * 0.5) / boxExtent;
        //uvw = half3(uvw.x, 0,uvw.z);
        half uniformStepDistance = SAMPLE_TEXTURE3D(SdfTexture, Sampler, uvw).r;
        half stepDistance = uniformStepDistance * boxExtentMax;
        rayDistance += stepDistance;
        
        if (abs(stepDistance) < 0.03)
        {
            break;
        }
        
        if (uvw.x > 1.0 || uvw.y > 1.0 || uvw.z > 1.0)
        {
            break;
        }
        
        if (uvw.x < 0.0 || uvw.y < 0.0 || uvw.z < 0.0)
        {
            break;
        }
    }
    
    half3 normal = calcNormal(uvw, 0.002, SdfTexture, Sampler);
    //if (abs(normal.x) > abs(normal.y) && abs(normal.x) > abs(normal.z))
    //{
    //    normal = half3(normal.x > 0 ? 1 : -1, 0, 0);
    //}
    //else if (abs(normal.y) > abs(normal.x) && abs(normal.y) > abs(normal.z))
    //{
    //    normal = half3(0, normal.y > 0 ? 1 : -1, 0);
    //}
    //else if (abs(normal.z) > abs(normal.x) && abs(normal.z) > abs(normal.y))
    //{
    //    normal = half3(0, 0, normal.z > 0 ? 1 : -1);
    //}
    Out = (normal + 1.0) / 2.0;
    //Out = currentPosition;
    //Out = half3(rayDistance, 0, 0);
    //Out = SAMPLE_TEXTURE3D(SdfTexture, Sampler, ((PositionWS + centerOffset + boxExtent * 0.5) / boxExtent)).rrr;
    half3 cubeMapSpaceWS = currentPosition - unity_SpecCube0_ProbePosition.xyz;
    half3 probeSize = unity_SpecCube0_BoxMax.xyz - unity_SpecCube0_BoxMin.xyz;
    half3 cubeMapUvw = cubeMapSpaceWS;// / probeSize;
    //half3 reflectVector = BoxProjectedCubemapDirection(normal, currentPosition, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
    half mip = PerceptualRoughnessToMipmapLevel(PerceptualRoughness);
    half4 encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, cubeMapUvw, mip));

    Out = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
    Ao = saturate(rayDistance * 5.0);
    //Out = cubeMapUvw -floor(cubeMapUvw);
#endif
}

#endif