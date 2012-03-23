float4x4	matWorldViewProj;
float4x4	matInverseWorld;
float4		vLightDirection;

// Set the direction to the sky
float3 SkyDirection = float3(0.0f,1.0f,0.0f);

// Set ground color
float4 GroundColor = float4(0.5f,1.0f,0.5f,1.0f);

// Set sky color
float4 SkyColor = float4(0.5f,0.5f,1.0f,1.0f);

// Set the intensity of the hemisphere color
float LightIntensity = 0.7f;
	
texture ColorMap;
sampler ColorMapSampler = sampler_state
{
   Texture = <ColorMap>;
   MinFilter = Linear;
   MagFilter = Linear;
   MipFilter = Linear;   
   AddressU  = Clamp;
   AddressV  = Clamp;
};

struct VertexShaderOutput
{
	float4 Position	: POSITION;
	float2 TexCoord	: TEXCOORD0;
	float3 Light	: TEXCOORD1;
	float3 Normal	: TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction( float4 Position : POSITION, float2 TexCoord : TEXCOORD, float3 Normal : NORMAL )
{
	VertexShaderOutput Out = (VertexShaderOutput)0;
	Out.Position = mul(Position, matWorldViewProj);
	Out.TexCoord = TexCoord;
	Out.Light = normalize(vLightDirection);
	Out.Normal = normalize(mul(matInverseWorld, Normal));
	
	return Out;
}

float4 PixelShaderFunction(float2 TexCoord : TEXCOORD0,float3 Light : TEXCOORD1, float3 Normal : TEXCOORD2) : COLOR
{
	// Calculate normal diffuse light.
	float4 Color = tex2D(ColorMapSampler, TexCoord);	
	float Ai = 0.7f;
	float4 Ac = float4(1.0, 1.0, 1.0, 1.0);
	float Di = 1.0f;
	float4 Dc = float4(1.0, 1.0, 1.0, 1.0);
	float Dd = saturate(dot(Light,Normal));
	
	float vecHemi = (dot(Normal, SkyDirection) * 0.5f ) + 0.5f;
	float4 HemiFinal = LightIntensity * lerp(GroundColor, SkyColor, vecHemi);

	return saturate((HemiFinal*Color)+(Color*Di*Dd));
}

technique HemisphericLight
{
	pass Pass1
	{
		Sampler[0] = (ColorMapSampler);	
		
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}