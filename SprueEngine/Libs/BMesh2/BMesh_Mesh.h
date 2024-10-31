#pragma once

#include <SprueEngine/MathGeoLib/AllMath.h>
#include <string>
#include <vector>

#define BALL_DETAIL 16

// 0 = use glBegin() and glEnd() blocks, use for older platforms or if vertex buffers don't work
// 1 = use vertex buffers, will be faster because data stays on the GPU between frames
#define ENABLE_GPU_UPLOAD 0

namespace BMesh
{

struct Ball
{
    SprueEngine::Vec3 center;

    // local coordinate frame of ellipsoid (includes scale factors)
    SprueEngine::Vec3 ex, ey, ez;

    // index into Mesh::balls
    int parentIndex;

    // temporary values used by b_mesh code, are only set immediately before calling
    std::vector<int> childrenIndices;

    Ball() : ex(1, 0, 0), ey(0, 1, 0), ez(0, 0, 1), parentIndex(-1) {}
    Ball(const SprueEngine::Vec3 &center, float radius, int parentIndex = -1) : center(center), ex(radius, 0, 0), ey(0, radius, 0), ez(0, 0, radius), parentIndex(parentIndex) {}

    float minRadius() const { return std::min(std::min(ex.Length(), ey.Length()), ez.Length()); }
    float maxRadius() const { return std::max(std::max(ex.Length(), ey.Length()), ez.Length()); }
    bool isOppositeOf(const Ball &other) const;
    void draw(int detail) const;
};

struct Vertex
{
    SprueEngine::Vec3 pos;
    SprueEngine::Vec3 normal;
    SprueEngine::BoneWeights jointWeights;
    int ball = -1;

    Vertex() {}
    Vertex(const SprueEngine::Vec3 &pos) : pos(pos) {}
    Vertex(const SprueEngine::Vec3 &pos, int jointIndex) : pos(pos) {
        jointWeights.AddBoneWeight(jointIndex, 1);
    }
};

struct Index
{
    // index into Mesh::vertices
    int index;

    // texture coordinate
    SprueEngine::Vec2 coord;

    Index() : index(0) {}
    Index(int index) : index(index) {}
};

struct Triangle
{
    Index a, b, c;

    Triangle() {}
    Triangle(int a, int b, int c) : a(a), b(b), c(c) {}
};

struct Quad
{
    Index a, b, c, d;

    Quad() {}
    Quad(int a, int b, int c, int d) : a(a), b(b), c(c), d(d) {}
};

class Mesh
{
private:
#if ENABLE_GPU_UPLOAD
    unsigned int vertexBuffer;
    unsigned int triangleIndexBuffer;
    unsigned int lineIndexBuffer;
    QVector<Vector3> cachedVertices;
    QVector<int> cachedTriangleIndices;
    QVector<int> cachedLineIndices;
#endif

public:
    std::vector<Ball> balls;
    std::vector<Vertex> vertices;
    std::vector<Triangle> triangles;
    std::vector<Quad> quads;
    int subdivisionLevel;

    Mesh();
    ~Mesh();

    void updateChildIndices();
    void updateNormals();
    void uploadToGPU();

    int getOppositeBall(int index) const;
    void drawPoints() const;
    void drawFill() const;
    void drawWireframe() const;
    void drawKeyBalls(float alpha = 1) const;
    void drawInBetweenBalls() const;
    void drawBones() const;
    // returns a new copy, must be deleted
    Mesh *copy() const;

    bool loadFromOBJ(const std::string &file);
    bool saveToOBJ(const std::string &file);

    static const SprueEngine::Vec3 symmetryFlip;

    static void RunAll(Mesh* mesh, int passes);
};

}