#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

float2 UVTiling = float2(1,1);
float3 CameraPosition;
float3 LightDir = float3(0, -1, 0);
matrix WorldViewProjection;
matrix Transform;
float AmbientBrightness;
int FlipCulling = -1;

#include "PBR.inc"
#include "NoTanNormals.inc"

Texture2D DiffuseTex;
Texture2D NormalMapTex;
Texture2D RoughnessTex;
Texture2D MetalnessTex;
Texture2D HeightMapTex;
Texture2D AOTex;
Texture2D SubsurfaceColorTex;
Texture2D SubsurfaceDepthTex;
Texture2D EmissiveMaskTex;

Texture2D IBLLUTTex;

TextureCube IBLTex;

sampler2D DiffuseSampler = sampler_state
{
    Texture = <DiffuseTex>;
    AddressU = Wrap;
    AddressV = Wrap;
};

sampler2D NormalMapSampler = sampler_state
{
    Texture = <NormalMapTex>;
    AddressU = Wrap;
    AddressV = Wrap;
};

sampler2D RoughnessSampler = sampler_state
{
    Texture = <RoughnessTex>;
    AddressU = Wrap;
    AddressV = Wrap;
};

sampler2D MetalnessSampler = sampler_state
{
    Texture = <MetalnessTex>;
    AddressU = Wrap;
    AddressV = Wrap;
};

sampler2D HeightMapSampler = sampler_state
{
    Texture = <HeightMapTex>;
    AddressU = Wrap;
    AddressV = Wrap;
};

sampler2D AOSampler = sampler_state
{
    Texture = <AOTex>;
    AddressU = Wrap;
    AddressV = Wrap;
};

sampler2D EmissiveMaskSampler = sampler_state
{
    Texture = <EmissiveMaskTex>;
    AddressU = Wrap;
    AddressV = Wrap;
};

sampler2D SSColorSampler = sampler_state
{
    Texture = <SubsurfaceColorTex>;
    AddressU = Wrap;
    AddressV = Wrap;
};

sampler2D SSDepthSampler = sampler_state
{
    Texture = <SubsurfaceDepthTex>;
    AddressU = Wrap;
    AddressV = Wrap;
};

sampler2D IBLLUTSampler = sampler_state
{
    Texture = <IBLLUTTex>;
    AddressU = Clamp;
    AddressV = Clamp;
};
samplerCUBE IBLSampler = sampler_state 
{
    Texture = <IBLTex>;
};

#include "Disp.inc"

struct VertexShaderInput
{
	float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float4 Tangent : TANGENT0;
	float2 UVCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float3 Normal : NORMAL0;
	float2 SampleUV : TEXCOORD0;
    float3 WorldPos : TEXCOORD1;
    float3 Tangent : TANGENT0;
    float3 Bitangent : BITANGENT0;
};

#include "IBL.inc"

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(mul(input.Position, Transform), WorldViewProjection);
    output.WorldPos = input.Position; //mul(input.Position, Transform).xyz;
    output.Normal = normalize(mul(input.Normal, (float3x3)Transform));
    
    float3 tangent = normalize(mul(input.Tangent.xyz, (float3x3)Transform));
    float3 bitangent = cross(tangent.xyz, output.Normal.xyz).xyz * input.Tangent.w;
    output.Tangent = normalize(tangent);
    output.Bitangent = normalize(bitangent);
	output.SampleUV = input.UVCoord * UVTiling;

	return output;
}

float GetAtten(float3 normal, float3 worldPos)
{
    return saturate(dot(normal, -LightDir));
}

float4 NormalToColor(float3 val)
{
    return float4(val * 0.5 + 0.5, 1.0);
}

float4 PBRCommon(VertexShaderOutput input, float4 diffuse, float3 specColor, float roughness)
{
    float3 normVal = tex2D(NormalMapSampler, input.SampleUV).xyz * 2.0 - 1.0;
    float3 eyeToPixel = normalize(input.WorldPos - CameraPosition);
    
    const float3x3 tbn = transpose(float3x3(input.Tangent, input.Bitangent * FlipCulling, input.Normal));
    float3 norm = normalize(mul(tbn, normVal).xyz);
    //float3 norm = perturb_normal(input.Normal, eyeToPixel, NormalMapSampler, input.SampleUV);
    
    float atten = GetAtten(norm, input.WorldPos);
    float4 oColor = float4(GetBRDF(input.WorldPos, -LightDir, normalize(-LightDir), -eyeToPixel, norm, roughness, diffuse.rgb, specColor), diffuse.a);
    
    float3 reflectVec = normalize(reflect(eyeToPixel, norm));
    float3 reflectionCubeColor = float3(0.25, 0.25, 0.25);
    float3 ibl = ImageBasedLighting(reflectVec, norm, -eyeToPixel, diffuse.rgb, specColor, roughness, reflectionCubeColor);
    
    float AOValue = tex2D(AOSampler, input.SampleUV).x;
    AOValue = AOValue < 0.001 ? 1.0 : AOValue;
    float emissive = max(tex2D(EmissiveMaskSampler, input.SampleUV).x - 0.2, 0.0);
    
    float ssDepth  =tex2D(SSDepthSampler, input.SampleUV).x;
    if (ssDepth > 0)
    {
        float4 ssColor = tex2D(SSColorSampler, input.SampleUV);
        float ssWrap = (1.0 - ssDepth);
        float scatterWidth = 0.5;
        
        float3 H = normalize(-LightDir + norm * scatterWidth);
        float VdotH = pow(saturate(dot(eyeToPixel, H)), 1.3);
        float3 I = VdotH  * ssDepth;
        float3 ssContrib = ssColor * I * ssColor.a;

        //float vdh = dot(norm, -LightDir) * 2 - 1;
        //float ndh = dot(eyeToPixel, -LightDir) * 2 - 1;
        //float ndlWrap = (vdh + ssWrap) / (1 + ssWrap);
        //float scatter = smoothstep(0.0, scatterWidth, ndlWrap) * smoothstep(scatterWidth * 2, scatterWidth, ndlWrap);
        
        //float3 ssContrib = ssColor * scatter * ssColor.a;
        oColor.rgb += ssContrib;
        // SS is emissive in nature, thus it fights off IBL
        ibl.rgb = max(ibl.rgb - ssContrib, 0);
        //ibl -= ssContrib*0.5;
    }
    
    /// ambient term + direct-light + ibl
    return 
        diffuse * emissive * AOValue +
            //float4(
            //(AmbientBrightness + emissive) * AOValue,
            //(AmbientBrightness + emissive) * AOValue,
            //(AmbientBrightness + emissive) * AOValue,
            //1) + 
        (oColor + float4(ibl * AOValue /** (1.0f - AmbientBrightness * 0.5 - emissive)*/, diffuse.a));
}

float4 RoughMetalPS(VertexShaderOutput input) : COLOR
{
	float4 diffuse =    tex2D(DiffuseSampler, input.SampleUV);
    float roughness =   tex2D(RoughnessSampler, input.SampleUV).r;
    roughness *= roughness;
    roughness = max(roughness, 0.01);
    float metalness =  tex2D(MetalnessSampler, input.SampleUV).r;
    float3 specColor = GetMetalnessSpecular(diffuse, metalness);
    diffuse = GetMetalnessDiffuse(diffuse, metalness, roughness);
    
    return PBRCommon(input, diffuse, specColor, roughness);
}

float4 RoughSpecularPS(VertexShaderOutput input) : COLOR
{
    float4 diffuse =    tex2D(DiffuseSampler, input.SampleUV);
    float roughness =   tex2D(RoughnessSampler, input.SampleUV).r;
    roughness *= roughness;
    roughness = max(roughness, 0.01);
    float3 specColor = tex2D(MetalnessSampler, input.SampleUV).rgb;
    
    return PBRCommon(input, diffuse, specColor, roughness);
}

float4 GlossyMetalPS(VertexShaderOutput input) : COLOR
{
    float4 diffuse =    tex2D(DiffuseSampler, input.SampleUV);
    float roughness =   tex2D(RoughnessSampler, input.SampleUV).r;
    roughness = 1.0 - roughness;
    roughness *= roughness;
    roughness = max(roughness, 0.01);
    float3 metalness =  tex2D(MetalnessSampler, input.SampleUV).r;
    float3 specColor = GetMetalnessSpecular(diffuse, metalness.x);
    diffuse = GetMetalnessDiffuse(diffuse, metalness.x, roughness);
    
    return PBRCommon(input, diffuse, specColor, roughness);
}

float4 GlossySpecularPS(VertexShaderOutput input) : COLOR
{
    float height =      tex2D(HeightMapSampler, input.SampleUV).r;
    float4 diffuse =    tex2D(DiffuseSampler, input.SampleUV);
    float roughness =   tex2D(RoughnessSampler, input.SampleUV).r;
    roughness = 1.0 - roughness;
    roughness *= roughness;
    roughness = max(roughness, 0.01);
    float3 specColor = tex2D(MetalnessSampler, input.SampleUV).rgb;
    
    return PBRCommon(input, diffuse, specColor, roughness);
}

float2 DoHeightMap(VertexShaderOutput input)
{
    //float3 normVal = tex2D(NormalMapSampler, input.SampleUV).xyz * 2.0 - 1.0;
    //const float3x3 tbn = transpose(float3x3(normalize(input.Tangent), normalize(-input.Bitangent), normalize(input.Normal)));
    //float3 norm = normalize(mul(tbn, normVal).xyz);

    float3 pixelToEye = normalize(CameraPosition - input.WorldPos);
    return updateUV(pixelToEye, input.Normal, normalize(input.Tangent), normalize(input.Bitangent), 0.4, input.SampleUV);
}

float4 RoughMetalHeightPS(VertexShaderOutput input) : COLOR
{
    float height =      tex2D(HeightMapSampler, input.SampleUV).r;
    input.SampleUV = DoHeightMap(input);
    return RoughMetalPS(input);
}

float4 RoughSpecularHeightPS(VertexShaderOutput input) : COLOR
{
    float height =      tex2D(HeightMapSampler, input.SampleUV).r;
    input.SampleUV = DoHeightMap(input);
    return RoughSpecularPS(input);
}

float4 GlossyMetalHeightPS(VertexShaderOutput input) : COLOR
{
    float height =      tex2D(HeightMapSampler, input.SampleUV).r;
    input.SampleUV = DoHeightMap(input);
    return GlossyMetalPS(input);
}

float4 GlossySpecularHeightPS(VertexShaderOutput input) : COLOR
{
    float height =      tex2D(HeightMapSampler, input.SampleUV).r;
    input.SampleUV = DoHeightMap(input);
    return GlossySpecularPS(input);
}

technique PBRRoughMetal
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL RoughMetalPS();
	}
};

technique PBRRoughSpecular
{
    pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL RoughSpecularPS();
	}  
};

technique PBRGlossyMetal
{
    pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL GlossyMetalPS();
	}  
};

technique PBRGlossySpecular
{
    pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL GlossySpecularPS();
	}  
};

// Displacement mapping versions

technique PBRRoughMetalHeight
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL RoughMetalHeightPS();
	}
};

technique PBRRoughSpecularHeight
{
    pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL RoughSpecularHeightPS();
	}  
};

technique PBRGlossyMetalHeight
{
    pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL GlossyMetalHeightPS();
	}  
};

technique PBRGlossySpecularHeight
{
    pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL GlossySpecularHeightPS();
	}  
};