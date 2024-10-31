#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

const float smoothing = 1.0/16.0;

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float distance = tex2D(SpriteTextureSampler,input.TextureCoordinates).a;
    float alpha = smoothstep(0.5 - smoothing, 0.5 + smoothing, distance);
    return float4(input.Color.rgb, input.Color.a * alpha);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};