#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED
#pragma multi_compile _ _ADDITIONAL_LIGHTS_SHADOWS

void DiffSpec_half(half3 Normal, half3 View, half3 WorldPos, half Smoothness, half3 SpecColor, out half3 Diffuse, out half3 Specular)
{
	Diffuse = 0;
	Specular = 0;
#if SHADERGRAPH_PREVIEW
    Smoothness = exp2(10 * Smoothness + 1);
#else
#if SHADOWS_SCREEN
    float4 clipPos = TransformWorldToHClip(WorldPos);
    float4 shadowCoord = ComputeScreenPos(clipPos);
#else
    float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
#endif
    Light light = GetMainLight(shadowCoord);
	half3 attenuatedColor = light.color * light.distanceAttenuation * light.shadowAttenuation;
	Diffuse += saturate(dot(Normal,light.direction))*attenuatedColor;
	//Lambert_half(attenuatedColor, light.direction, Normal)
	Diffuse += LightingSpecular(attenuatedColor, light.direction, Normal, View, half4(SpecColor, 0), Smoothness);
    int pixelLightCount = GetAdditionalLightsCount(); 
    for (int i = 0; i < pixelLightCount; ++i)
    {
        Light light = GetAdditionalLight(i, WorldPos);
        half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
        Diffuse += LightingLambert(attenuatedLightColor, light.direction, Normal);
        Specular += LightingSpecular(attenuatedLightColor, light.direction, Normal, View, half4(SpecColor, 0), Smoothness);
    }
#endif
}

#endif 