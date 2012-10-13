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
	float luminance = dot(original.rgb,  float3(0.30f, 0.59f, 0.11f));
	if(original.r > (original.g + 0.2f) && original.r > (original.b + 0.05f))
	{
	color.rgb = float3(1, 0, 0);
	}
	else if(original.g > (original.r + 0.2f) && original.g > (original.b + 0.05f))
	{
	color.rgb = float3(0, 1, 0);
	} 
	else if(original.b > (original.g + 0.2f) && original.b > (original.r + 0.05f))
	{
	color.rgb = float3(0, 0, 1);
	}
	else
	{
	color.rgb = (luminance > 0.5f) ? 1.0f : 0.0f;
	}

	return float4(color, original.a);
}

technique Technique1
{
    pass BWTransform
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
