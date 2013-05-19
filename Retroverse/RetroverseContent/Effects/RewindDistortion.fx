//Distort pixels left/right for a VCR rewind effect

texture2D Texture;
sampler2D TextureSampler = sampler_state
{
	Texture = <Texture>;
};

const float MAX_WAVEFREQUENCY = 100;
const float MAX_WAVEAMPLITUDE = 1;
const float MAX_GRANULARITY = 100;

float granularity = 0;
float waveFrequency = 0;
float waveAmplitude = 0;
float waveOffset = 0;
float phaseOffset = 0;

float4 GetShiftAmount(float2 coords){
	float _granularity = saturate(granularity) * MAX_GRANULARITY;
	float _waveFrequency = saturate(waveFrequency) * MAX_WAVEFREQUENCY;
	float _waveAmplitude = saturate(waveAmplitude) * MAX_WAVEAMPLITUDE;
	float shift = waveOffset + round(sin((coords.y + phaseOffset) * _waveFrequency) * _waveAmplitude * _granularity) / _granularity;
	return shift;
}

float4 DistortRightFunction(float2 coords : TEXCOORD0) : COLOR0{
	float shift = GetShiftAmount(coords);
	shift = 0 - saturate(shift);
	float finalX = coords.x + shift;
	if (finalX < 0)
		return float4(0,0,0,1);
    return tex2D(TextureSampler, float2(finalX, coords.y));
}

float4 DistortLeftFunction(float2 coords : TEXCOORD0) : COLOR0{
	float shift = GetShiftAmount(coords);
	shift = saturate(shift);
	return tex2D(TextureSampler, float2(coords.x + shift, coords.y));
}

technique DistortLeft
{
    pass main
    {
        PixelShader = compile ps_2_0 DistortLeftFunction();
    }
}

technique DistortRight
{
    pass main
    {
        PixelShader = compile ps_2_0 DistortRightFunction();
    }
}
