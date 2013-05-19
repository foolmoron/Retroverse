//Create random lines of TV static 

sampler2D Texture : register(s0);

float4 unchargedColor;
float4 chargedColor;
float shadingPercentage;

float4 StoreIconShadeFunction(float2 inCoord : TEXCOORD0) : COLOR0
{
	float4 original = tex2D(Texture, inCoord);
	if (original.a == 0)
		return original;

	float4 tintColor;
	if ((1 - inCoord.y) >= shadingPercentage)
		tintColor = unchargedColor;
	else
		tintColor = chargedColor;
	
	float4 result = original * tintColor; 
	return result;
	//return float4(finalnoise * alpha, finalnoise * alpha, finalnoise * alpha, alpha);
}

technique StoreIconShading
{
    pass main
    {
		PixelShader = compile ps_2_0 StoreIconShadeFunction();
    }
}