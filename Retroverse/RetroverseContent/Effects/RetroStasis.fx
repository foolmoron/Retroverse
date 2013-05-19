// Lightly grays out a ring between innerradius and outerradius at the center position

float width;
float height;
float innerradius;
float outerradius;
float intensity;
float zoom;
float2 center;
texture2D Texture;
sampler2D TextureSampler = sampler_state
{
	Texture = <Texture>;
};

struct PixelShaderInput
{
	float2 TexCoord : TEXCOORD0;
};

float weightR = 0.30;
float weightG = 0.59;
float weightB = 0.11;

float negativeFactor = 0.25;

float4 PixelShaderFunction(PixelShaderInput input) : COLOR0
{
	float3 color;
	float4 original = tex2D(TextureSampler, input.TexCoord);
	
	float innerrad2 = pow(innerradius / zoom, 2);
	float outerrad2 = pow(outerradius / zoom, 2);
	float dist2 = pow((input.TexCoord.x - center.x) * width, 2) + pow((input.TexCoord.y - center.y) * height, 2);
	
	if (dist2 > outerrad2)
	{
		return original;
	}
	if (dist2 > innerrad2)
	{
		float luminance = dot(original.rgb,  float3(weightR, weightG, weightB));
		float3 gray = (luminance > 0.5f) ? (luminance + 1) / 2 : luminance / 2;
		color = 1 - luminance;
		color = color * negativeFactor + original.rgb * (1 - negativeFactor);
	}
	else
	{
		float luminance = dot(original.rgb,  float3(weightR, weightG, weightB));
		float3 gray = (luminance > 0.5f) ? (luminance + 1) / 2 : luminance / 2;
		float perc = pow((dist2 / innerrad2), intensity) * negativeFactor;
		color = ((1-luminance) * (perc)) + (original.rgb * (1 - perc));
	}
		return float4(color, original.a);
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}