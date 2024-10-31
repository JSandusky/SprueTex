#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4 HeatGradient(float value, float minimum, float maximum)
{
    float ratio = 2 * (value - minimum) / (maximum - minimum);
    int b = max(0, (255 * (1 - ratio)));
    int r = max(0, (255 * (ratio - 1)));
    int g = 255 - b - r;
    return float4(r / 255.0f, g / 255.0f, b / 255.0f, 1.0f);
}

matrix WorldViewProjection;
matrix Transform;
int SelectedBone = -2;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
    float4 BoneWeights : BLENDWEIGHT0;
    float4 BoneIndices : BLENDINDICES0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(mul(input.Position, Transform), WorldViewProjection);
    
    if (SelectedBone == input.BoneIndices.x)
        output.Color = HeatGradient(input.BoneWeights.x, 0, 1);
    else if (SelectedBone == input.BoneIndices.y)
        output.Color = HeatGradient(input.BoneWeights.y, 0, 1);
    else if (SelectedBone == input.BoneIndices.z)
        output.Color = HeatGradient(input.BoneWeights.z, 0, 1);
    else if (SelectedBone == input.BoneIndices.w)
        output.Color = HeatGradient(input.BoneWeights.w, 0, 1);
    else
        output.Color = HeatGradient(0, 0, 1);

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	return input.Color;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};