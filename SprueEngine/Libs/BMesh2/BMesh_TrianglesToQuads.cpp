#include "BMesh_TrianglesToQuads.h"

#include "BMesh_Mesh.h"

#include <algorithm>
#include <map>
#include <unordered_map>
#include <vector>

using namespace SprueEngine;

namespace BMesh
{

    Vec3 getNormal(const Mesh &mesh, const Triangle &tri)
    {
        const Vec3 &a = mesh.vertices[tri.a.index].pos;
        const Vec3 &b = mesh.vertices[tri.b.index].pos;
        const Vec3 &c = mesh.vertices[tri.c.index].pos;
        return (c - a).Cross(b - a).Normalized();
    }

    void edgeHelper(std::vector<int>& indices, int a, int b, const Triangle& tri)
    {
        if (tri.a.index == a && tri.b.index == b) indices.push_back(tri.c.index);
        else if (tri.b.index == a && tri.c.index == b) indices.push_back(tri.a.index);
        else if (tri.c.index == a && tri.a.index == b) indices.push_back(tri.b.index);
    }

    struct Edge
    {
        Quad quad;
        int indexA, indexB;
        float score;

        Edge() : indexA(-1), indexB(-1), score(0) {}

        bool computeScore(const Mesh &mesh)
        {
            if (indexA == -1 || indexB == -1) return false;

            const Triangle &triA = mesh.triangles[indexA];
            const Triangle &triB = mesh.triangles[indexB];

            // merge the two triangles
            std::vector<int> indices;
            indices.push_back(triA.a.index);
            edgeHelper(indices, triA.b.index, triA.a.index, triB);
            indices.push_back(triA.b.index);
            edgeHelper(indices, triA.c.index, triA.b.index, triB);
            indices.push_back(triA.c.index);
            edgeHelper(indices, triA.a.index, triA.c.index, triB);
            if (indices.size() != 4) return false; // this happens sometimes, like in buddha.obj
            quad = Quad(indices[0], indices[1], indices[2], indices[3]);

            // compute a score
            Vec3 normalA = getNormal(mesh, triA);
            Vec3 normalB = getNormal(mesh, triB);
            const Vec3& a = mesh.vertices[quad.a.index].pos;
            const Vec3& b = mesh.vertices[quad.b.index].pos;
            const Vec3& c = mesh.vertices[quad.c.index].pos;
            const Vec3& d = mesh.vertices[quad.d.index].pos;
            float ac = (a - c).Length();
            float bd = (b - d).Length();

            // measure how similar the surface normals are
            score = normalA.Dot(normalB);

            // penalize for quads with different length diagonals
            score -= fabsf(ac - bd) / (ac + bd);

            // penalize for quads with different diagonal centers
            score -= (a + c - b - d).Length() / (ac + bd);

            return true;
        }

        bool operator < (const Edge &other) const
        {
            return score > other.score;
        }
    };

    typedef std::pair<int, int> Pair;

    Pair makePair(int a, int b)
    {
        return Pair(std::min(a, b), std::max(a, b));
    }

    typedef std::map<Pair, Edge> EdgeTable;
    typedef std::unordered_map<int, bool> DeletedTriTable;

    void TrianglesToQuads::run(Mesh* aMesh)
    {
        if (aMesh == nullptr)
            return;

        Mesh& mesh = *aMesh;
        std::map<Pair, Edge> edges;
        std::unordered_map<int, bool> triangleDeleted;

        for (int i = 0; i < mesh.triangles.size(); i++)
        {
            const Triangle &tri = mesh.triangles[i];
            Pair pair;

            pair = makePair(tri.a.index, tri.b.index);
            if (edges.find(pair) != edges.end()) 
                edges[pair].indexB = i;
            else 
                edges[pair].indexA = i;

            pair = makePair(tri.b.index, tri.c.index);
            if (edges.find(pair) != edges.end()) 
                edges[pair].indexB = i;
            else 
                edges[pair].indexA = i;

            pair = makePair(tri.c.index, tri.a.index);
            if (edges.find(pair) != edges.end()) 
                edges[pair].indexB = i;
            else 
                edges[pair].indexA = i;
        }

        // sort triangles by score
        std::vector<Edge> sortedEdges;
        for (auto i = edges.begin(); i != edges.end(); ++i)
        {
            Edge &edge = edges[i->first];
            if (edge.computeScore(mesh))
                sortedEdges.push_back(edge);
        }
        std::sort(sortedEdges.begin(), sortedEdges.end());

        // convert triangles to quads
        for (const Edge &edge : sortedEdges)
        {
            if (triangleDeleted[edge.indexA] || triangleDeleted[edge.indexB])
                continue;

            triangleDeleted[edge.indexA] = true;
            triangleDeleted[edge.indexB] = true;
            mesh.quads.push_back(edge.quad);
        }

        // delete triangles
        std::vector<Triangle> triangles;
        for (int i = 0; i < mesh.triangles.size(); i++)
            if (!triangleDeleted[i])
                triangles.push_back(mesh.triangles[i]);
        mesh.triangles = triangles;
    }


}