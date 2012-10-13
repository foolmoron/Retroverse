
texture2D Texture;
sampler2D TextureSampler = sampler_state
{
    Texture = <Texture>;
};

struct PixelShaderInput
{
    float2 TexCoord : TEXCOORD0;
};

float4 PixelShaderFunction(PixelShaderInput input) : COLOR0
{
	return tex2D(TextureSampler, input.TexCoord)/2;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
