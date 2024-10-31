#include "BMesh_Subdivision.h"

using namespace SprueEngine;

namespace BMesh
{

    //??void addJointWeights(const QHash<int, float> &src, QHash<int, float> &dst, float weight) {
    //??    QHashIterator<int, float> it(src);
    //??    while (it.hasNext()) {
    //??        it.next();
    //??        dst[it.key()] += it.value() * weight;
    //??    }
    //??}


    std::pair<int, int> getEdgeIndex(int v1, int v2) {
        return v1 < v2 ? std::pair<int, int>(v1, v2) : std::pair<int, int>(v2, v1);
    }


    CatmullFace::CatmullFace(int numSides) : n(numSides) {
        for (int i = 0; i < numSides; ++i) {
            neighbors.push_back(NULL);
            points.push_back(-1);
            edges.push_back(std::pair<int, int>());
        }
    }


    CatmullMesh::CatmullMesh(const Mesh &m) {
        // copy the vertices over
        for (const Vertex &v : m.vertices) {
            CatmullVertex cv;
            cv.pos = v.pos;
            //TODO cv.jointWeights = v.jointWeights;
            vertices.push_back(cv);
        }

        // create the faces with vertices only
        for (const Triangle &tri : m.triangles) 
        {
            CatmullFace cTri(3);
            // set vertices (copies made since Catmull vertex positions will be changed)
            cTri.points[0] = tri.a.index;
            cTri.points[1] = tri.b.index;
            cTri.points[2] = tri.c.index;
            faces.push_back(cTri);
        }

        for (const Quad &quad : m.quads) 
        {
            CatmullFace cQuad(4);
            cQuad.points[0] = quad.a.index;
            cQuad.points[1] = quad.b.index;
            cQuad.points[2] = quad.c.index;
            cQuad.points[3] = quad.d.index;
            faces.push_back(cQuad);
        }

        // add the edges and face points
        for (int iface = 0; iface < faces.size(); ++iface) {
            CatmullFace &face = faces[iface];

#define CONTAINS(LIST, VAL) (LIST.find(VAL) != LIST.end())
            // add edges
            for (int i = 0; i < face.n; ++i) {
                const std::pair<int, int>& pair = getEdgeIndex(face.points[i], face.points[(i + 1) % face.n]);
                face.edges[i] = pair;
                if (!CONTAINS(edges, pair)) {
                    edges[pair].faces[0] = &face;
                }
                else if (edges[pair].faces[1] == NULL) {
                    edges[pair].faces[1] = &face;
                }
                else {
                    //TODO std::cerr << "Error, edge connects to more than 2 faces" << std::endl;
                    valid = false;
                    return;
                }
            }

            // add face point
            Vertex fp;
            fp.pos = Vec3();
            // set face point to average of vertex points
            float weight = 1.f / face.n;
            for (int i = 0; i < face.n; ++i) {
                fp.pos += vertices[face.points[i]].pos;
                //TODO addJointWeights(vertices[face.points[i]].jointWeights, fp.jointWeights, weight);
            }
            fp.pos *= weight;

            face.facePoint = facePoints.size();
            facePoints.push_back(fp);
        }

        // add the edge points
        std::map<std::pair<int, int>, CatmullEdge>::const_iterator it;
        for (it = edges.begin(); it != edges.end(); ++it) 
        {            
            CatmullEdge &edge = edges[it->first];

            if (it->second.faces[1] == NULL) {
                // for edges on the border of a hole, the edge point is average of edge endpoints
                edge.pos = (vertices[it->first.first].pos + vertices[it->first.second].pos) / 2;

                // set the weights for animation
                //TODO addJointWeights(vertices[it.key().first].jointWeights, edge.jointWeights, 0.5f);
                //TODO addJointWeights(vertices[it.key().second].jointWeights, edge.jointWeights, 0.5f);
            }
            else {
                // edge point is average of edge endpoints and adjacent face points
                edge.pos = (vertices[it->first.first].pos + vertices[it->first.second].pos +
                    facePoints[it->second.faces[0]->facePoint].pos + facePoints[it->second.faces[1]->facePoint].pos) / 4;

                // set the weights for animation
                //TODO addJointWeights(vertices[it.key().first].jointWeights, edge.jointWeights, 0.25f);
                //TODO addJointWeights(vertices[it.key().second].jointWeights, edge.jointWeights, 0.25f);
                //TODO addJointWeights(facePoints[it.value().faces[0]->facePoint].jointWeights, edge.jointWeights, 0.25f);
                //TODO addJointWeights(facePoints[it.value().faces[1]->facePoint].jointWeights, edge.jointWeights, 0.25f);
            }
        }

        valid = true;
    }

    // update the mesh vertices (step 3 of Catmull-Clark subdivision)
    bool CatmullMesh::moveVertices() {
        // find neighboring edges and faces
        for (const CatmullFace &face : faces) 
        {
            for (int i = 0; i < face.n; ++i) 
            {
                int prevIndex = (i == 0 ? face.n - 1 : i - 1);
                CatmullVertex &v = vertices[face.points[i]];
                v.facePoints.push_back(&facePoints[face.facePoint]);

#define VEC_CONTAINS(VEC, VAL) (std::find(VEC.begin(), VEC.end(), VAL) == VEC.end())
                if (!VEC_CONTAINS(v.edges, &edges[face.edges[i]])) {
                //if (!v.edges.contains(&edges[face.edges[i]])) {
                    v.edges.push_back(&edges[face.edges[i]]);
                }
                if (!VEC_CONTAINS(v.edges, &edges[face.edges[prevIndex]])) {
                //if (!v.edges.contains(&edges[face.edges[prevIndex]])) {
                    v.edges.push_back(&edges[face.edges[prevIndex]]);
                }
            }
        }

        // take averages for faces and edges and move vertices
        Vec3 faceAverage, edgeAverage;
        for (int i = 0; i < vertices.size(); ++i) {
            CatmullVertex &v = vertices[i];
            std::unordered_map<int, float> origWeights = v.jointWeights;
            v.jointWeights.clear();

            faceAverage.x = faceAverage.y = faceAverage.z = 0;
            edgeAverage.x = edgeAverage.y = edgeAverage.z = 0;

            if (v.edges.size() != v.facePoints.size()) {
                // point is on the border of a hole, average edges along hole and old point
                int count = 0;
                for (const CatmullEdge *edge : v.edges) {
                    if (edge->faces[1] == NULL) {
                        edgeAverage += edge->pos;
                        ++count;
                    }
                }
                v.pos = (edgeAverage + v.pos) / (count + 1);

                // second pass to add in animation weights
                float weight = 1.f / (count + 1);
                for (const CatmullEdge *edge : v.edges) {
                    //addJointWeights(edge->jointWeights, v.jointWeights, weight);
                }
                //addJointWeights(origWeights, v.jointWeights, weight);

            }
            else 
            {
                int numNeighbors = v.edges.size();
                float weight = 1.f / (numNeighbors * numNeighbors);
                for (int i = 0; i < numNeighbors; ++i) {
                    faceAverage += v.facePoints[i]->pos;
                    edgeAverage += v.edges[i]->pos;
                    //TODO addJointWeights(v.facePoints[i]->jointWeights, v.jointWeights, weight);
                    //TODO addJointWeights(v.edges[i]->jointWeights, v.jointWeights, 2 * weight);
                }
                faceAverage /= numNeighbors;
                edgeAverage /= numNeighbors;

                //TODO addJointWeights(origWeights, v.jointWeights, (numNeighbors - 3.f) / numNeighbors);

                // Pnew = ( F + 2R + (n - 3)P ) / n
                v.pos = (faceAverage + edgeAverage * 2 + v.pos * (numNeighbors - 3)) / numNeighbors;
            }
        }

        return true;
    }


    bool CatmullMesh::convertToMesh(Mesh &m) {
        // clear the old mesh
        m.vertices.clear();
        m.triangles.clear();
        m.quads.clear();

        // create the list of vertices
        for (const CatmullVertex &cv : vertices) 
        {
            Vertex v;
            v.pos = cv.pos;
            //TODO v.jointWeights = cv.jointWeights;
            m.vertices.push_back(v);
        }

        std::map<std::pair<int, int>, int> edgeIndices;
        std::map<std::pair<int, int>, CatmullEdge>::const_iterator it;
        for (auto it = edges.begin(); it != edges.end(); ++it) {
            const CatmullEdge &edge = it->second;
            edgeIndices[it->first] = m.vertices.size();
            Vertex v;
            v.pos = edge.pos;
            //v.jointWeights = edge.jointWeights;
            m.vertices.push_back(v);
        }

        int faceOffset = m.vertices.size();
        for (const Vertex &cv : facePoints) 
        {
            m.vertices.push_back(cv);
        }

        // create the lists of quads and triangles
        for (const CatmullFace& face : faces) {
            int faceIndex = faceOffset + face.facePoint;
            for (int i = 0; i < face.n; ++i) {
                Quad q;
                q.a = face.points[i];
                q.b = edgeIndices[face.edges[i]];
                q.c = faceIndex;
                q.d = edgeIndices[face.edges[(i + face.n - 1) % face.n]];
                m.quads.push_back(q);
            }
        }

        return true;
    }


    bool CatmullMesh::subdivide(const Mesh &in, Mesh &out) {
        CatmullMesh cm(in);
        if (!cm.valid) 
            return false;
        if (!cm.moveVertices()) 
            return false;
        out.balls = in.balls;
        if (!cm.convertToMesh(out)) 
            return false;
        out.updateNormals();
        return true;
    }

}