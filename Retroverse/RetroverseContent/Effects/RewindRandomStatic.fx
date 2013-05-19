//Create random lines of TV static 

sampler2D Texture : register(s0);

float randomSeed = 0;
float numLinesOfStatic = 0;
float whiteness = 0.5;

float3 staticPositions = float3(-100,-100,-100);
float3 staticThicknesses = float3(-100,-100,-100);

float4 CreateStaticFunction(float2 inCoord : TEXCOORD0) : COLOR0
{
	float4 original = tex2D(Texture, inCoord);
	float4 ret = float4(0,0,0,0);
	
	[loop]
	for (int i = 0; i < numLinesOfStatic; i++)
	{
		float position = staticPositions[i];
		float thickness = staticThicknesses[i];
        float distFromBase = abs(position - inCoord.y);
		float distFromBasePerc = distFromBase / (thickness/2);
        if (distFromBasePerc <= 1)
		{
			float randBasedOnPixelPosition = dot(inCoord, float2(12.9898,78.233) * randomSeed);
			float largeNumber = 43758.5453;
			float n = frac(randBasedOnPixelPosition * largeNumber * distFromBase);
			float finalnoise = abs(n) * whiteness;
			ret = float4(finalnoise, finalnoise, finalnoise, 1);
        }		
	}
	return ret;
}

float fadeIntensity;

float4 FadeFunction(float2 inCoord : TEXCOORD0) : COLOR0
{	
	float4 original = tex2D(Texture, inCoord);

	float4 ret = float4(0,0,0,0);
	float minDistPerc = 1;
	bool inStatic= false;

	[loop]
	for (int i = 0; i < 3; i++)
	{
        float position = staticPositions[i];
        float thickness = staticThicknesses[i]/2;
        float distFromBase = abs(position - inCoord.y);
        if (distFromBase <= thickness/2)
		{
			inStatic = true;
            float distPerc = distFromBase / (thickness/2);
			if (distPerc < minDistPerc)
			{
				minDistPerc = distPerc;
				float fadeintensity = 1/fadeIntensity;
				float fadingFactor = pow(distPerc, fadeintensity);
				ret = lerp(original, float4(0,0,0,0), fadingFactor);
			}
        }
	}

	if (inStatic)
		return ret;
	else
		return original;
}

technique CreateStatic
{
    pass main
    {
		PixelShader = compile ps_2_0 CreateStaticFunction();
    }
}

technique Fade
{
    pass main
    {
		PixelShader = compile ps_2_0 FadeFunction();
    }
}