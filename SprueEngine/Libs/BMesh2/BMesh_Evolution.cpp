#include "BMesh_Evolution.h"

#include <SprueEngine/ResponseCurve.h>

using namespace SprueEngine;

namespace BMesh
{

    inline math::float3x3 EllipsoidMatrix(const Vec3& x, const Vec3& y, const Vec3& z)
    {
        return math::float3x3(x, y, z).Inverted();
    }

    inline float EllipsoidDistance(const math::float3x3& ellipse, const Vec3& samplePos, const Vec3& centroid)
    {
        auto vec = ellipse * samplePos;
        return 0.0f;
    }

    float Sweep::project(const Vec3& pos) const
    {
        return AtoB.Dot(pos - centerA) / AtoB_lengthSquared;
    }

    float Sweep::scalarField(const Vec3& pos) const
    {
        // hax that gets the signed distance to  //
        // a cylinder fitted between two spheres //
        //                                       //
        //              ___---^^^/^^^\           //
        //           /^^\       /     \          //
        //           \__/       \     /          //
        //              ^^^---___\___/           //
        //                                       //
        if (onlyUseA) 
            return (centerA - pos).Length() - radiusA;

        Vec3 closest = centerA + AtoB * project(pos);
        Vec3 tilted = closest + AtoB * ((radiusB - radiusA) * (closest - pos).Length() / AtoB_lengthSquared);
        float t = std::max(0.0f, std::min(1.0f, project(tilted)));
        return (centerA + AtoB * t - pos).Length() - (radiusA + (radiusB - radiusA) * t);
    }

    Vec3 MeshEvolution::scalarFieldNormal(int ball, const Vec3& pos) const
    {
        const float e = 1e-2;
        Vec3 dx(e, 0, 0);
        Vec3 dy(0, e, 0);
        Vec3 dz(0, 0, e);

        Vec3 normal(
            scalarField(ball, pos + dx) - scalarField(ball, pos - dx),
            scalarField(ball, pos + dy) - scalarField(ball, pos - dy),
            scalarField(ball, pos + dz) - scalarField(ball, pos - dz)
        );

        float len = normal.Length();
        if (len < 1e-6) {
            return Vec3();
        }

        return normal / len;
    }

    void MeshEvolution::drawDebug(Mesh &mesh, float gridMin, float gridMax, int divisions)
    {
        //MeshEvolution evo(mesh);
        //
        //float x, y, z;
        //float increment = (gridMax - gridMin) / divisions;
        //
        //glBegin(GL_LINES);
        //glColor3f(0, 0, 1);
        //
        //x = gridMin;
        //for (int i = 0; i < divisions; ++i) {
        //    y = gridMin;
        //    for (int j = 0; j < divisions; ++j) {
        //        z = gridMin;
        //        for (int k = 0; k < divisions; ++k) {
        //            glVertex3f(x, y, z);
        //            glVertex3fv((Vector3(x, y, z) + evo.scalarFieldNormal(Vector3(x, y, z)) * increment * 0.2f).xyz);
        //            z += increment;
        //        }
        //        y += increment;
        //    }
        //    x += increment;
        //}
        //
        //glEnd();
    }


    MeshEvolution::MeshEvolution(Mesh &mesh) : mesh(mesh)
    {
        // generate in-between balls using hax
        for (const Ball& ball : mesh.balls)
        {
            // get the parent ball
            if (ball.parentIndex == -1) 
            {
                sweeps.push_back(Sweep(ball, ball));
            }
            else
            {
                const Ball &parent = mesh.balls[ball.parentIndex];
                sweeps.push_back(Sweep(ball, parent));
            }
        }
    }

    float MeshEvolution::scalarField(int ball, const Vec3& pos) const
    {
        float minPos = FLT_MAX, minNeg = 0;
        for (const Sweep &sweep : sweeps) 
        {
            float curr = sweep.scalarField(pos);
            if (curr < 0) {
                minNeg = std::min(minNeg, curr);
            }
            else {
                minPos = std::min(minPos, curr);
            }
        }

        if (minNeg == 0) {
            return minPos;
        }
        return minNeg;
    }

    void MeshEvolution::evolve() const
    {
        for (int i = 0; i < mesh.vertices.size(); ++i) 
        {
            Vertex &vertex = mesh.vertices[i];
            vertex.pos -= scalarFieldNormal(vertex.ball, vertex.pos) * scalarField(vertex.ball, vertex.pos);
        }
        mesh.updateNormals();
    }

    void MeshEvolution::run(Mesh &mesh)
    {
        MeshEvolution evolution(mesh);
        evolution.evolve();
    }

}