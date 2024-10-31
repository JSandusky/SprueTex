#pragma once

#include <SprueEngine/Meshing/MeshingAlgorithm.h>

namespace SprueEngine
{
    /// Based on Cory Gene Bloyd's public domain implementation.
    /// http://paulbourke.net/geometry/polygonise/
    class SPRUE MarchingCubes : public MeshingAlgorithm
    {
        BASECLASSDEF(MarchingCubes, MeshingAlgorithm);
        NOCOPYDEF(MarchingCubes);
    public:
        MarchingCubes() { }

        virtual void GenerateMesh(IMeshable* meshable, VertexBuffer& vertexBuffer, IndexBuffer& indexBuffer) override;

    private:
        inline int offset_3d(const Vec3 &p, const Vec3 &size) { return (p.z * size.y + p.y) * size.x + p.x; }
    };

}