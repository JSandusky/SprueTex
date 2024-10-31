#pragma once

#include <SprueEngine/Libs/BMesh2/BMesh_Mesh.h>

#include <map>
#include <unordered_map>

namespace BMesh
{

    // a face in a CatmullMesh
    struct CatmullFace {
        CatmullFace(int numSides);

        int n;
        // these are filled to the correct size in the constructor, so they can be treated like arrays after construction
        std::vector<const CatmullFace *> neighbors;
        std::vector<int> points; // point indices
        std::vector< std::pair<int, int> > edges; // edge indices
        int facePoint;
    };

    // an edge in a CatmullMesh
    struct CatmullEdge {
        CatmullEdge() { faces[0] = faces[1] = NULL; }

        SprueEngine::Vec3 pos; // position of this edge's edgePoint
        std::unordered_map<int, float> jointWeights; // weighted joints for animation
        CatmullFace *faces[2];
    };

    // a vertex in a CatmullMesh
    struct CatmullVertex {
        SprueEngine::Vec3 pos;

        std::unordered_map<int, float> jointWeights; // weighted joints for animation
        std::vector<Vertex *> facePoints; // face points for faces including this vertex
        std::vector<CatmullEdge *> edges; // edges including this vertex

    };

    // a mesh holding additional data used for Catmull-Clark subdivision
    class CatmullMesh {
    public:
        // create the mesh Catmull mesh from a regular mesh
        CatmullMesh(const Mesh &m);

        bool convertToMesh(Mesh &m);
        static bool subdivide(const Mesh &in, Mesh &out);
        static bool subdivide(Mesh &mesh) { return subdivide(mesh, mesh); }

    private:
        std::vector<CatmullVertex> vertices;
        std::map<std::pair<int, int>, CatmullEdge> edges;
        std::vector<Vertex> facePoints;
        std::vector<CatmullFace> faces;
        bool valid;

        bool moveVertices();
    };

}