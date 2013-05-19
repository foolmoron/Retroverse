//Create random lines of TV static 

sampler2D Texture : register(s0);
texture2D AlphaTexture;
sampler2D AlphaSampler = sampler_state
{
	Texture = <AlphaTexture>;
};

float randomSeed = 0;
float whiteness = 0.5;
bool premultiply = false;

float4 CreateStaticFunction(float2 inCoord : TEXCOORD0) : COLOR0
{
	float4 original = tex2D(Texture, inCoord);
	float alpha = tex2D(AlphaSampler, inCoord).a;

	float position = 0.5;
	float thickness = 1;
    float distFromBase = abs(position - inCoord.y);
	float distFromBasePerc = distFromBase / (thickness/2);
	float randBasedOnPixelPosition = dot(inCoord, float2(12.9898,78.233) * randomSeed);
	float largeNumber = 43758.5453;
	float n = frac(randBasedOnPixelPosition * largeNumber * distFromBase);
	float finalnoise = abs(n) * whiteness;
	if (premultiply)
		return float4(finalnoise * alpha, finalnoise * alpha, finalnoise * alpha, alpha);
	else
		return float4(finalnoise, finalnoise, finalnoise, alpha);
}

technique CreateStatic
{
    pass main
    {
		PixelShader = compile ps_2_0 CreateStaticFunction();
    }
}