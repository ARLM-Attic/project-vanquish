float4x4 World;
float4x4 View;
float4x4 Projection;
float SpecularIntensity = 0.8f;
float SpecularPower = 0.5f;

// Define the Color Texture
texture Texture;
sampler diffuseSampler = sampler_state
{
    Texture = (Texture);
    MAGFILTER = LINEAR;
    MINFILTER = LINEAR;
    MIPFILTER = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

// Define the Normal Map Texture
texture NormalMap;
sampler normalSampler = sampler_state
{
    Texture = (NormalMap);
    MAGFILTER = LINEAR;
    MINFILTER = LINEAR;
    MIPFILTER = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

// Define the Specular Map Texture
texture SpecularMap;
sampler specularSampler = sampler_state
{
    Texture = (SpecularMap);
    MAGFILTER = LINEAR;
    MINFILTER = LINEAR;
    MIPFILTER = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
    float3 Binormal : BINORMAL0;
    float3 Tangent : TANGENT0;
    // We will add additional variables when we add Skinned models
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float2 Depth : TEXCOORD1;
    float3x3 tangentToWorld : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(float4(input.Position.xyz,1), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.TexCoord = input.TexCoord;
    output.Depth.x = output.Position.z;
    output.Depth.y = output.Position.w;

    // Calculate tangent space to world space matrix using the world space tangent,
    // binormal, and normal as basis vectors
    output.tangentToWorld[0] = mul(input.Tangent, World);
    output.tangentToWorld[1] = mul(input.Binormal, World);
    output.tangentToWorld[2] = mul(input.Normal, World);

    return output;
}

struct PixelShaderOutput
{
    half4 Color : COLOR0;
    half4 Normal : COLOR1;
    half4 Depth : COLOR2;
};

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
    PixelShaderOutput output;
    output.Color = tex2D(diffuseSampler, input.TexCoord);

    float4 specularAttributes = tex2D(specularSampler, input.TexCoord);
    // Specular Intensity
    output.Color.a = specularAttributes.r;

    // Read the normal from the normal map
    float3 normalFromMap = tex2D(normalSampler, input.TexCoord);
    // Transform to [-1,1]
    normalFromMap = 2.0f * normalFromMap - 1.0f;
    // Transform into world space
    normalFromMap = mul(normalFromMap, input.tangentToWorld);
    // Normalize the result
    normalFromMap = normalize(normalFromMap);
    // Output the normal, in [0,1] space
    output.Normal.rgb = 0.5f * (normalFromMap + 1.0f);

    // Specular Power
    output.Normal.a = specularAttributes.a;
    output.Depth = input.Depth.x / input.Depth.y;

    return output;
}

technique RenderGBuffer
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}