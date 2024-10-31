#include "BMesh_MeshConstruction.h"

#include "BMesh_ConvexHull3D.h"
#include "BMesh_Mesh.h"
#include "BMesh_TrianglesToQuads.h"

#include <SprueEngine/MathGeoLib/AllMath.h>
#include <SprueEngine/MathGeoLib/Geometry/Polyhedron.h>

#include <map>
#include <unordered_map>

using namespace SprueEngine;

namespace BMesh
{

    unsigned int qHash(const Vec3 &vec)
    {
        //unsigned int h1 = qHash(*(int *)&vec.x);
        //unsigned int h2 = qHash(*(int *)&vec.y);
        //unsigned int h3 = qHash(*(int *)&vec.z);
        //unsigned int h12 = ((h1 << 16) | (h1 >> 16)) ^ h2;
        //return ((h12 << 16) | (h12 >> 16)) ^ h3;
        return 0;
    }

    struct ResultQuad
    {
        Vec3 v[4];
        int i0, i1, i2, i3;

        void setVertices(const Vec3 &a, const Vec3 &b, const Vec3 &c, const Vec3 &d) { v[0] = a; v[1] = b; v[2] = c; v[3] = d; }
        void setIndices(int a, int b, int c, int d) { i0 = a; i1 = b; i2 = c; i3 = d; }
    };

    void sweep(Mesh &m, int ballIndex, ResultQuad &result);

    static Vec3 rotate(const Vec3 &p, const Vec3 &v, float radians)
    {
        Vec3 temp = Vec3(p);

        Vec3 axisX = ((fabsf(v.Dot(Vec3::unitX)) < 0.75) ? Vec3::unitX : Vec3::unitY).Cross(v).Normalized();
        Vec3 axisY = v.Cross(axisX);

        float x = temp.Dot(axisX);
        float y = temp.Dot(axisY);
        float z = temp.Dot(v);
        float sinA = sin(radians);
        float cosA = cos(radians);

        return axisX * (x * cosA - y * sinA) + axisY * (y * cosA + x * sinA) + v * z;
    }

    static void makeStartOfSweep(Mesh &mesh, int ballIndex, Vec3& v0, Vec3& v1, Vec3& v2, Vec3& v3)
    {
        const Ball &ball = mesh.balls[ballIndex];
        //this is an end node. find the local vectors
        Ball parent = mesh.balls.at(ball.parentIndex);
        Vec3 boneDirection = parent.center - ball.center;
        Vec3 x, y, z;
        x = -boneDirection.Normalized();
        y = (fabsf(x.Dot(Vec3::unitY)) < 0.5 ? Vec3::unitY : Vec3::unitX).Cross(x).Normalized();
        z = x.Cross(y).Normalized();

        float r = ball.maxRadius();
        x *= r;
        y *= r;
        z *= r;

        //make the quad cap
        v0 = ball.center + y + z;
        v1 = ball.center - y + z;
        v2 = ball.center - y - z;
        v3 = ball.center + y - z;
    }

    static void makeCap(Mesh &mesh, int ballIndex, ResultQuad &result)
    {
        const Ball &ball = mesh.balls[ballIndex];

        // if we have no parent (and no children, since we are in this method), then just make a box
        if (ball.parentIndex == -1)
        {
            float r = ball.maxRadius();
            Vec3 x, y, z;
            x = Vec3::unitX*r;
            y = Vec3::unitY*r;
            z = Vec3::unitZ*r;

            Vec3 v0, v1, v2, v3, v4, v5, v6, v7;
            v0 = ball.center;
            v1 = ball.center;
            v2 = ball.center;
            v3 = ball.center;
            v4 = ball.center;
            v5 = ball.center;
            v6 = ball.center;
            v7 = ball.center;

            v0 += y + x;
            v0 += z;
            v1 -= y - x;
            v1 += z;
            v2 -= y - x;
            v2 -= z;
            v3 += y + x;
            v3 -= z;
            v4 += y - x;
            v4 += z;
            v5 -= y + x;
            v5 += z;
            v6 -= y + x;
            v6 -= z;
            v7 += y - x;
            v7 -= z;

            int i = mesh.vertices.size();
            mesh.vertices.push_back(Vertex(v0, ballIndex));
            mesh.vertices.push_back(Vertex(v1, ballIndex));
            mesh.vertices.push_back(Vertex(v2, ballIndex));
            mesh.vertices.push_back(Vertex(v3, ballIndex));
            mesh.vertices.push_back(Vertex(v4, ballIndex));
            mesh.vertices.push_back(Vertex(v5, ballIndex));
            mesh.vertices.push_back(Vertex(v6, ballIndex));
            mesh.vertices.push_back(Vertex(v7, ballIndex));

            mesh.quads.push_back(Quad(i, i + 1, i + 2, i + 3));
            mesh.quads.push_back(Quad(i + 7, i + 6, i + 5, i + 4));
            mesh.quads.push_back(Quad(i + 7, i + 4, i, i + 3));
            mesh.quads.push_back(Quad(i + 6, i + 7, i + 3, i + 2));
            mesh.quads.push_back(Quad(i + 6, i + 2, i + 1, i + 5));
            mesh.quads.push_back(Quad(i, i + 4, i + 5, i + 1));

            return;
        }

        //these are the edge of the sphere
        Ball parent = mesh.balls.at(ball.parentIndex);
        Vec3 x = -(parent.center - ball.center).Normalized() * ball.maxRadius();
        Vec3 v0, v1, v2, v3, v4, v5, v6, v7;
        makeStartOfSweep(mesh, ballIndex, v4, v5, v6, v7);
        v0 = v4 + x;
        v1 = v5 + x;
        v2 = v6 + x;
        v3 = v7 + x;

        //add the cap vertices to the mesh, keep track of the start index
        int i = mesh.vertices.size();
        mesh.vertices.push_back(Vertex(v0, ballIndex));
        mesh.vertices.push_back(Vertex(v1, ballIndex));
        mesh.vertices.push_back(Vertex(v2, ballIndex));
        mesh.vertices.push_back(Vertex(v3, ballIndex));
        mesh.vertices.push_back(Vertex(v4, ballIndex));
        mesh.vertices.push_back(Vertex(v5, ballIndex));
        mesh.vertices.push_back(Vertex(v6, ballIndex));
        mesh.vertices.push_back(Vertex(v7, ballIndex));

        //add the quad to the cap
        mesh.quads.push_back(Quad(i, i + 1, i + 2, i + 3));

        //now add the sweep to the center of the sphere
        mesh.quads.push_back(Quad(i + 4, i + 5, i + 1, i));
        mesh.quads.push_back(Quad(i + 5, i + 6, i + 2, i + 1));
        mesh.quads.push_back(Quad(i + 6, i + 7, i + 3, i + 2));
        mesh.quads.push_back(Quad(i + 7, i + 4, i, i + 3));

        result.setVertices(v4, v5, v6, v7);
        result.setIndices(i + 4, i + 5, i + 6, i + 7);
    }

    static void addSegmentedSweep(Mesh &mesh, ResultQuad &startQuad, const Vec3& end0, const Vec3& end1, const Vec3& end2, const Vec3& end3, int startIndex, float endRadius)
    {
        const Ball &startBall = mesh.balls[startIndex];

        float startRadius = startBall.maxRadius();

        Vec3 start = (mesh.vertices[startQuad.i0].pos + mesh.vertices[startQuad.i1].pos + mesh.vertices[startQuad.i2].pos + mesh.vertices[startQuad.i3].pos) / 4;
        Vec3 end = (end0 + end1 + end2 + end3) / 4;
        float startToEnd = (end - start).Length();
        const int divisions = std::max(0.0f, (startToEnd - startRadius - endRadius) / (startRadius + endRadius));
        int i0 = startQuad.i0;
        int i1 = startQuad.i1;
        int i2 = startQuad.i2;
        int i3 = startQuad.i3;

        for (int i = 0; i <= divisions; i++)
        {
            if (i == divisions)
            {
                // special-case the end, which is at a different angle
                mesh.vertices.push_back(Vertex(end0, startIndex));
                mesh.vertices.push_back(Vertex(end1, startIndex));
                mesh.vertices.push_back(Vertex(end2, startIndex));
                mesh.vertices.push_back(Vertex(end3, startIndex));
            }
            else
            {
                // calculate how far along the bone we are, starting from after startRadius and ending before endRadius
                // this will go negative if the spheres are intersecting, but in that case divisions == 0 so it doesn't matter
                float percent = (divisions > 1) ? (float)i / (float)(divisions - 1) : 0;
                float scale = (startRadius + (startToEnd - startRadius - endRadius) * percent) / startToEnd;
                Vec3 offset = start + (end - start) * scale;

                // interpolate the position along the bone, growing or shrinking based on startRadius, endRadius, and how far along the bone we are
                scale = (startRadius + (endRadius - startRadius) * scale) / startRadius;
                mesh.vertices.push_back(Vertex(offset + (startQuad.v[0] - start) * scale, startIndex));
                mesh.vertices.push_back(Vertex(offset + (startQuad.v[1] - start) * scale, startIndex));
                mesh.vertices.push_back(Vertex(offset + (startQuad.v[2] - start) * scale, startIndex));
                mesh.vertices.push_back(Vertex(offset + (startQuad.v[3] - start) * scale, startIndex));
            }

            {
                // generate the quads
                int i = mesh.vertices.size() - 4;
                mesh.quads.push_back(Quad(i, i + 1, i1, i0));
                mesh.quads.push_back(Quad(i + 1, i + 2, i2, i1));
                mesh.quads.push_back(Quad(i + 2, i + 3, i3, i2));
                mesh.quads.push_back(Quad(i + 3, i, i0, i3));

                i0 = i;
                i1 = i + 1;
                i2 = i + 2;
                i3 = i + 3;
            }
        }
    }

    static void makeElbow(Mesh &mesh, int ballIndex, ResultQuad &result)
    {
        const Ball &ball = mesh.balls[ballIndex];
        int childIndex = ball.childrenIndices[0];
        const Ball &child = mesh.balls[childIndex];
        ResultQuad last;
        sweep(mesh, childIndex, last);

        if (ball.parentIndex == -1)
        {
            // we are the root node and we have one child, so cap off the root's end
            float scale = ball.maxRadius() / child.maxRadius();
            Vec3 v[4];
            for (int j = 0; j < 4; j++) {
                v[j] = ball.center + (last.v[j] - child.center) * scale;
            }
            addSegmentedSweep(mesh, last, v[0], v[1], v[2], v[3], childIndex, ball.maxRadius());
            int i = mesh.vertices.size() - 4;
            Vec3 offset = ball.center + (ball.center - child.center).Normalized() * ball.maxRadius();
            for (int j = 0; j < 4; j++) {
                mesh.vertices.push_back(Vertex(offset + (last.v[j] - child.center) * scale, childIndex));
            }
            mesh.quads.push_back(Quad(i, i + 4, i + 5, i + 1));
            mesh.quads.push_back(Quad(i + 1, i + 5, i + 6, i + 2));
            mesh.quads.push_back(Quad(i + 2, i + 6, i + 7, i + 3));
            mesh.quads.push_back(Quad(i + 3, i + 7, i + 4, i));
            mesh.quads.push_back(Quad(i + 4, i + 7, i + 6, i + 5));
            return;
        }

        Ball &parent = mesh.balls[ball.parentIndex];

        // calculate rotation
        Vec3 childDirection = child.center - ball.center;
        Vec3 parentDirection = parent.center - ball.center;
        Vec3 rotationAxis = childDirection.Cross(parentDirection).Normalized();
        float rotationAngle = -acos(-childDirection.Dot(parentDirection) / childDirection.Length() / parentDirection.Length());

        // rotate 50% for elbow
        float scale = ball.maxRadius() / child.maxRadius();
        Vec3 v[4];
        for (int j = 0; j < 4; j++) {
            v[j] = ball.center + rotate(last.v[j] - child.center, rotationAxis, rotationAngle / 2) * scale;
        }
        addSegmentedSweep(mesh, last, v[0], v[1], v[2], v[3], childIndex, ball.maxRadius());

        // rotate 100% for the next step
        for (int j = 0; j < 4; j++) {
            v[j] = ball.center + rotate(last.v[j] - child.center, rotationAxis, rotationAngle) * scale;
        }
        int i = mesh.vertices.size() - 4;
        result.setIndices(i, i + 1, i + 2, i + 3);
        result.setVertices(v[0], v[1], v[2], v[3]);
    }

    static void makeJoint(Mesh &mesh, int ballIndex, ResultQuad &result)
    {
        const Ball &ball = mesh.balls[ballIndex];
        std::vector<Quad> quads;

        for (int k = 0; k < ball.childrenIndices.size(); k++)
        {
            int childIndex = ball.childrenIndices[k];
            const Ball &child = mesh.balls[childIndex];

            ResultQuad last;
            sweep(mesh, childIndex, last);

            // move the quad center from child to ball
            float scale = ball.maxRadius() / child.maxRadius();
            Vec3 v[4];
            for (int j = 0; j < 4; j++) {
                v[j] = ball.center + (child.center - ball.center).Normalized() * ball.maxRadius() + (last.v[j] - child.center) * scale;
            }
            addSegmentedSweep(mesh, last, v[0], v[1], v[2], v[3], childIndex, ball.maxRadius());

            // remember the last quad for convex hull
            int i = mesh.vertices.size() - 4;
            quads.push_back(Quad(i, i + 1, i + 2, i + 3));
        }

        // if there's a parent, we have to do a join using a convex hull
        if (ball.parentIndex != -1)
        {
            // create the quad that will be swept up the parent after this
            int i = mesh.vertices.size();
            Vec3 v0, v1, v2, v3;
            Ball &parent = mesh.balls[ball.parentIndex];
            Vec3 offset = (parent.center - ball.center).Normalized() * ball.maxRadius();
            makeStartOfSweep(mesh, ballIndex, v0, v1, v2, v3);
            mesh.vertices.push_back(Vertex(v0 + offset, ballIndex));
            mesh.vertices.push_back(Vertex(v1 + offset, ballIndex));
            mesh.vertices.push_back(Vertex(v2 + offset, ballIndex));
            mesh.vertices.push_back(Vertex(v3 + offset, ballIndex));
            result.setIndices(i, i + 1, i + 2, i + 3);
            result.setVertices(v0, v1, v2, v3);
            quads.push_back(Quad(i, i + 1, i + 2, i + 3));
        }

        // run the convex hull
        Mesh temp;
        std::map<Vec3, int> indexForVector;
        for (const Quad &quad : quads)
        {
            // read the quad vertices
            Vec3 v0 = mesh.vertices[quad.a.index].pos;
            Vec3 v1 = mesh.vertices[quad.b.index].pos;
            Vec3 v2 = mesh.vertices[quad.c.index].pos;
            Vec3 v3 = mesh.vertices[quad.d.index].pos;

            // hack: temporarily shrink the size of the quad so things are much less likely to intersect
            Vec3 center = (v0 + v1 + v2 + v3) / 4;
            const float percent = 0.9;
            v0 += (center - v0) * percent;
            v1 += (center - v1) * percent;
            v2 += (center - v2) * percent;
            v3 += (center - v3) * percent;

            // add the vertices to the input of the convex hull algorithm
            temp.vertices.push_back(Vertex(v0));
            temp.vertices.push_back(Vertex(v1));
            temp.vertices.push_back(Vertex(v2));
            temp.vertices.push_back(Vertex(v3));

            // remember what maps where for reconstruction
            indexForVector[v0] = quad.a.index;
            indexForVector[v1] = quad.b.index;
            indexForVector[v2] = quad.c.index;
            indexForVector[v3] = quad.d.index;
        }

        // Generate hull
        {
            math::VecArray pts;
            for (const Vertex &vertex : temp.vertices)
                pts.push_back(vertex.pos);

            auto hull = math::Polyhedron::ConvexHull(pts);
            math::TriangleArray hullTriangles = hull.Triangulate();

            //ConvexHull3D::run(&temp);
            for (const math::Triangle& tri : hullTriangles)
            {
                Vec3 v0 = tri.a; // temp.vertices[tri.a.index].pos;
                Vec3 v1 = tri.b; // temp.vertices[tri.b.index].pos;
                Vec3 v2 = tri.c; // temp.vertices[tri.c.index].pos;

                // if we generate more vertices, then some of the old vertices were inside the convex hull
                // we can't deal with that so skip these (don't add any triangles to the mesh with new vertices)
    #define CONTAINS(VEC, VAL) (VEC.find(VAL) != VEC.end())
                if (!CONTAINS(indexForVector, v0) ||
                    !CONTAINS(indexForVector, v1) ||
                    !CONTAINS(indexForVector, v2))
                {
                    //std::cout << "warning: new vertex generated by convex hull, ignoring triangles with this vertex" << endl;
                    continue;
                }

                Triangle tri(indexForVector[v0], indexForVector[v1], indexForVector[v2]);
                bool allOnSameQuad = false;

                // is the triangle all on the same quad?
                for (const Quad &quad : quads)
                {
                    if (quad.a.index != tri.a.index &&
                        quad.b.index != tri.a.index &&
                        quad.c.index != tri.a.index &&
                        quad.d.index != tri.a.index)
                        continue;
                    if (quad.a.index != tri.b.index &&
                        quad.b.index != tri.b.index &&
                        quad.c.index != tri.b.index &&
                        quad.d.index != tri.b.index)
                        continue;
                    if (quad.a.index != tri.c.index &&
                        quad.b.index != tri.c.index &&
                        quad.c.index != tri.c.index &&
                        quad.d.index != tri.c.index)
                        continue;
                    allOnSameQuad = true;
                    break;
                }

                // only add the triangle if it won't be inside the mesh
                if (!allOnSameQuad)
                    mesh.triangles.push_back(tri);
            }
        }
    }

    void sweep(Mesh &mesh, int ballIndex, ResultQuad &result)
    {
        const Ball &ball = mesh.balls[ballIndex];

        if (ball.childrenIndices.empty())
            makeCap(mesh, ballIndex, result);
        else if (ball.childrenIndices.size() == 1)
            makeElbow(mesh, ballIndex, result);
        else
            makeJoint(mesh, ballIndex, result);
    }

    void MeshConstruction::BMeshInit(Mesh* mesh) 
    {
        if (mesh == nullptr)
            return;

        Mesh& m = *mesh;

        ResultQuad result;
        //empty the mesh
        m.vertices.clear();
        m.quads.clear();
        m.triangles.clear();

        //call sweep at each root node
        for (int i = 0; i < m.balls.size(); ++i) {
            if (m.balls[i].parentIndex == -1) {
                sweep(m, i, result);
            }
        }

        TrianglesToQuads::run(&m);
        m.updateNormals();
    }

}