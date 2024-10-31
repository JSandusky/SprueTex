#pragma once

#include <SprueEngine/MathGeoLib/AllMath.h>

#include <unordered_set>
#include <vector>

namespace BMesh
{

    class Mesh;

    struct VertexInfo
    {
        SprueEngine::Vec3 nextPos;
        std::unordered_set<int> neighbors;
    };

    class EdgeFairing
    {
    private:
        Mesh* mesh;
        std::vector<VertexInfo> vertexInfo;

        EdgeFairing(Mesh* mesh) : mesh(mesh) {}
        void computeNeighbors();
        void iterate(float power = 0.1f);

    public:
        static void run(Mesh* mesh, int iterations, float power);
    };


}