sampler TextureSampler { 
	Texture = <xTexture>; 
	magfilter = Point;
	minfilter = Point;
	mipfilter = Point;
	AddressU = Wrap; 
	AddressV = Wrap; 
};


struct ColorVertexToPixel
{
    float4 Position   	: POSITION;    
    float4 Color		: COLOR0;
    float LightingFactor: TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float4 WorldPosition : TEXCOORD2;
};

struct TextureVertexToPixel {
	float4 Position : POSITION;
	float LightingFactor : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float4 WorldPosition : TEXCOORD2;
	float2 TexturePosition : TEXCOORD3;
};

struct PixelToFrame
{
    float4 Color : COLOR0;
};

//------- Constants --------
float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;
float3 xLightDirection;
bool xEnableWorldLighting;
float xAmbient;
bool xEnableLighting;

bool xEnablePointLight;
float3 xLight1Pos;
float xLight1Attenuation;
float xLight1Falloff;
float3 xLight2Pos;
float xLight2Attenuation;
float xLight2Falloff;
float3 xLight3Pos;
float xLight3Attenuation;
float xLight3Falloff;
float3 xLight4Pos;
float xLight4Attenuation;
float xLight4Falloff;
float3 xLight5Pos;
float xLight5Attenuation;
float xLight5Falloff;

Texture xTexture;

bool xXLight;
bool xZLight;
//------- Technique: Colored --------

ColorVertexToPixel ColoredVS( float4 inPos : POSITION, float3 inNormal: NORMAL, float4 inColor: COLOR)
{	
	ColorVertexToPixel Output = (ColorVertexToPixel)0;
	float4x4 preViewProjection = mul (xView, xProjection);
	float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
    
	Output.Position = mul(inPos, preWorldViewProjection);
	Output.WorldPosition = mul(inPos, xWorld);
	Output.Color = inColor;
	
	float3 Normal = normalize(mul(normalize(inNormal), xWorld));	
	Output.Normal = Normal;
    
	return Output;    
}

float CalcLightFactor2(float2 WorldPosition, float2 LightPos, float Attenuation, float Falloff) {
	return 1 - pow(saturate(distance(LightPos, WorldPosition) / Attenuation), Falloff);
}

float CalcLightFactorX(float4 WorldPosition) {
	float LightingFactor = 0;
	LightingFactor += CalcLightFactor2(WorldPosition.xy, xLight1Pos.xy, xLight1Attenuation, xLight1Falloff);
	LightingFactor += CalcLightFactor2(WorldPosition.xy, xLight2Pos.xy, xLight2Attenuation, xLight2Falloff);
	LightingFactor += CalcLightFactor2(WorldPosition.xy, xLight3Pos.xy, xLight3Attenuation, xLight3Falloff);
	LightingFactor += CalcLightFactor2(WorldPosition.xy, xLight4Pos.xy, xLight4Attenuation, xLight4Falloff);
	LightingFactor += CalcLightFactor2(WorldPosition.xy, xLight5Pos.xy, xLight5Attenuation, xLight5Falloff);
	return LightingFactor;
}

float CalcLightFactorZ(float4 WorldPosition) {
	float LightingFactor = 0;
	LightingFactor += CalcLightFactor2(WorldPosition.yz, xLight1Pos.yz, xLight1Attenuation, xLight1Falloff);
	LightingFactor += CalcLightFactor2(WorldPosition.yz, xLight2Pos.yz, xLight2Attenuation, xLight2Falloff);
	LightingFactor += CalcLightFactor2(WorldPosition.yz, xLight3Pos.yz, xLight3Attenuation, xLight3Falloff);
	LightingFactor += CalcLightFactor2(WorldPosition.yz, xLight4Pos.yz, xLight4Attenuation, xLight4Falloff);
	LightingFactor += CalcLightFactor2(WorldPosition.yz, xLight5Pos.yz, xLight5Attenuation, xLight5Falloff);
	return LightingFactor;
}

PixelToFrame ColoredPS(ColorVertexToPixel In) 
{
	PixelToFrame Output = (PixelToFrame)0;		
    
	float LightingFactor = 1;
	if (xEnableLighting) {
		LightingFactor = 0;
		if (xEnableWorldLighting) LightingFactor += dot(In.Normal, -xLightDirection);
		if (xEnablePointLight) {
			//In.WorldPosition.x *= xXLightMul;
			//In.WorldPosition.z *= xZLightMul;
			if (xXLight && xZLight) {
				float xLightFactor = abs(In.Normal.x)<abs(In.Normal.z) ? 1 : 0;
				float zLightFactor = abs(In.Normal.z)<abs(In.Normal.x) ? 1 : 0;
				LightingFactor += xLightFactor * CalcLightFactorX(In.WorldPosition) + zLightFactor * CalcLightFactorZ(In.WorldPosition);
			} else if (xXLight) {
				LightingFactor += CalcLightFactorX(In.WorldPosition);
			} else if (xZLight) {
				LightingFactor += CalcLightFactorZ(In.WorldPosition);
			}
		}
	}

	Output.Color = In.Color;
	Output.Color.rgb *= saturate(LightingFactor) + xAmbient;
	//Output.Color.x = In.Normal.x;
	//Output.Color.z = -In.Normal.x;

	//Output.Color.a = 0;
	//Output.Color.a = LightingFactor;
	//if (LightingFactor < .2) Output.Color.a = (LightingFactor-.1) / .1;
	//if (LightingFactor < .1) Output.Color.a = 0;
	float4 black = (float4)0;
	black.a = 1;
	if (LightingFactor < .2) {
		float4 factor = (float4)0;
		factor.xyzw = (LightingFactor - .1) / .1;
		Output.Color = lerp(black, Output.Color, factor);
	}
	if (LightingFactor < .1) Output.Color = black;

	return Output;
}

TextureVertexToPixel TexturedVS(float4 inPos : POSITION, float3 inNormal : NORMAL, float2 inTex : TEXCOORD0)
{
	TextureVertexToPixel Output = (TextureVertexToPixel)0;
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);

	Output.Position = mul(inPos, preWorldViewProjection);
	Output.WorldPosition = mul(inPos, xWorld);
	Output.TexturePosition = inTex;

	float3 Normal = normalize(mul(normalize(inNormal), xWorld));
	Output.Normal = Normal;

	return Output;
}

PixelToFrame TexturedPS(TextureVertexToPixel In)
{
	PixelToFrame Output = (PixelToFrame)0;

	float LightingFactor = 1;
	if (xEnableLighting) {
		LightingFactor = 0;
		if (xEnableWorldLighting) LightingFactor += dot(In.Normal, -xLightDirection);
		if (xEnablePointLight) {
			//In.WorldPosition.x *= xXLightMul;
			//In.WorldPosition.z *= xZLightMul;
			if (xXLight && xZLight) {
				float xLightFactor = abs(In.Normal.x)<abs(In.Normal.z) ? 1 : 0;
				float zLightFactor = abs(In.Normal.z)<abs(In.Normal.x) ? 1 : 0;
				LightingFactor += xLightFactor * CalcLightFactorX(In.WorldPosition) + zLightFactor * CalcLightFactorZ(In.WorldPosition);
			}
			else if (xXLight) {
				LightingFactor += CalcLightFactorX(In.WorldPosition);
			}
			else if (xZLight) {
				LightingFactor += CalcLightFactorZ(In.WorldPosition);
			}
		}
	}

	Output.Color = tex2D(TextureSampler, In.TexturePosition);
	Output.Color.rgb *= saturate(LightingFactor) + xAmbient;
	//Output.Color.x = In.Normal.x;
	//Output.Color.z = -In.Normal.x;

	//Output.Color.a = 0;
	//Output.Color.a = LightingFactor;
	//if (LightingFactor < .2) Output.Color.a = (LightingFactor - .1) / .1;
	//if (LightingFactor < .1) Output.Color.a = 0;
	float4 black = (float4)0;
	black.a = 1;
	if (LightingFactor < .2) {
		float4 factor = (float4)0;
		factor.xyzw = (LightingFactor - .1) / .1;
		Output.Color = lerp(black, Output.Color, factor);
	}
	if (LightingFactor < .1) Output.Color = black;

	return Output;
}

technique Colored
{
	pass Pass0
	{   
		AlphaBlendEnable = TRUE;
		DestBlend = INVSRCALPHA;
		SrcBlend = SRCALPHA;
		VertexShader = compile vs_4_0 ColoredVS();
		PixelShader  = compile ps_4_0 ColoredPS();
	}
}

technique Textured
{
	pass Pass0
	{
		AlphaBlendEnable = TRUE;
		DestBlend = INVSRCALPHA;
		SrcBlend = SRCALPHA;
		VertexShader = compile vs_4_0 TexturedVS();
		PixelShader  = compile ps_4_0 TexturedPS();
	}
};

technique Player
{
	pass Pass0
	{
		//AlphaBlendEnable = TRUE;
		//DestBlend = INVSRCALPHA;
		//SrcBlend = SRCALPHA;
		VertexShader = compile vs_4_0 ColoredVS();
		PixelShader = compile ps_4_0 ColoredPS();
	}
}