float2 blurDirection;
float2 targetSize;
sampler GBuffer1 : register(s1);
sampler GBuffer2 : register(s2);
sampler SSAO : register(s3);

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
	VertexShaderOutput output;

	// Pass Position
	output.Position = float4(input.Position, 1);

	// Pass UV
	output.UV = input.UV - float2(1.0f / targetSize.xy);
    
	return output;
}

// Manual Linear Sample
float4 manualSample(sampler Sampler, float2 UV, float2 textureSize)
{
	float2 texelpos = textureSize * UV; 
	float2 lerps = frac(texelpos); 
	float texelSize = 1.0 / textureSize;                 
 
	float4 sourcevals[4]; 
	sourcevals[0] = tex2D(Sampler, UV); 
	sourcevals[1] = tex2D(Sampler, UV + float2(texelSize, 0)); 
	sourcevals[2] = tex2D(Sampler, UV + float2(0, texelSize)); 
	sourcevals[3] = tex2D(Sampler, UV + float2(texelSize, texelSize));   
         
	float4 interpolated = lerp(lerp(sourcevals[0], sourcevals[1], lerps.x), lerp(sourcevals[2], sourcevals[3], lerps.x ), lerps.y); 

	return interpolated;
}

// Normal Decoding Function
float3 decode(float3 enc)
{
	return (2.0f * enc.xyz- 1.0f);
}

float4 PixelShaderFunction(float2 UV :TEXCOORD0) : COLOR0
{
	// Sample Depth
	float depth = manualSample(GBuffer2, UV, targetSize).y;
    
	// Sample Normal
	float3 normal = decode(tex2D(GBuffer1, UV).xyz);
    
	// Sample SSAO
	float ssao = tex2D(SSAO, UV).x;
   
	// Color Normalizer
    float ssaoNormalizer = 1;

	// Blur Samples to be done
    int blurSamples = 8; 
	
	// From the negative half of blurSamples to the positive half; almost like gaussian blur
	for(int i = -blurSamples / 2; i <= blurSamples / 2; i++)
	{
		// Calculate newUV as the current UV offset by the current sample
		float2 newUV = float2(UV.xy + i * blurDirection.xy);
		
		// Sample SSAO
		float sample = manualSample(SSAO, newUV, targetSize).y;
		
		// Sample Normal
		float3 samplenormal = decode(tex2D(GBuffer1, newUV).xyz);
			
		// Check Angle Between SampleNormal and Normal
		if (dot(samplenormal, normal) > 0.99)	
		{
			// Calculate this samples contribution
			float contribution = blurSamples / 2 - abs(i);
			
			// Accumulate to normalizer
			ssaoNormalizer += (blurSamples / 2 - abs(i));

			// Accumulate to SSAO
			ssao += sample * contribution;
		}
	}

	// Return Averaged Samples
	return ssao / ssaoNormalizer;
}

technique SSAOBlur
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}