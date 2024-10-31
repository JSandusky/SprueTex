#pragma once

#include <SprueEngine/Meshing/MeshingAlgorithm.h>

namespace SprueEngine
{

    class SPRUE SurfaceNets : public MeshingAlgorithm
    {
        BASECLASSDEF(SurfaceNets, MeshingAlgorithm);
        NOCOPYDEF(SurfaceNets);
    public:
        SurfaceNets() { }

        void SetBlocky(bool blocky) { blocky_ = blocky; }
        bool IsBlocky() const { return blocky_; }

        virtual void GenerateMesh(IMeshable* meshable, VertexBuffer& vertexBuffer, IndexBuffer& indexBuffer) override;
        void ByteGenerateMesh(IMeshable* meshable, VertexBuffer& vertexBuffer, IndexBuffer& indexBuffer);
        virtual void GenerateMesh(const float* densities, const Vec4* points, const unsigned char* writeMasks, const int size, VertexBuffer& vertexBuffer, IndexBuffer& indexBuffer);

    private:
        bool blocky_ = false;
    };

}