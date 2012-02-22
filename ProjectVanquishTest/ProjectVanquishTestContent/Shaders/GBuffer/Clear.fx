struct VertexShaderInput
{
    float3 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position,1);
    return output;
}

struct PixelShaderOutput
{
    float4 Color : COLOR0;
    float4 Normal : COLOR1;
    float4 Depth : COLOR2;
};

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
    PixelShaderOutput output;

    // Black color
    output.Color = 0.0f;
    output.Color.a = 0.0f;

    // When transforming 0.5f into [-1,1], we will get 0.0f
    output.Normal.rgb = 0.5f;

    // No specular power
    output.Normal.a = 0.0f;

    // Max depth
    output.Depth = 1.0f;

    return output;
}

technique ClearGBuffer
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}