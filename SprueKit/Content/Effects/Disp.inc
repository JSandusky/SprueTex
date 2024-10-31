// Performs parallax

float HeightFieldRaycast(float2 dp, float2 ds)
{
    const int linearSteps = 20;
    const int binarySteps = 10;

    float size = 1.0 / float(linearSteps);
    float depth = 0.0;
    for (int i = 0; i < linearSteps; i++)
    {
        float t = tex2D(HeightMapSampler, dp + ds*depth).r;
        if (depth < (1.0 - t))
            depth += size;
    }
    
    for (int ii = 0; ii < binarySteps; ii++)
    {
        size *= 0.5;
        float t = tex2D(HeightMapSampler, dp + ds * depth).r;
        if (depth<(1.0 - t))
            depth += (2.0 * size);
        depth -= size;
    }
    return depth;
}

float2 updateUV(
    float3 pointToCameraDirWS,
    float3 n,
    float3 t,
    float3 b,
    float Depth,
    float2 uv)
{
    if (Depth > 0.0)
    {
        float a = dot(n, -pointToCameraDirWS);
        float3 s = float3(-dot(pointToCameraDirWS, t), dot(pointToCameraDirWS, b), a);
        s *= -Depth / a * 0.1;
        float2 ds = s.xy;
        float d = HeightFieldRaycast(uv, ds);
        return uv + (ds * d);
    }
    else 
        return uv;
    return uv;
}