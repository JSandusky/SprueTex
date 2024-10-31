#pragma once

#include <SprueEngine/MathGeoLib/AllMath.h>
#include <SprueEngine/ClassDef.h>
#include <SprueEngine/IMeshable.h>
#include <SprueEngine/Geometry/MeshData.h>

namespace SprueEngine
{
    /// Baseclass for implementations of meshing algorithms.
    class SPRUE MeshingAlgorithm
    {
        NOCOPYDEF(MeshingAlgorithm);
    public:
        MeshingAlgorithm() { }
        virtual void GenerateMesh(IMeshable* meshable, VertexBuffer& vertexBuffer, IndexBuffer& indexBuffer) = 0;

        /// Returns true if the meshing method uses hermite data (edge intersections and normals).
        virtual bool IsHermiteMethod() const { return false; }
        /// Returns true if the meshing method is a cuberille method (DC, surface-nets)
        virtual bool IsCuberilleMethod() const { return false; }
        /// Returns true if the meshing method is a primal method (Marching Cubes)
        virtual bool IsPrimalMethod() const { return false; }
    };

}