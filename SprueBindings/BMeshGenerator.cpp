#include "Stdafx.h"

#include "BMeshGenerator.h"
#include <SprueEngine/Libs/BMesh/BMeshWrapper.h>

#include <SprueEngine/Libs/BMesh2/BMesh_Mesh.h>

#include <SprueEngine/Geometry/MeshData.h>

namespace SprueBindings
{

    MeshData^ BMeshBuilder::Build(array<BMeshBall^>^ balls, array<BMeshConnection^>^ links, int subdivs)
    {
#if 1
        std::vector<SprueEngine::BMeshBall> cBalls;
        std::vector<SprueEngine::BMeshLink> cConnections;

        for (int i = 0; i < balls->Length; ++i)
        {
            auto ball = balls[i];
            SprueEngine::BMeshBall b;
            b.position_ = SprueEngine::Vec3(ball->position_.X, ball->position_.Y, ball->position_.Z);
            b.radius_ = ball->radius_;
            b.radiusX_ = ball->radiusX_;
            b.radiusY_ = ball->radiusY_;
            b.type_ = ball->ballType_;
            cBalls.push_back(b);
        }

        for (int i = 0; i < links->Length; ++i)
        {
            auto link = links[i];
            if (link->from_ == -1 || link->to_ == -1)
                continue;
            SprueEngine::BMeshLink conn;
            conn.start_ = link->from_;
            conn.end_ = link->to_;
            cConnections.push_back(conn);
        }

        if (SprueEngine::MeshData* meshData = SprueEngine::BuildBMesh(cBalls, cConnections, subdivs))
            return gcnew SprueBindings::MeshData(meshData);

        return nullptr;
#else

        BMesh::Mesh mesh;
        for (int i = 0; i < balls->Length; ++i)
        {
            auto ball = balls[i];
            BMesh::Ball oBall = BMesh::Ball(SprueEngine::Vec3(ball->position_.X, ball->position_.Y, ball->position_.Z),
                ball->radius_);
            oBall.ex = SprueEngine::Vec3(ball->axisX_.X, ball->axisX_.Y, ball->axisX_.Z);
            oBall.ey = SprueEngine::Vec3(ball->axisY_.X, ball->axisY_.Y, ball->axisY_.Z);
            oBall.ez = SprueEngine::Vec3(ball->axisZ_.X, ball->axisZ_.Y, ball->axisZ_.Z);
            mesh.balls.push_back(oBall);
        }

        for (int i = 0; i < links->Length; ++i)
        {
            auto link = links[i];
            if (link->from_ != -1 && link->to_ != -1)
                mesh.balls[link->to_].parentIndex = link->from_;
        }

        BMesh::Mesh::RunAll(&mesh, subdivs);

        SprueEngine::MeshData* ret = new SprueEngine::MeshData();
        
        for (const BMesh::Vertex& vertex : mesh.vertices)
        {
            ret->positionBuffer_.push_back(vertex.pos);
            ret->normalBuffer_.push_back(vertex.normal);
        }

        for (const BMesh::Triangle& tri : mesh.triangles)
        {
            ret->indexBuffer_.push_back(tri.a.index);
            ret->indexBuffer_.push_back(tri.b.index);
            ret->indexBuffer_.push_back(tri.c.index);
        }

        for (const BMesh::Quad& quad : mesh.quads)
        {
            ret->indexBuffer_.push_back(quad.a.index);
            ret->indexBuffer_.push_back(quad.b.index);
            ret->indexBuffer_.push_back(quad.c.index);

            ret->indexBuffer_.push_back(quad.a.index);
            ret->indexBuffer_.push_back(quad.c.index);
            ret->indexBuffer_.push_back(quad.d.index);
        }
        ret->Smooth(0.6f);
        return gcnew MeshData(ret);
#endif
    }
}