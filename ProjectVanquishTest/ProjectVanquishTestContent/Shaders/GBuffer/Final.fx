texture colorMap;
texture lightMap;
texture shadowMap;
float2 halfPixel;

sampler colorSampler = sampler_state
{
    Texture = (colorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

sampler lightSampler = sampler_state
{
    Texture = (lightMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

sampler shadowSampler = sampler_state
{
	Texture = (shadowMap);
	MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

struct VertexShaderInput
{
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position,1);
    output.TexCoord = input.TexCoord - halfPixel;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float shadowTerm = tex2D(shadowSampler,input.TexCoord).r;
    float3 diffuseColor = tex2D(colorSampler,input.TexCoord).rgb;
    float4 light = shadowTerm * tex2D(lightSampler,input.TexCoord);
    float3 diffuseLight = light.rgb;
    float specularLight = light.a;
    //return float4(shadowTerm * (diffuseColor * diffuseLight + specularLight),1);
	return float4((diffuseColor * diffuseLight + specularLight),1);
}

technique CombineFinal
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}