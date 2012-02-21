// Direction of the light
float3 lightDirection;
// Color of the light
float3 Color;
// Position of the camera, for specular light
float3 cameraPosition;
// This is used to compute the world-position
float4x4 InvertViewProjection;
// Diffuse color, and SpecularIntensity in the alpha channel
texture colorMap;
// Normals, and SpecularPower in the alpha channel
texture normalMap;
// Depth
texture depthMap;
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
 
sampler depthSampler = sampler_state
{
    Texture = (depthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};
 
sampler normalSampler = sampler_state
{
    Texture = (normalMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
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
    // Align texture coordinates
    output.TexCoord = input.TexCoord - halfPixel;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // Get normal data from the normalMap
    float4 normalData = tex2D(normalSampler,input.TexCoord);
    // Transform normal back into [-1,1] range
    float3 normal = 2.0f * normalData.xyz - 1.0f;
    // Get specular power, and get it into [0,255] range]
    float specularPower = normalData.a * 255;
    // Get specular intensity from the colorMap
    float specularIntensity = tex2D(colorSampler, input.TexCoord).a;
 
	// Read depth
    float depthVal = tex2D(depthSampler,input.TexCoord).r;
 
	// Compute screen-space position
    float4 position;
    position.x = input.TexCoord.x * 2.0f - 1.0f;
    position.y = -(input.TexCoord.x * 2.0f - 1.0f);
    position.z = depthVal;
    position.w = 1.0f;
    // Transform to world space
    position = mul(position, InvertViewProjection);
    position /= position.w;
 
	// Surface-to-light vector
    float3 lightVector = -normalize(lightDirection);
 
	// Compute diffuse light
    float NdL = max(0,dot(normal,lightVector));
    float3 diffuseLight = NdL * Color.rgb;
 
	// Reflection vector
    float3 reflectionVector = normalize(reflect(-lightVector, normal));
    // Camera-to-surface vector
    float3 directionToCamera = normalize(cameraPosition - position);
    // Compute specular light
    float specularLight = specularIntensity * pow( saturate(dot(reflectionVector, directionToCamera)), specularPower);
 
	// Output the two lights
    return float4(diffuseLight.rgb, specularLight) ;
}
 
technique DirectionalLight
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}