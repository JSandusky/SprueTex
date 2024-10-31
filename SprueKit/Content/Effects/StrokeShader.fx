#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

matrix WorldViewProjection;
int u_scale = 1;

struct VertexShaderInput
{
	float4 Position : POSITION0;
    float3 StartPos : POSITION1;
    float3 EndPos : POSITION2;
    float2 StartRadius : TEXCOORD0;
    float2 EndRadius : TEXCOORD1;
	float4 BrushColor : COLOR0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	
    float3 RawPos : POSITION1;
    float3 StartPos : TEXCOORD0;
    float3 EndPos : TEXCOORD1;
    float2 StartRadius : TEXCOORD2;
    float2 EndRadius : TEXCOORD3;
    float4 BrushColor : COLOR0;
};

float4 u_eraser_color = float4(23/255.0,34/255.0,45/255.0,56/255.0);

bool brush_is_eraser(float4 brushColor)
{
    bool is_eraser = false;
    // Constant k_eraser_color defined in canvas.cc
    if (length(brushColor - u_eraser_color) == 0)
        is_eraser = true;
    return is_eraser;
}

float3 closest_point_in_segment(float2 a, float2 b, float2 ab, float ab_magnitude_squared, float2 pt)
{
    float3 result;
    float mag_ab = sqrt(ab_magnitude_squared);
    float d_x = ab.x / mag_ab;
    float d_y = ab.y / mag_ab;
    float ax_x = float(pt.x - a.x);
    float ax_y = float(pt.y - a.y);
    float disc = d_x * ax_x + d_y * ax_y;

    disc = clamp(disc, 0.0, mag_ab);
    result = float3(a.x + disc * d_x, a.y + disc * d_y, disc / mag_ab);
    return result;
}

float sample_stroke(float2 pt, float3 a, float3 b, float radius)
{
    //float dist = float(1<<20);
    float dist = 1048576;
    // Check against a circle of pressure*brush_size at each point, which is cheap.
    float dist_a = distance(pt, a.xy);
    float dist_b = distance(pt, b.xy);
    //float radius_a = radius;//float(a.z*radius);
    //float radius_b = radius;//float(b.z*radius);
    //if ( dist_a < radius_a || dist_b < radius_b ) {
    //    dist = min(dist_a - radius_a, dist_b - radius_b);
    //}
    //// If it's not inside the circle, it might be somewhere else in the stroke.
    //else {
        float2 ab = b.xy - a.xy;
        float ab_magnitude_squared = ab.x*ab.x + ab.y*ab.y;
    
        if ( ab_magnitude_squared > 0.0 ) {
            float3 stroke_point = closest_point_in_segment(a.xy, b.xy, ab, ab_magnitude_squared, pt);
            // z coordinate of a and b has pressure values.
            // z coordinate of stroke_point has interpolation between them for closes point.
            float pressure = lerp(a.z, b.z, stroke_point.z);
    
            dist = radius - distance(stroke_point.xy, pt);
        }
    //}
    //dist = min(radius - dist_a, radius - dist_b);
    return dist;
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(input.Position, WorldViewProjection);
    output.RawPos = input.Position;
    output.StartPos = input.StartPos;
    output.EndPos = input.EndPos;
    
    output.StartRadius = input.StartRadius;
    output.EndRadius = input.EndRadius;
    
	output.BrushColor = input.BrushColor;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 screen_point = input.RawPos.xy;
    
    float dist = sample_stroke(screen_point, input.StartPos, input.EndPos, input.StartRadius);
    
    //if ( dist > 0 ) {
    //    // TODO: is there a way to do front-to-back rendering with a working eraser?
    //    if ( brush_is_eraser(input.BrushColor) ) {
    //        //#if HAS_TEXTURE_MULTISAMPLE
    //        //    float4 eraser_color = texelFetch(u_canvas, ivec2(gl_FragCoord.xy), gl_SampleID);
    //        //#else
    //            //float2 coord = screen_point.xy / u_screen_size;
    //            //float4 eraser_color = texture(u_canvas, coord);
    //        //#endif
    //        return input.BrushColor;
    //    }
    //    else {
    //        return float4(input.BrushColor.rgb, 1.0 - dist);
    //    }
    //}
    //else
    //{
    //    discard;
    //}
    
    return float4(input.BrushColor.rgb,dist * 2);
    //return input.BrushColor;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};