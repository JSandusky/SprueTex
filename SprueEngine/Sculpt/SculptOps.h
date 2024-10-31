#pragma once

#include <SprueEngine/BlockMap.h>
#include <SprueEngine/Math/Color.h>
#include <SprueEngine/MathGeoLib/AllMath.h>

#include <vector>
#include <set>
#include <memory>

namespace SprueEngine
{

class MeshData;

struct StrokeData
{
    StrokeData(const std::vector<float>& weights) : maskWeights(weights) { }

    /// Position of the brush in 3d space
    Vec3 brushPosition;
    /// Current running mouse position, for use in a system where mouse position is repeatedly snapped
    Vec2 activeMouse;
    /// Mouse position at the very start
    Vec2 mouseStart;
    /// Previous mouse position
    Vec2 mouseBegin;
    /// Current mouse position
    Vec2 mouseEnd;
    /// Normal at the current picking
    Vec3 startNormal;
    Vec3 normal;

    /// Viewing camera vector
    Vec3 viewUp;
    /// Viewing camera vector
    Vec3 viewRight;
    /// Viewing camera vector
    Vec3 viewForward;
    /// Value to use for vertex flat coloring
    RGBA color;
    std::shared_ptr<FilterableBlockMap<float> > mask;
    std::shared_ptr<FilterableBlockMap<RGBA> > texture;

    Vec2 mouseDelta() { return mouseEnd - mouseStart; }
    float GetMaskWeight(float u, float v, bool wrap = false) const;
    RGBA GetTexture(float u, float v, bool wrap = false) const;

    /// Per vertex weights.
    std::vector<float> maskWeights;
};

void Brush(MeshData* mesh, StrokeData*, const std::vector<unsigned>& vertIndices, const Vec3& pickNormal, const Vec3& pickCenter, float pickRadius2, float intensity, float hardness, bool negative);

void Crease(MeshData* mesh, StrokeData*, const std::vector<unsigned>& vertIndices, const Vec3& pickNormal, const Vec3& pickCenter, float pickRadius2, float intensity, float hardness, bool negative);

void Drag(MeshData* mesh, StrokeData*, const std::vector<unsigned>& vertIndices, const Vec3& pickNormal, const Vec3& pickCenter, float pickRadius2, float intensity, float hardness, bool negative);

void Flatten(MeshData* mesh, StrokeData*, const std::vector<unsigned>& vertIndices, const Vec3& pickNormal, const Vec3& pickCenter, float pickRadius2, float intensity, float hardness, bool negative);

void Inflate(MeshData* mesh, StrokeData*, const std::vector<unsigned>& vertIndices, const Vec3& pickNormal, const Vec3& pickCenter, float pickRadius2, float intensity, float hardness, bool negative);

void LocalScale(MeshData* mesh, StrokeData*, const std::vector<unsigned>& vertIndices, const Vec3& pickNormal, const Vec3& pickCenter, float pickRadius2, float intensity, float hardness, bool negative);

void Mask(MeshData* mesh, StrokeData*, const std::vector<unsigned>& vertIndices, const Vec3& pickNormal, const Vec3& pickCenter, float pickRadius2, float intensity, float hardness, bool negative);
    
void PaintVertexColor(MeshData* mesh, StrokeData*, const std::vector<unsigned>& vertIndices, const Vec3& pickNormal, const Vec3& pickCenter, float pickRadius2, float intensity, float hardness, bool negative);

void Smooth(MeshData* mesh, StrokeData*, const std::vector<unsigned>& vertIndices, const Vec3& pickNormal, const Vec3& pickCenter, float pickRadius2, float intensity, float hardness, bool negative);

void Pinch(MeshData* mesh, StrokeData*, const std::vector<unsigned>& vertIndices, const Vec3& pickNormal, const Vec3& pickCenter, float pickRadius2, float intensity, float hardness, bool negative);

void Transform(MeshData* mesh, StrokeData*, const std::vector<unsigned>& vertIndices, const Vec3& pickNormal, const Vec3& pickCenter, float pickRadius2, float intensity, float hardness, bool negative);

void Twist(MeshData* mesh, StrokeData*, const std::vector<unsigned>& vertIndices, const Vec3& pickNormal, const Vec3& pickCenter, float pickRadius2, float intensity, float hardness, bool negative);

}