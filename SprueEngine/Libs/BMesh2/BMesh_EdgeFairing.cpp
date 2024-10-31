#include "BMesh_EdgeFairing.h"

#include "BMesh_Mesh.h"

using namespace SprueEngine;

namespace BMesh
{

    // helper to sort vertices by their rotation in a coordinate system
    struct rotationInCoordinateSystem
    {
        Vec3 origin, axisX, axisY;

        rotationInCoordinateSystem(const Vec3& origin, const Vec3& axisX, const Vec3& axisY) : 
            origin(origin), axisX(axisX), axisY(axisY) 
        {
        }

        bool operator () (const Vec3 &a, const Vec3 &b)
        {
            float angleA = atan2f(axisX.Dot(a - origin), axisY.Dot(a - origin));
            float angleB = atan2f(axisX.Dot(b - origin), axisY.Dot(b - origin));
            return angleA < angleB;
        }
    };

    void EdgeFairing::computeNeighbors()
    {
        vertexInfo.resize(mesh->vertices.size());

        for (const Triangle& tri : mesh->triangles)
        {
            vertexInfo[tri.a.index].neighbors.insert(tri.b.index);
            vertexInfo[tri.a.index].neighbors.insert(tri.c.index);

            vertexInfo[tri.b.index].neighbors.insert(tri.c.index);
            vertexInfo[tri.b.index].neighbors.insert(tri.a.index);

            vertexInfo[tri.c.index].neighbors.insert(tri.a.index);
            vertexInfo[tri.c.index].neighbors.insert(tri.b.index);
        }

        for (const Quad& quad : mesh->quads)
        {
            vertexInfo[quad.a.index].neighbors.insert(quad.b.index);
            vertexInfo[quad.a.index].neighbors.insert(quad.d.index);

            vertexInfo[quad.b.index].neighbors.insert(quad.c.index);
            vertexInfo[quad.b.index].neighbors.insert(quad.a.index);
            
            vertexInfo[quad.c.index].neighbors.insert(quad.d.index);
            vertexInfo[quad.c.index].neighbors.insert(quad.b.index);
            
            vertexInfo[quad.d.index].neighbors.insert(quad.a.index);
            vertexInfo[quad.d.index].neighbors.insert(quad.c.index);
        }
    }

    void EdgeFairing::iterate(float power)
    {
        for (int i = 0; i < vertexInfo.size(); i++)
        {
            Vertex &vertex = mesh->vertices[i];
            VertexInfo &info = vertexInfo[i];

            // special-case vertices with valence 4
            if (info.neighbors.size() == 4)
            {
                // project the neighbors onto the tangent plane
                std::vector<Vec3> projectedNeighbors;
                for (int neighbor : info.neighbors)
                {
                    Vec3& pos = mesh->vertices[neighbor].pos;
                    projectedNeighbors.push_back(pos + vertex.normal * (vertex.pos - pos).Dot(vertex.normal));
                }

                // define a tangent-space coordinate system
                Vec3 axisX = (projectedNeighbors[0] - vertex.pos).Normalized();
                Vec3 axisY = vertex.normal.Cross(axisX);

                // sort the vertices by their rotation in the tangent-space coordinate system
                std::sort(projectedNeighbors.begin(), projectedNeighbors.end(), rotationInCoordinateSystem(vertex.pos, axisX, axisY));

                // we now have vectors in clockwise or counter-clockwise order
                //
                //      b
                //      |
                //  a---+---c
                //      |
                //      d
                //
                Vec3& a = projectedNeighbors[0];
                Vec3& b = projectedNeighbors[1];
                Vec3& c = projectedNeighbors[2];
                Vec3& d = projectedNeighbors[3];

                // set the vertex to the intersection of the lines between the two opposite neighbor pairs
                // blend between the intersection and the average for stability
                Vec3 normal = (c - a).Cross(vertex.normal).Normalized();
                float t = (a - b).Dot(normal) / (d - b).Dot(normal);
                Vec3 intersection = b + (d - b) * std::max(0.0f, std::min(1.0f, t));
                Vec3 average = (a + b + c + d) / 4;
                info.nextPos = (average + intersection) / 2;
            }
            else
            {
                // calculate the average neighbor vertex position
                Vec3 average;
                for (int neighbor : info.neighbors)
                    average += mesh->vertices[neighbor].pos;
                average /= info.neighbors.size();

                // move the vertex to the average projected onto the tangent plane
                float t = (vertex.pos - average).Dot(vertex.normal);
                info.nextPos = average + vertex.normal * t;
            }
        }

        // actually move the vertices (must be done in a separate loop or we would be mutating while iterating)
        // only move a small amount per iteration for stability
        for (int i = 0; i < vertexInfo.size(); ++i)
            mesh->vertices[i].pos = Vec3::Lerp(mesh->vertices[i].pos, vertexInfo[i].nextPos, power);

        mesh->updateNormals();
    }

    void EdgeFairing::run(Mesh* mesh, int iterations, float power)
    {
        EdgeFairing edgeFairing(mesh);
        edgeFairing.computeNeighbors();

        for (int i = 0; i < iterations; i++)
            edgeFairing.iterate(power);
    }

}