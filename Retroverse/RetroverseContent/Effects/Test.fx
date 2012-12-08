
texture2D Texture;
sampler2D TextureSampler = sampler_state
{
    Texture = <Texture>;
	MinFilter = Anisotropic; // Minification Filter
	MagFilter = Anisotropic; // Magnification Filter
    MipFilter = Linear; // Mip-mapping
    AddressU = Wrap; // Address Mode for U Coordinates
    AddressV = Wrap; // Address Mode for V Coordinates
};

struct PixelShaderInput
{
    float2 TexCoord : TEXCOORD0;
};

float4 PixelShaderFunction(PixelShaderInput input) : COLOR0
{
	return tex2D(TextureSampler, input.TexCoord);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
