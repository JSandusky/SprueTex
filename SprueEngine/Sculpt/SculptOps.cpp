#include "SculptOps.h"

#include <SprueEngine/Geometry/MeshData.h>

namespace SprueEngine
{


float StrokeData::GetMaskWeight(float u, float v, bool wrap) const
{
    return 0x0;
}

RGBA StrokeData::GetTexture(float u, float v, bool wrap) const
{
    return RGBA();
}

void Brush(MeshData* mesh, StrokeData* stroke, const std::vector<unsigned>& vertIndices, const Vec3& pickNormal, const Vec3& pickCenter, float pickRadius2, float intensity, float hardness, bool negative)
{
    float deformIntensity = intensity * 0.07f;
    float radius = sqrtf(pickRadius2);
    float brushFactor = radius * deformIntensity;

    if (negative)
        brushFactor = -brushFactor;

    for (unsigned i = 0; i < vertIndices.size(); ++i)
    {
        Vec3& vert = mesh->GetPositionBuffer()[vertIndices[i]];
        Vec3 normal = mesh->GetNormalBuffer()[vertIndices[i]];
        float sqrDist = (vert - stroke->brushPosition).LengthSq() / pickRadius2;
        if (sqrDist >= 1.0f)
            continue;

        float dist = sqrtf(sqrDist);
        float falloff = sqrDist;
        falloff = 3.0f * falloff * falloff - 4.0f * falloff * dist + 1.0f;
        falloff *= brushFactor;

        vert = vert + pickNormal * falloff;
    }
}

void Drag(MeshData* mesh, StrokeData* stroke, const std::vector<unsigned>& vertIndices, const Vec3& pickNormal, const Vec3& pickCenter, float pickRadius2, float intensity, float hardness, bool negative)
{
    Vec2 mouseDelta = stroke->mouseDelta();
    Vec3 displace = stroke->viewUp * mouseDelta.y + stroke->viewRight * mouseDelta.x;
    displace.y *= -1;
    if (displace.Length() <= 0.01f)
        return;

    for (unsigned i = 0; i < vertIndices.size(); ++i)
    {
        Vec3& vert = mesh->GetPositionBuffer()[vertIndices[i]];
        float sqrDist = (vert - stroke->brushPosition).LengthSq() / pickRadius2;
        if (sqrDist >= 1.0f)
            continue;

        float dist = sqrtf(sqrDist);
        float falloff = sqrDist;
        falloff = 3.0f * falloff * falloff - 4.0f * falloff * dist + 1.0f;

        vert += displace * falloff;
    }
}

void Crease(MeshData* mesh, StrokeData* stroke, const std::vector<unsigned>& vertIndices, const Vec3& pickNormal, const Vec3& pickCenter, float pickRadius2, float intensity, float hardness, bool negative)
{
    float deformIntensity = intensity * 0.07f;
    float radius = sqrtf(pickRadius2);
    float brushFactor = radius * deformIntensity;

    if (negative)
        brushFactor = -brushFactor;

    for (unsigned i = 0; i < vertIndices.size(); ++i)
    {
        Vec3& vert = mesh->GetPositionBuffer()[vertIndices[i]];
        Vec3 normal = mesh->GetNormalBuffer()[vertIndices[i]];
        float sqrDist = (vert - stroke->brushPosition).LengthSq() / pickRadius2;
        if (sqrDist >= 1.0f)
            continue;

        float dist = sqrtf(sqrDist);
        float falloff = sqrDist;
        falloff = 3.0f * falloff * falloff - 4.0f * falloff * dist + 1.0f;

        float modifier = powf(falloff, 5.0f) * brushFactor;
        falloff *= deformIntensity;

        vert += vert * falloff + normal * modifier;
    }
}

void Flatten(MeshData* mesh, StrokeData* stroke, const std::vector<unsigned>& vertIndices, const Vec3& pickNormal, const Vec3& pickCenter, float pickRadius2, float intensity, float hardness, bool negative)
{
    float deformIntensity = intensity * 0.07f;
    float radius = sqrtf(pickRadius2);
    float brushFactor = radius * deformIntensity;

    float comp = negative ? -1.0f : 1.0f;
    if (negative)
        brushFactor = -brushFactor;

    for (unsigned i = 0; i < vertIndices.size(); ++i)
    {
        Vec3& vert = mesh->GetPositionBuffer()[vertIndices[i]];
        Vec3 normal = mesh->GetNormalBuffer()[vertIndices[i]];
        float sqrDist = (vert - stroke->brushPosition).LengthSq() / pickRadius2;
        math::Plane plane(pickCenter, pickNormal);
        float distToPlane = plane.SignedDistance(vert);
        if (distToPlane * comp > 0.0f || sqrDist >= 1.0f)
            continue;

        float dist = sqrtf(sqrDist);
        float falloff = sqrDist;
        falloff = 3.0f * falloff * falloff - 4.0f * falloff * dist + 1.0f;

        float modifier = powf(falloff, 5.0f) * brushFactor;
        falloff *= intensity;
        falloff *= distToPlane > 0.0f ? 1.0f : -1.0f;
        //falloff *= distToPlane * intensity; // picking get alpha;

        vert -= plane.normal * modifier;
    }
}

void Inflate(MeshData* mesh, StrokeData* stroke, const std::vector<unsigned>& vertIndices, const Vec3& pickNormal, const Vec3& pickCenter, float pickRadius2, float intensity, float hardness, bool negative)
{
    float deformIntensity = intensity * 0.07f;
    float radius = sqrtf(pickRadius2);
    float brushFactor = radius * deformIntensity;

    float comp = negative ? -1.0f : 1.0f;
    if (negative)
        brushFactor = -brushFactor;

    for (unsigned i = 0; i < vertIndices.size(); ++i)
    {
        Vec3& vert = mesh->GetPositionBuffer()[vertIndices[i]];
        Vec3 normal = mesh->GetNormalBuffer()[vertIndices[i]];
        float sqrDist = (vert - stroke->brushPosition).LengthSq() / pickRadius2;
        
        Vec3 d = vert - pickCenter;
        float dist = d.Length() / radius;
        if (dist >= 1.0f)
            continue;

        float falloff = sqrDist;
        falloff = 3.0f * falloff * falloff - 4.0f * falloff * dist + 1.0f;
        falloff *= intensity;
        float modifier = powf(falloff, 5.0f) * brushFactor;
        vert = vert + normal * modifier;
    }
}

void LocalScale(MeshData* mesh, StrokeData* stroke, const std::vector<unsigned>& vertIndices, const Vec3& pickNormal, const Vec3& pickCenter, float pickRadius2, float intensity, float hardness, bool negative)
{
    const float deformIntensity = intensity * 0.01f;
    const float radius = sqrtf(pickRadius2);
    const float brushFactor = radius * deformIntensity;

    for (unsigned i = 0; i < vertIndices.size(); ++i)
    {
        Vec3& vert = mesh->GetPositionBuffer()[vertIndices[i]];
        Vec3 normal = mesh->GetNormalBuffer()[vertIndices[i]];
        float sqrDist = (vert - stroke->brushPosition).LengthSq() / pickRadius2;
        if (sqrDist >= 1.0f)
            continue;

        Vec3 d = vert - pickCenter;
        float dist = d.Length() / radius;

        float falloff = sqrDist;
        falloff = 3.0f * falloff * falloff - 4.0f * falloff * dist + 1.0f;
        float modifier = falloff * brushFactor;//powf(falloff, 5.0f) * brushFactor;
        falloff *= deformIntensity;
        vert = vert + d * modifier;
    }
}

void Mask(MeshData* mesh, StrokeData* stroke, const std::vector<unsigned>& vertIndices, const Vec3& pickNormal, const Vec3& pickCenter, float pickRadius2, float intensity, float hardness, bool negative)
{
    if (negative)
        intensity = -intensity;

    float softness = 2.0f * (1.0f - hardness);

    for (unsigned i = 0; i < vertIndices.size(); ++i)
    {
        Vec3& vert = mesh->GetPositionBuffer()[vertIndices[i]];
        Vec3 normal = mesh->GetNormalBuffer()[vertIndices[i]];
        float sqrDist = (vert - stroke->brushPosition).LengthSq() / pickRadius2;
        if (sqrDist >= 1.0f)
            continue;

        Vec3 d = vert - pickCenter;

        float dist = sqrtf(sqrDist);
        float falloff = powf(1.0f - dist, softness);
        falloff *= intensity;

        stroke->maskWeights[i] = SprueMin(SprueMax(stroke->maskWeights[i] + falloff, 0.0f), 1.0f);
    }
}

void PaintVertexColor(MeshData* mesh, StrokeData* stroke, const std::vector<unsigned>& vertIndices, const Vec3& pickNormal, const Vec3& pickCenter, float pickRadius2, float intensity, float hardness, bool negative)
{
    float deformIntensity = intensity * 0.07f;
    float radius = sqrtf(pickRadius2);
    float brushFactor = radius * deformIntensity;

    if (negative)
        brushFactor = -brushFactor;

    for (unsigned i = 0; i < vertIndices.size(); ++i)
    {
        Vec3 vert = mesh->GetPositionBuffer()[vertIndices[i]];
        Vec3 normal = mesh->GetNormalBuffer()[vertIndices[i]];
        RGBA& vertColor = mesh->GetColorBuffer()[vertIndices[i]];
        float sqrDist = (vert - stroke->brushPosition).LengthSq() / pickRadius2;
        if (sqrDist >= 1.0f)
            continue;

        float dist = sqrtf(sqrDist);
        float falloff = sqrDist;
        falloff = 3.0f * falloff * falloff - 4.0f * falloff * dist + 1.0f;
        falloff *= brushFactor;

        vertColor = SprueLerp(vertColor, stroke->color, falloff);
    }
}

void Smooth(MeshData* mesh, StrokeData*, const std::vector<unsigned>& vertIndices, const Vec3& pickNormal, const Vec3& pickCenter, float pickRadius2, float intensity, float hardness, bool negative)
{
    // TODO: laplacian smoothing
}

void Pinch(MeshData* mesh, StrokeData* stroke, const std::vector<unsigned>& vertIndices, const Vec3& pickNormal, const Vec3& pickCenter, float pickRadius2, float intensity, float hardness, bool negative)
{
    float deformIntensity = intensity * 0.07f;
    float radius = sqrtf(pickRadius2);
    float brushFactor = radius * deformIntensity;

    float comp = negative ? -1.0f : 1.0f;
    if (negative)
        brushFactor = -brushFactor;

    for (unsigned i = 0; i < vertIndices.size(); ++i)
    {
        Vec3& vert = mesh->GetPositionBuffer()[vertIndices[i]];
        Vec3 normal = mesh->GetNormalBuffer()[vertIndices[i]];
        float sqrDist = (vert - stroke->brushPosition).LengthSq() / pickRadius2;
        if (sqrDist >= 1.0f)
            continue;

        Vec3 d = pickCenter - vert;
        float dist = sqrtf(sqrDist);// sqrtf(d.Length() / radius;

        float falloff = sqrDist;
        falloff = 3.0f * falloff * falloff - 4.0f * falloff * dist + 1.0f;
        falloff *= brushFactor;
        vert = vert + d * falloff;
    }
}

void Transform(MeshData* mesh, StrokeData*, const std::vector<unsigned>& vertIndices, const Vec3& pickNormal, const Vec3& pickCenter, float pickRadius2, float intensity, float hardness, bool negative)
{

}

void Twist(MeshData* mesh, StrokeData* stroke, const std::vector<unsigned>& vertIndices, const Vec3& pickNormal, const Vec3& pickCenter, float pickRadius2, float intensity, float hardness, bool negative)
{
    Vec2 vecMouse = stroke->mouseEnd - stroke->mouseStart;
    if (vecMouse.Length() <= 0.01f)
        return;

    vecMouse.Normalize();
    Vec3 nNormal = stroke->startNormal;
    Vec2 oldMouse = stroke->mouseBegin - stroke->mouseStart;
    oldMouse.Normalize();
    const float radius = sqrtf(pickRadius2);

    //TODO: radians or degrees
    const float angle = vecMouse.AngleBetween(oldMouse);
    const float invRadius = 1.0f / radius;

    for (unsigned i = 0; i < vertIndices.size(); ++i)
    {
        Vec3& vert = mesh->GetPositionBuffer()[vertIndices[i]];
        Vec3 normal = mesh->GetNormalBuffer()[vertIndices[i]];

        float sqrDist = (vert - stroke->brushPosition).LengthSq() / pickRadius2;
        if (sqrDist >= 1.0f)
            continue;

        Vec3 d = vert - pickCenter;
        float dist = d.Length() / radius;

        float falloff = dist * dist;
        falloff = 3.0f * falloff * falloff - 4.0f * falloff * dist + 1.0f;
        falloff *= angle;
        Quat quat(nNormal.Normalized(), angle);
        //Quat quat(Vec3::zero, nNormal * falloff);
        vert += quat * (vert - pickCenter) * falloff;
    }
}

}