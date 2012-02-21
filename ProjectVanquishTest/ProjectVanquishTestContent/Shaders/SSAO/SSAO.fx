float sampleRadius;
float distanceScale;
float4x4 Projection;
float3 cornerFustrum;

texture depthTexture;
sampler2D depthSampler = sampler_state
{
	Texture = <depthTexture>;
    ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = POINT;
	MINFILTER = POINT;
};

texture randomTexture;
sampler2D RandNormal = sampler_state
{
	Texture = <randomTexture>;
    ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
};

struct VS_OUTPUT
{
    float4 pos				: POSITION;
    float2 TexCoord			: TEXCOORD0;
    float3 viewDirection	: TEXCOORD1;
};

VS_OUTPUT VertexShaderFunction(
    float4 Position : POSITION, float2 TexCoord : TEXCOORD0)
{
    VS_OUTPUT Out = (VS_OUTPUT)0;

    Out.pos = Position;
    Position.xy = sign(Position.xy);
    Out.TexCoord = (float2(Position.x, -Position.y) + float2( 1.0f, 1.0f ) ) * 0.5f;
    float3 corner = float3(-cornerFustrum.x * Position.x, cornerFustrum.y * Position.y, cornerFustrum.z);
	Out.viewDirection =  corner;
    
    return Out;
}

float4 PixelShaderFunction(VS_OUTPUT IN) : COLOR0
{
	float4 samples[8] =
	{
		float4(0.355512, 	-0.709318, 	-0.102371,	0.0 ),
		float4(0.534186, 	0.71511, 	-0.115167,	0.0 ),
		float4(-0.87866, 	0.157139, 	-0.115167,	0.0 ),
		float4(0.140679, 	-0.475516, 	-0.0639818,	0.0 ),
		float4(-0.0796121, 	0.158842, 	-0.677075,	0.0 ),
		float4(-0.0759516, 	-0.101676, 	-0.483625,	0.0 ),
		float4(0.12493, 	-0.0223423,	-0.483625,	0.0 ),
		float4(-0.0720074, 	0.243395, 	-0.967251,	0.0 )
	};
	
	IN.TexCoord.x += 1.0/1600.0;
	IN.TexCoord.y += 1.0/1200.0;

	normalize (IN.viewDirection);
	float depth = tex2D(depthSampler, IN.TexCoord).a;
	float3 se = depth * IN.viewDirection;
	
	float3 randNormal = tex2D( RandNormal, IN.TexCoord * 200.0 ).rgb;

	float3 normal = tex2D(depthSampler, IN.TexCoord).rgb;
	float finalColor = 0.0f;
	
	for (int i = 0; i < 8; i++)
	{
		float3 ray = reflect(samples[i].xyz,randNormal) * sampleRadius;
		
		//if (dot(ray, normal) < 0)
		//	ray += normal * sampleRadius;
			
		float4 sample = float4(se + ray, 1.0f);
		float4 ss = mul(sample, Projection);

		float2 sampleTexCoord = 0.5f * ss.xy/ss.w + float2(0.5f, 0.5f);
		
		sampleTexCoord.x += 1.0/1600.0;
		sampleTexCoord.y += 1.0/1200.0;
		float sampleDepth = tex2D(depthSampler, sampleTexCoord).a;
		
		if (sampleDepth == 1.0)
		{
			finalColor ++;
		}
		else
		{		
			float occlusion = distanceScale* max(sampleDepth - depth, 0.0f);
			finalColor += 1.0f / (1.0f + occlusion * occlusion * 0.1);
		}
	}

	return float4(finalColor/16, finalColor/16, finalColor/16, 1.0f);
}

technique SSAO
{
    pass Pass1
    {          
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader  = compile ps_3_0 PixelShaderFunction();
    }
}