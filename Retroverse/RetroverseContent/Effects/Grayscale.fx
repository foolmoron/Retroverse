float width;
float height;
float radius;
float intensity;
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

float4 PixelShaderFunction(PixelShaderInput input) : COLOR0
{
	float3 color;
	float4 original = tex2D(TextureSampler, input.TexCoord);
	float hPixel = 1.0f / width;
	float vPixel = 1.0f / height;

	float rad2 = pow(radius, 2);
	float dist2 = pow((input.TexCoord.x * width) - width/2, 2) + pow((input.TexCoord.y * height)- height/2, 2);
	
	if (dist2 > rad2)
	{
		return original;
	}
	else
	{
		float luminance = dot(original.rgb,  float3(weightR, weightG, weightB));
		float3 gray = (luminance > 0.5f) ? (luminance + 1) / 2 : luminance / 2;
		float perc = pow((dist2 / rad2), intensity);
		color = (gray * (1 - perc)) + (original.rgb * perc);
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
