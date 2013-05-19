float time;

texture2D Texture;
sampler2D TextureSampler = sampler_state
{
	Texture = <Texture>;
};

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float4 original = tex2D(TextureSampler, coords);

    float x = coords.x * coords.y * (time * 100);
    x = fmod(x, 13) * fmod(x, 123);
    float dx = fmod(x, 0.01);

    float3 cResult = original.rgb + original.rgb * saturate(0.1f + dx.xxx * 100);
    float2 sc;
    sincos(coords.y * 2048, sc.x, sc.y);

    cResult += original.rgb * float3(sc.x, sc.y, sc.x) * 1024;

	return lerp(original, float4(cResult, 1), 0.5);
}


technique Technique1
{
    pass Pass1
	{
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }

}