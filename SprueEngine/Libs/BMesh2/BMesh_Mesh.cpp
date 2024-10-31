#include "BMesh_Mesh.h"

#include "BMesh_EdgeFairing.h"
#include "BMesh_Evolution.h"
#include "BMesh_MeshConstruction.h"
#include "BMesh_Subdivision.h"
#include "BMesh_TrianglesToQuads.h"

#include <algorithm>

using namespace SprueEngine;

namespace BMesh
{

#define COMPILE_TIME_ASSERT(pred) switch(0){case 0:case pred:;}
#define BUFFER_OFFSET(i) ((char *)NULL + (i))

    typedef std::pair<int, int> Edge;

    const Vec3 Mesh::symmetryFlip(-1, 1, 1);

    inline void addEdge(std::set<Edge> &edges, int a, int b)
    {
        edges.insert(Edge(std::min(a, b), std::max(a, b)));
    }

    bool Ball::isOppositeOf(const Ball &other) const
    {
        const float epsilon = 1.0e-8f;
        return (center - other.center * Mesh::symmetryFlip).LengthSq() < epsilon &&
            fabsf(minRadius() - other.minRadius()) < epsilon &&
            fabsf(maxRadius() - other.maxRadius()) < epsilon;
    }

    void Ball::draw(int detail) const
    {
        float matrix[16] = {
            ex.x, ex.y, ex.z, 0,
            ey.x, ey.y, ey.z, 0,
            ez.x, ez.y, ez.z, 0,
            0, 0, 0, 1
        };
        //TODO glPushMatrix();
        //TODO glTranslatef(center.x, center.y, center.z);
        //TODO glMultMatrixf(matrix);
        //TODO drawSphere(detail);
        //TODO glPopMatrix();
    }

    Mesh::Mesh()
#if ENABLE_GPU_UPLOAD
        : vertexBuffer(0), triangleIndexBuffer(0), lineIndexBuffer(0)
#endif
    {
        subdivisionLevel = 0;
    }

    Mesh::~Mesh()
    {
#if ENABLE_GPU_UPLOAD
        glDeleteBuffersARB(1, &vertexBuffer);
        glDeleteBuffersARB(1, &triangleIndexBuffer);
        glDeleteBuffersARB(1, &lineIndexBuffer);
#endif
    }

    void Mesh::updateChildIndices()
    {
        for (int i = 0; i < balls.size(); i++)
        {
            Ball &ball = balls[i];
            ball.childrenIndices.clear();
        }

        for (int i = 0; i < balls.size(); i++)
        {
            Ball &ball = balls[i];
            if (ball.parentIndex != -1)
                balls[ball.parentIndex].childrenIndices.push_back(i);
        }
    }

    void Mesh::updateNormals()
    {
        for (int i = 0; i < vertices.size(); i++)
        {
            Vertex &vertex = vertices[i];
            vertex.normal = Vec3();
        }

        for (int i = 0; i < triangles.size(); i++)
        {
            Triangle &tri = triangles[i];
            Vertex &a = vertices[tri.a.index];
            Vertex &b = vertices[tri.b.index];
            Vertex &c = vertices[tri.c.index];
            Vec3 normal = (b.pos - a.pos).Cross(c.pos - a.pos).Normalized();
            a.normal += normal;
            b.normal += normal;
            c.normal += normal;
        }

        for (int i = 0; i < quads.size(); i++)
        {
            Quad &quad = quads[i];
            Vertex &a = vertices[quad.a.index];
            Vertex &b = vertices[quad.b.index];
            Vertex &c = vertices[quad.c.index];
            Vertex &d = vertices[quad.d.index];
            Vec3 normal = (
                (b.pos - a.pos).Cross(d.pos - a.pos) +
                (c.pos - b.pos).Cross(a.pos - b.pos) +
                (d.pos - c.pos).Cross(b.pos - c.pos) +
                (a.pos - d.pos).Cross(c.pos - d.pos)
                ).Normalized();
            a.normal += normal;
            b.normal += normal;
            c.normal += normal;
            d.normal += normal;
        }

        for (int i = 0; i < vertices.size(); i++)
        {
            Vertex &vertex = vertices[i];
            vertex.normal.Normalize();
        }
    }

    void Mesh::uploadToGPU()
    {
#if ENABLE_GPU_UPLOAD
        if (triangles.count() + quads.count() > 0)
        {
            QSet<Edge> edges;

            COMPILE_TIME_ASSERT(sizeof(Vector3) == sizeof(float) * 3);

            cachedVertices.clear();
            cachedTriangleIndices.clear();
            cachedLineIndices.clear();

            foreach(const Vertex &vertex, vertices)
            {
                cachedVertices += vertex.pos;
                cachedVertices += vertex.normal;
            }

            foreach(const Triangle &tri, triangles)
            {
                cachedTriangleIndices += tri.a.index;
                cachedTriangleIndices += tri.b.index;
                cachedTriangleIndices += tri.c.index;

                addEdge(edges, tri.a.index, tri.b.index);
                addEdge(edges, tri.b.index, tri.c.index);
                addEdge(edges, tri.c.index, tri.a.index);
            }

            foreach(const Quad &quad, quads)
            {
                cachedTriangleIndices += quad.a.index;
                cachedTriangleIndices += quad.b.index;
                cachedTriangleIndices += quad.c.index;

                cachedTriangleIndices += quad.a.index;
                cachedTriangleIndices += quad.c.index;
                cachedTriangleIndices += quad.d.index;

                addEdge(edges, quad.a.index, quad.b.index);
                addEdge(edges, quad.b.index, quad.c.index);
                addEdge(edges, quad.c.index, quad.d.index);
                addEdge(edges, quad.d.index, quad.a.index);
            }

            foreach(const Edge &edge, edges)
            {
                cachedLineIndices += edge.first;
                cachedLineIndices += edge.second;
            }

            if (!vertexBuffer) glGenBuffersARB(1, &vertexBuffer);
            glBindBufferARB(GL_ARRAY_BUFFER_ARB, vertexBuffer);
            glBufferDataARB(GL_ARRAY_BUFFER_ARB, cachedVertices.count() * sizeof(Vector3), &cachedVertices[0], GL_STATIC_DRAW_ARB);
            glBindBufferARB(GL_ARRAY_BUFFER_ARB, 0);

            if (!triangleIndexBuffer) glGenBuffersARB(1, &triangleIndexBuffer);
            glBindBufferARB(GL_ELEMENT_ARRAY_BUFFER_ARB, triangleIndexBuffer);
            glBufferDataARB(GL_ELEMENT_ARRAY_BUFFER_ARB, cachedTriangleIndices.count() * sizeof(int), &cachedTriangleIndices[0], GL_STATIC_DRAW_ARB);
            glBindBufferARB(GL_ELEMENT_ARRAY_BUFFER_ARB, 0);

            if (!lineIndexBuffer) glGenBuffersARB(1, &lineIndexBuffer);
            glBindBufferARB(GL_ELEMENT_ARRAY_BUFFER_ARB, lineIndexBuffer);
            glBufferDataARB(GL_ELEMENT_ARRAY_BUFFER_ARB, cachedLineIndices.count() * sizeof(int), &cachedLineIndices[0], GL_STATIC_DRAW_ARB);
            glBindBufferARB(GL_ELEMENT_ARRAY_BUFFER_ARB, 0);
        }
#endif
    }

    void Mesh::drawKeyBalls(float alpha) const
    {
        for (const Ball &ball : balls)
        {
            //TODO if (ball.parentIndex == -1)
            //TODO     glColor4f(0.75, 0, 0, alpha);
            //TODO else
            //TODO     glColor4f(0, 0.5, 1, alpha);
            ball.draw(BALL_DETAIL);
        }
    }

    void Mesh::drawInBetweenBalls() const
    {
        for (const Ball &ball : balls)
        {
            // get the parent ball
            if (ball.parentIndex == -1) continue;
            const Ball &parent = balls[ball.parentIndex];

            // decide how many in-between balls to generate
            float totalRadius = ball.maxRadius() + parent.maxRadius();
            float edgeLength = (ball.center - parent.center).Length();
            const int count = std::min(100.0f, ceilf(edgeLength / totalRadius * 4));

            // generate in-between balls
            for (int i = 1; i < count; i++)
            {
                float percent = (float)i / (float)count;
                Ball tween;
                tween.center = Vec3::Lerp(ball.center, parent.center, percent);
                tween.ex = Vec3::Lerp(ball.ex, parent.ex, percent);
                tween.ey = Vec3::Lerp(ball.ey, parent.ey, percent);
                tween.ez = Vec3::Lerp(ball.ez, parent.ez, percent);
                tween.draw(BALL_DETAIL);
            }
        }
    }

    void Mesh::drawBones() const
    {
        // draw bones as rotated and scaled cylinders
        for (const Ball &ball : balls)
        {
            if (ball.parentIndex == -1) continue;
            const Ball &parent = balls[ball.parentIndex];

            // calculate an appropriate radius based on the minimum ball size
            float radius = std::min(ball.minRadius(), parent.minRadius()) / 4;

            Vec3 delta = ball.center - parent.center;
            //TODO Vec2 angles = delta.toAngles();
            //TODO glPushMatrix();
            //TODO glTranslatef(parent.center.x, parent.center.y, parent.center.z);
            //TODO glRotatef(90 - angles.x * 180 / M_PI, 0, 1, 0);
            //TODO glRotatef(-angles.y * 180 / M_PI, 1, 0, 0);
            //TODO glScalef(radius, radius, delta.length());
            //TODO drawCylinder(BALL_DETAIL);
            //TODO glPopMatrix();
        }
    }

    int Mesh::getOppositeBall(int index) const
    {
        const Ball &ball = balls[index];
        int oppositeIndex = -1;
        for (int i = 0; i < balls.size(); i++)
        {
            const Ball &opposite = balls[i];
            if (i != index && ball.isOppositeOf(opposite))
                oppositeIndex = i;
        }
        return oppositeIndex;
    }

    void Mesh::drawPoints() const
    {
#if ENABLE_GPU_UPLOAD
        if (vertexBuffer && triangleIndexBuffer)
        {
            glEnableClientState(GL_VERTEX_ARRAY);
            glBindBufferARB(GL_ARRAY_BUFFER_ARB, vertexBuffer);
            glVertexPointer(3, GL_FLOAT, sizeof(Vector3) * 2, BUFFER_OFFSET(0));
            glDrawArrays(GL_POINTS, 0, cachedVertices.count() / 2);
            glBindBufferARB(GL_ARRAY_BUFFER_ARB, 0);
            glDisableClientState(GL_VERTEX_ARRAY);
        }
        else
#endif
        {
            //TODO glBegin(GL_POINTS);
            //TODO foreach(const Vertex &vertex, vertices)
            //TODO     glVertex3fv(vertex.pos.xyz);
            //TODO glEnd();
        }
    }

    void Mesh::drawFill() const
    {
#if ENABLE_GPU_UPLOAD
        if (vertexBuffer && triangleIndexBuffer)
        {
            glEnableClientState(GL_VERTEX_ARRAY);
            glEnableClientState(GL_NORMAL_ARRAY);
            glBindBufferARB(GL_ARRAY_BUFFER_ARB, vertexBuffer);
            glVertexPointer(3, GL_FLOAT, sizeof(Vector3) * 2, BUFFER_OFFSET(0));
            glNormalPointer(GL_FLOAT, sizeof(Vector3) * 2, BUFFER_OFFSET(sizeof(Vector3)));
            glBindBufferARB(GL_ELEMENT_ARRAY_BUFFER_ARB, triangleIndexBuffer);
            glDrawElements(GL_TRIANGLES, cachedTriangleIndices.count(), GL_UNSIGNED_INT, BUFFER_OFFSET(0));
            glBindBufferARB(GL_ARRAY_BUFFER_ARB, 0);
            glBindBufferARB(GL_ELEMENT_ARRAY_BUFFER_ARB, 0);
            glDisableClientState(GL_VERTEX_ARRAY);
            glDisableClientState(GL_NORMAL_ARRAY);
        }
        else
#endif
        {
            //TODO glBegin(GL_TRIANGLES);
            //TODO foreach(const Triangle &tri, triangles)
            //TODO {
            //TODO     vertices[tri.a.index].draw();
            //TODO     vertices[tri.b.index].draw();
            //TODO     vertices[tri.c.index].draw();
            //TODO }
            //TODO glEnd();
            //TODO glBegin(GL_QUADS);
            //TODO foreach(const Quad &quad, quads)
            //TODO {
            //TODO     vertices[quad.a.index].draw();
            //TODO     vertices[quad.b.index].draw();
            //TODO     vertices[quad.c.index].draw();
            //TODO     vertices[quad.d.index].draw();
            //TODO }
            //TODO glEnd();
        }
#if ANIM_DEBUG
        glDisable(GL_DEPTH_TEST);
        glBegin(GL_LINES);
        foreach(const Vertex &vertex, vertices) {
            if (vertex.jointIndices[0] != -1) {
                glColor3f(vertex.jointWeights[0], 0, 0);
                glVertex3fv(vertex.pos.xyz);
                glVertex3fv(balls[vertex.jointIndices[0]].center.xyz);
            }
            if (vertex.jointIndices[1] != -1) {
                glColor3f(0, vertex.jointWeights[1], 0);
                glVertex3fv(vertex.pos.xyz);
                glVertex3fv(balls[vertex.jointIndices[1]].center.xyz);
            }
        }
        glEnd();
        glEnable(GL_DEPTH_TEST);
#endif
    }

    void Mesh::drawWireframe() const
    {
#if ENABLE_GPU_UPLOAD
        if (vertexBuffer && lineIndexBuffer)
        {
            glEnableClientState(GL_VERTEX_ARRAY);
            glEnableClientState(GL_NORMAL_ARRAY);
            glBindBufferARB(GL_ARRAY_BUFFER_ARB, vertexBuffer);
            glVertexPointer(3, GL_FLOAT, sizeof(Vector3) * 2, BUFFER_OFFSET(0));
            glNormalPointer(GL_FLOAT, sizeof(Vector3) * 2, BUFFER_OFFSET(sizeof(Vector3)));
            glBindBufferARB(GL_ELEMENT_ARRAY_BUFFER_ARB, lineIndexBuffer);
            glDrawElements(GL_LINES, cachedLineIndices.count(), GL_UNSIGNED_INT, BUFFER_OFFSET(0));
            glBindBufferARB(GL_ARRAY_BUFFER_ARB, 0);
            glBindBufferARB(GL_ELEMENT_ARRAY_BUFFER_ARB, 0);
            glDisableClientState(GL_VERTEX_ARRAY);
            glDisableClientState(GL_NORMAL_ARRAY);
        }
        else
#endif
        {
            //TODO foreach(const Triangle &tri, triangles)
            //TODO {
            //TODO     glBegin(GL_LINE_LOOP);
            //TODO     vertices[tri.a.index].draw();
            //TODO     vertices[tri.b.index].draw();
            //TODO     vertices[tri.c.index].draw();
            //TODO     glEnd();
            //TODO }
            //TODO foreach(const Quad &quad, quads)
            //TODO {
            //TODO     glBegin(GL_LINE_LOOP);
            //TODO     vertices[quad.a.index].draw();
            //TODO     vertices[quad.b.index].draw();
            //TODO     vertices[quad.c.index].draw();
            //TODO     vertices[quad.d.index].draw();
            //TODO     glEnd();
            //TODO }
        }
    }


    Mesh * Mesh::copy() const
    {
        Mesh *copy = new Mesh();
        copy->subdivisionLevel = subdivisionLevel;
        for (const Ball &ball : balls)
        {
            copy->balls.push_back(ball);
        }
        for (const Vertex &vert : vertices)
        {
            copy->vertices.push_back(vert);
        }
        for (const Triangle &tri : triangles)
        {
            copy->triangles.push_back(tri);
        }
        for (const Quad &quad : quads)
        {
            copy->quads.push_back(quad);
        }

        return copy;
    }

    void Mesh::RunAll(Mesh* mesh, int passes)
    {
        mesh->updateChildIndices();
        MeshConstruction::BMeshInit(mesh);
        for (int i = 0; i < passes; ++i)
            CatmullMesh::subdivide(*mesh);
        MeshEvolution::run(*mesh);
        EdgeFairing::run(mesh, 15, 0.15f);
    }
}