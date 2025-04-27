#ifndef CUSTOM_ASEC_FUNCTION_INCLUDED
#define CUSTOM_ASEC_FUNCTION_INCLUDED

#ifndef SHADERGRAPH_PREVIEW
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
#endif

inline half3 UnityACES(in float3 Color)
{
    const float a = 2.51;
    const float b = 0.03;
    const float c = 2.43;
    const float d = 0.59;
    const float e = 0.14;
    return saturate(
                    Color * (a * Color + b) /
                    (Color * (c * Color + d) + e));
}

float3 AcesTonemap_UE4(float3 aces)
{
#ifndef SHADERGRAPH_PREVIEW
    const float FilmSlope = 0.91;
    const float FilmToe = 0.53;
    const float FilmShoulder = 0.23;
    const float FilmBlackClip = 0;
    const float FilmWhiteClip = 0.035;
    
				// "Glow" module constants
    const float RRT_GLOW_GAIN = 0.05;
    const float RRT_GLOW_MID = 0.08;
			 
    float saturation = rgb_2_saturation(aces);
    float ycIn = rgb_2_yc(aces);
    float s = sigmoid_shaper((saturation - 0.4) / 0.2);
    float addedGlow = 1.0 + glow_fwd(ycIn, RRT_GLOW_GAIN * s, RRT_GLOW_MID);
    aces *= addedGlow;
			 
    const float RRT_RED_SCALE = 0.82;
    const float RRT_RED_PIVOT = 0.03;
    const float RRT_RED_HUE = 0.0;
    const float RRT_RED_WIDTH = 135.0;
			 
			    // --- Red modifier --- //
    float hue = rgb_2_hue(aces);
    float centeredHue = center_hue(hue, RRT_RED_HUE);
    float hueWeight;
			    {
        hueWeight = smoothstep(0.0, 1.0, 1.0 - abs(2.0 * centeredHue / RRT_RED_WIDTH));
        hueWeight *= hueWeight;
    }
			    //float hueWeight = Square( smoothstep(0.0, 1.0, 1.0 - abs(2.0 * centeredHue / RRT_RED_WIDTH)) );
			 
    aces.r += hueWeight * saturation * (RRT_RED_PIVOT - aces.r) * (1.0 - RRT_RED_SCALE);
			 
			    // Use ACEScg primaries as working space
    float3 acescg = max(0.0, ACES_to_ACEScg(aces));
			 
			    // Pre desaturate
    acescg = lerp(dot(acescg, AP1_RGB2Y).xxx, acescg, 0.96);
			 
    const half ToeScale = 1 + FilmBlackClip - FilmToe;
    const half ShoulderScale = 1 + FilmWhiteClip - FilmShoulder;
			 
    const float InMatch = 0.18;
    const float OutMatch = 0.18;
			 
    float ToeMatch;
    if (FilmToe > 0.8)
    {
			        // 0.18 will be on straight segment
        ToeMatch = (1 - FilmToe - OutMatch) / FilmSlope + log10(InMatch);
    }
    else
    {
			        // 0.18 will be on toe segment
			 
			        // Solve for ToeMatch such that input of InMatch gives output of OutMatch.
        const float bt = (OutMatch + FilmBlackClip) / ToeScale - 1;
        ToeMatch = log10(InMatch) - 0.5 * log((1 + bt) / (1 - bt)) * (ToeScale / FilmSlope);
    }
			 
    float StraightMatch = (1 - FilmToe) / FilmSlope - ToeMatch;
    float ShoulderMatch = FilmShoulder / FilmSlope - StraightMatch;
			 
    half3 LogColor = log10(acescg);
    half3 StraightColor = FilmSlope * (LogColor + StraightMatch);
			 
    half3 ToeColor = (-FilmBlackClip) + (2 * ToeScale) / (1 + exp((-2 * FilmSlope / ToeScale) * (LogColor - ToeMatch)));
    half3 ShoulderColor = (1 + FilmWhiteClip) - (2 * ShoulderScale) / (1 + exp((2 * FilmSlope / ShoulderScale) * (LogColor - ShoulderMatch)));
			 
    ToeColor = LogColor < ToeMatch ? ToeColor : StraightColor;
    ShoulderColor = LogColor > ShoulderMatch ? ShoulderColor : StraightColor;
			 
    half3 t = saturate((LogColor - ToeMatch) / (ShoulderMatch - ToeMatch));
    t = ShoulderMatch < ToeMatch ? 1 - t : t;
    t = (3 - 2 * t) * t * t;
    half3 linearCV = lerp(ToeColor, ShoulderColor, t);
			 
			    // Post desaturate
    linearCV = lerp(dot(float3(linearCV), AP1_RGB2Y), linearCV, 0.93);
			 
			    // Returning positive AP1 values
			    //return max(0, linearCV);
			 
			    // Convert to display primary encoding
			    // Rendering space RGB to XYZ
    float3 XYZ = mul(AP1_2_XYZ_MAT, linearCV);
			 
			    // Apply CAT from ACES white point to assumed observer adapted white point
    XYZ = mul(D60_2_D65_CAT, XYZ);
			 
			    // CIE XYZ to display primaries
    linearCV = mul(XYZ_2_REC709_MAT, XYZ);
			 
    linearCV = saturate(linearCV); //Protection to make negative return out.
			 
    return linearCV;
#else
    return aces;
#endif
}

float3 AcesTonemap_Unity(float3 aces)
{
#ifndef SHADERGRAPH_PREVIEW
#if TONEMAPPING_USE_FULL_ACES

				    float3 oces = RRT(aces);
				    float3 odt = ODT_RGBmonitor_100nits_dim(oces);
				    return odt;

#else

				    // --- Glow module --- //
    float saturation = rgb_2_saturation(aces);
    float ycIn = rgb_2_yc(aces);
    float s = sigmoid_shaper((saturation - 0.4) / 0.2);
    float addedGlow = 1.0 + glow_fwd(ycIn, RRT_GLOW_GAIN * s, RRT_GLOW_MID);
    aces *= addedGlow;

				    // --- Red modifier --- //
    float hue = rgb_2_hue(aces);
    float centeredHue = center_hue(hue, RRT_RED_HUE);
    float hueWeight;
				    {
				        //hueWeight = cubic_basis_shaper(centeredHue, RRT_RED_WIDTH);
        hueWeight = smoothstep(0.0, 1.0, 1.0 - abs(2.0 * centeredHue / RRT_RED_WIDTH));
        hueWeight *= hueWeight;
    }

    aces.r += hueWeight * saturation * (RRT_RED_PIVOT - aces.r) * (1.0 - RRT_RED_SCALE);

				    // --- ACES to RGB rendering space --- //
    float3 acescg = max(0.0, ACES_to_ACEScg(aces));

				    // --- Global desaturation --- //
				    //acescg = mul(RRT_SAT_MAT, acescg);
    acescg = lerp(dot(acescg, AP1_RGB2Y).xxx, acescg, RRT_SAT_FACTOR.xxx);

				    // Luminance fitting of *RRT.a1.0.3 + ODT.Academy.RGBmonitor_100nits_dim.a1.0.3*.
				    // https://github.com/colour-science/colour-unity/blob/master/Assets/Colour/Notebooks/CIECAM02_Unity.ipynb
				    // RMSE: 0.0012846272106
#if defined(SHADER_API_SWITCH) // Fix floating point overflow on extremely large values.
				    const float a = 2.785085 * 0.01;
				    const float b = 0.107772 * 0.01;
				    const float c = 2.936045 * 0.01;
				    const float d = 0.887122 * 0.01;
				    const float e = 0.806889 * 0.01;
				    float3 x = acescg;
				    float3 rgbPost = ((a * x + b)) / ((c * x + d) + e/(x + FLT_MIN));
#else
    const float a = 2.785085;
    const float b = 0.107772;
    const float c = 2.936045;
    const float d = 0.887122;
    const float e = 0.806889;
    float3 x = acescg;
    float3 rgbPost = (x * (a * x + b)) / (x * (c * x + d) + e);
#endif

				    // Scale luminance to linear code value
				    // float3 linearCV = Y_2_linCV(rgbPost, CINEMA_WHITE, CINEMA_BLACK);

				    // Apply gamma adjustment to compensate for dim surround
    float3 linearCV = darkSurround_to_dimSurround(rgbPost);

				    // Apply desaturation to compensate for luminance difference
				    //linearCV = mul(ODT_SAT_MAT, color);
    linearCV = lerp(dot(linearCV, AP1_RGB2Y).xxx, linearCV, ODT_SAT_FACTOR.xxx);

				    // Convert to display primary encoding
				    // Rendering space RGB to XYZ
    float3 XYZ = mul(AP1_2_XYZ_MAT, linearCV);

				    // Apply CAT from ACES white point to assumed observer adapted white point
    XYZ = mul(D60_2_D65_CAT, XYZ);

				    // CIE XYZ to display primaries
    linearCV = mul(XYZ_2_REC709_MAT, XYZ);

    return linearCV;

#endif
#else
    return aces;
#endif
}

void ACES_float(in float3 Color, out half3 Out)
{    
#ifndef SHADERGRAPH_PREVIEW
    float3 colorACES = unity_to_ACES(Color);
    Out = AcesTonemap_UE4(colorACES);
    //Out = AcesTonemap_Unity(colorACES);
    //Out = ACES_to_unity(UnityACES(colorACES));
#else
    Out = Color;
#endif
}

void ApplyLut_half(in half3 Color, in UnityTexture2D LutTexture, in UnitySamplerState Sampler, in half3 UserLutParams, in half UserLutContrib, out half3 Out)
{
    half3 srgbColor = FastLinearToSRGB(Color);
    half3 scaleOffset = UserLutParams;
    half3 uvw = srgbColor;
    uvw.z *= scaleOffset.z;
    float shift = floor(uvw.z);
    uvw.xy = uvw.xy * scaleOffset.z * scaleOffset.xy + scaleOffset.xy * 0.5;
    uvw.x += shift * scaleOffset.y;
    uvw.xyz = lerp(
        SAMPLE_TEXTURE2D_LOD(LutTexture, Sampler, uvw.xy, 0.0).rgb,
        SAMPLE_TEXTURE2D_LOD(LutTexture, Sampler, uvw.xy + float2(scaleOffset.y, 0.0), 0.0).rgb,
        uvw.z - shift
    );
    half3 outLut = uvw.xyz;
    half3 outSrgbColor = lerp(srgbColor, outLut, UserLutContrib);
    Out = FastSRGBToLinear(outSrgbColor);
}

#endif