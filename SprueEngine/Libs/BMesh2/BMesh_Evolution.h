#pragma once

#include <SprueEngine/Libs/BMesh2/BMesh_Mesh.h>

namespace BMesh
{

    /**
    * Defines a signed distance field around a sphere swept from one sphere to another.
    */
    class Sweep
    {
    private:
        float project(const SprueEngine::Vec3& pos) const;

        SprueEngine::Vec3 centerA;
        SprueEngine::Vec3 centerB;
        SprueEngine::Vec3 AtoB;
        float radiusA;
        float radiusB;
        float AtoB_lengthSquared;
        bool onlyUseA;

    public:
        Sweep() {}
        Sweep(const Ball &ballA, const Ball &ballB) :
            centerA(ballA.center), 
            centerB(ballB.center), 
            AtoB(centerB - centerA),
            radiusA(ballA.maxRadius()), 
            radiusB(ballB.maxRadius()),
            AtoB_lengthSquared(AtoB.LengthSq()), 
            onlyUseA(false)
        {
            float distance = (centerA - centerB).LengthSq();
            if (distance + radiusA <= radiusB)
            {
                // A is inside B
                centerA = centerB;
                radiusA = radiusB;
                onlyUseA = true;
            }
            else if (distance + radiusB <= radiusA)
            {
                // B is inside A
                onlyUseA = true;
            }
        }

        Sweep(const Ball& ballA, const Ball& ballB, float interpA, float interpB) :
            centerA(ballA.center),
            centerB(ballB.center),
            AtoB(centerB - centerA),
            radiusA(ballA.maxRadius()),
            radiusB(ballB.maxRadius()),
            AtoB_lengthSquared(AtoB.LengthSq()),
            onlyUseA(false)
        {
            centerA = SprueEngine::Vec3::Lerp(ballA.center, ballB.center, interpA);
            centerB = SprueEngine::Vec3::Lerp(ballA.center, ballB.center, interpB);
            radiusA = SprueLerp(ballA.maxRadius(), ballB.maxRadius(), interpA);
            radiusB = SprueLerp(ballA.maxRadius(), ballB.maxRadius(), interpB);
            AtoB = centerB - centerA;
            AtoB_lengthSquared = AtoB.LengthSq();

            float distance = (centerA - centerB).LengthSq();
            if (distance + radiusA <= radiusB)
            {
                // A is inside B
                centerA = centerB;
                radiusA = radiusB;
                onlyUseA = true;
            }
            else if (distance + radiusB <= radiusA)
            {
                // B is inside A
                onlyUseA = true;
            }
        }

        float scalarField(const SprueEngine::Vec3& pos) const;
    };

    class MeshEvolution
    {
    private:
        MeshEvolution();

        Mesh &mesh;
        std::vector<Sweep> sweeps;

        MeshEvolution(Mesh &mesh);

        float scalarField(int ball, const SprueEngine::Vec3& pos) const;
        SprueEngine::Vec3 scalarFieldNormal(int ball, const SprueEngine::Vec3& pos) const;
        void evolve() const;

    public:
        static void run(Mesh &mesh);
        static void drawDebug(Mesh &mesh, float min, float max, int increments);
    };

}