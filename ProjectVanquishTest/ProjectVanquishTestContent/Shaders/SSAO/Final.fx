float2 halfPixel;
sampler Scene : register(s0);
sampler SSAO : register(s1);

struct VertexShaderInput
{
	float3 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	//Initialize Output
	VertexShaderOutput output;

	//Pass Position
	output.Position = float4(input.Position, 1);

	//Pass Texcoord's
	output.UV = input.UV - halfPixel;

	//Return
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	//Sample Scene
	float4 scene = tex2D(Scene, input.UV);

	//Sample SSAO
	float4 ssao = tex2D(SSAO, input.UV);

	//Return
	return (scene * ssao);
}

technique SSAOFinal
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}