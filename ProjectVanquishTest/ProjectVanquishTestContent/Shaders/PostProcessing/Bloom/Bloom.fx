sampler TextureSampler : register(s0);

float Threshold = 0.3;

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 Color = tex2D(TextureSampler, texCoord);
    
    // Get the bright areas that is brighter than Threshold and return it.
    return saturate((Color - Threshold) / (1 - Threshold));
}

technique Bloom
{
    pass Pass1
    {
		PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
