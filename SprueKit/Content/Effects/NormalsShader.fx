#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection;
matrix Transform;
float3 CamForward;
float3 CamUp;
float3 CamRight;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Normal : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(mul(input.Position, Transform), WorldViewProjection);
    output.Normal = input.Normal;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float x = dot(input.Normal.xyz, CamRight);
    float y = dot(input.Normal.xyz, CamUp);
    float z = 1.0;
    
    return float4(normalize(float3(x,y,z)) * 0.5 + 0.5, 1.0);

	//return float4((input.Normal.xyz * 0.5) + 0.5, 1.0);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};