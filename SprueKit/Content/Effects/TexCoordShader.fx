#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D UVChartTex;
sampler2D UVChartSampler = sampler_state
{
    Texture = <UVChartTex>;
    AddressU = Wrap;
    AddressV = Wrap;
};

matrix WorldViewProjection;
matrix Transform;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 UVCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float2 UVCoord : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(mul(input.Position, Transform), WorldViewProjection);
	output.UVCoord = input.UVCoord;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	return tex2D(UVChartSampler, input.UVCoord * 2);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};