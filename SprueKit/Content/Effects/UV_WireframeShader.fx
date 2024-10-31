#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection; // view * projection
matrix Transform; // always identify thus far
float4 OffsetScale; //unused presently

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 UVCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
    output.Position = mul(float4(input.UVCoord.x + OffsetScale.x, input.UVCoord.y + OffsetScale.y, 0, 1), WorldViewProjection);
	return output;
}

float4 MainPS(in VertexShaderOutput input) : COLOR
{
	return float4(1.0, 1.0, 1.0, 1.0);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};