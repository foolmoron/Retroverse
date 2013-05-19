//VCR rewind effect

sampler2D Texture : register(s0);

float randomSeed = 0;
float numLinesOfStatic = 0;
float rot = 0;

float4 staticpositions = float4(-100,-100,-100,-100);
float4 staticthicknesses = float4(-100,-100,-100,-100);

float4 CreateStatic(float2 inCoord : TEXCOORD0) : COLOR0
{
	float4 original = tex2D(Texture, inCoord);
	
	[loop]
	for (int i = 0; i < 4; i++)
	{
		float position = staticpositions[i];
		float thickness = staticthicknesses[i]/2;
        float distFromBase = abs(position - inCoord.y);
        if (distFromBase <= thickness/2)
		{
            float distPerc = distFromBase / (thickness/2);
			float2 n = frac(dot(inCoord, float2(12.9898,78.233) * randomSeed) * 43758.5453 * distFromBase);
			float finalnoise = abs(n.x + n.y) * 0.5;
			float biasToTrans = pow(distPerc, 2);
            float rand = (1-biasToTrans) * finalnoise;
            return float4(rand, rand, rand, 1);
        }		
	}
	return float4(0,0,0,0);
}

technique Static
{
    pass CreateStatic
    {
		PixelShader = compile ps_2_0 CreateStatic();
    }
}