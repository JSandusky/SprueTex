#include "BMesh_ConvexHull3D.h"

#include "BMesh_Mesh.h"

#include <SprueEngine/MathGeoLib/Geometry/Polygon.h>
#include <SprueEngine/MathGeoLib/Geometry/Polyhedron.h>

using namespace SprueEngine;

namespace BMesh
{

    void ConvexHull3D::run(Mesh* mesh)
    {
        Mesh& m = *mesh;

        math::VecArray pts;
        for (const Vertex &vertex : m.vertices)
            pts.push_back(vertex.pos);
        
        auto hull = math::Polyhedron::ConvexHull(pts);
        
        for (auto& face : hull.Faces())
        {
            
        }

        //auto triangleArray = hull.Triangulate();
        //hull.FaceIndicesValid
        //for (auto& triangle : triangleArray)
        //{
        //    
        //}
        //const int *indices = hull.GetIndices();
        //for (int i = 0; i < numIndices; i++)
        //    mesh.triangles += Triangle(indices[i * 3], indices[i * 3 + 1], indices[i * 3 + 2]);
    }

}