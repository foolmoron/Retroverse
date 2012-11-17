float width;
float height;
float radius;
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

float4 PixelShaderFunction(PixelShaderInput input) : COLOR0
{
	float3 color;
	float4 original = tex2D(TextureSampler, input.TexCoord);

	float rad2 = pow(radius / zoom, 2);
	float dist2 = pow((input.TexCoord.x - center.x) * width, 2) + pow((input.TexCoord.y - center.y) * height, 2);
	
	if (dist2 < rad2)
	{
		float corner2 = pow(width/2, 2) + pow(height/2, 2) - rad2;		
		float luminance = dot(original.rgb,  float3(weightR, weightG, weightB));
		float3 gray = (luminance > 0.5f) ? (luminance + 1) / 2 : luminance / 2;
		float percorig = (dist2 / rad2) * intensity;
		percorig = percorig * percorig;
		if (percorig > 1)
			percorig = 1;
		if (percorig < 0)
			percorig = 0;
		color = (gray * (percorig)) + (original.rgb * (1-percorig));
	}
	else
	{
		float luminance = dot(original.rgb,  float3(weightR, weightG, weightB));
		float3 gray = (luminance > 0.5f) ? (luminance + 1) / 2 : luminance / 2;
		color = gray;
		//dist2 = dist2 - rad2;
		//float corner2 = pow(width/2, 2) + pow(height/2, 2) - rad2;		
		//float luminance = dot(original.rgb,  float3(weightR, weightG, weightB));
		//float3 gray = (luminance > 0.5f) ? (luminance + 1) / 2 : luminance / 2;
		//float percorig = (dist2 / corner2) * intensity;
		//percorig = percorig * percorig;
		//if (percorig > 1)
		//	percorig = 1;
		//if (percorig < 0)
		//	percorig = 0;
		//color = (gray * (percorig)) + (original.rgb * (1-percorig));
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