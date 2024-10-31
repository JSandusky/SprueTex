#pragma once

#include <SprueEngine/Math/MathDef.h>
#include <SprueEngine/MathGeoLib/AllMath.h>

#include <vector>
#include <math.h>

namespace Decimate
{
    using namespace SprueEngine;

    struct VertexElement;

    struct DecimationTriangle {
        Vec3 normal;
        float error[4];
        unsigned vertices[3];
        float borderFactor;
        bool deleted;
        bool isDirty;

        DecimationTriangle(std::vector<unsigned> vertices)
        {
            this->vertices[0] = vertices[0];
            this->vertices[1] = vertices[1];
            this->vertices[2] = vertices[2];
            deleted = false;
            isDirty = false;
            borderFactor = 0;
        }

        DecimationTriangle(unsigned a, unsigned b, unsigned c)
        {
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
            deleted = false;
            isDirty = false;
            borderFactor = 0;
        }
    };

    struct QuadraticMatrix {
        float data[10];

        QuadraticMatrix() 
        {
            memset(data, 0, sizeof(float) * 10); 
        }

        QuadraticMatrix(float* data) 
        {
            for (auto i = 0; i < 10; ++i) 
            {
                if (data && data[i])
                    this->data[i] = data[i];
                else 
                    this->data[i] = 0;
            }
        }

        float det(int a11, int a12, int a13, int a21, int a22, int a23, int a31, int a32, int a33) 
        {
            auto det = data[a11] * data[a22] * data[a33] + data[a13] * data[a21] * data[a32] +
                data[a12] * data[a23] * data[a31] - data[a13] * data[a22] * data[a31] -
                data[a11] * data[a23] * data[a32] - data[a12] * data[a21] * data[a33];
            return det;
        }

        QuadraticMatrix& operator+=(const QuadraticMatrix& matrix) 
        {
            for (auto i = 0; i < 10; ++i)
                data[i] += matrix.data[i];
            return *this;
        }

        QuadraticMatrix& operator+=(float* data) 
        {
            for (auto i = 0; i < 10; ++i)
                this->data[i] += data[i];
            return *this;
        }

        QuadraticMatrix operator+(const QuadraticMatrix& matrix) 
        {
            auto m = QuadraticMatrix();
            for (auto i = 0; i < 10; ++i)
                m.data[i] = data[i] + matrix.data[i];
            return m;
        }

        static QuadraticMatrix FromData(float a, float b, float c, float d) {
            QuadraticMatrix ret;
            ret.data[0] = a * a;
            ret.data[1] = a * b;
            ret.data[2] = a * c;
            ret.data[3] = a * d;
            ret.data[4] = b * b;
            ret.data[5] = b * c;
            ret.data[6] = b * d;
            ret.data[7] = c * c;
            ret.data[8] = c * d;
            ret.data[9] = d * d;
            return ret;
        }
    };

    struct DecimationVertex {
        unsigned triangleStart;
        unsigned triangleCount;

        int id;
        Vec3 position;
        Vec3 normal;
        Vec2 uv;

        QuadraticMatrix q;
        bool isBorder;

        DecimationVertex(Vec3 position, Vec3 normal, Vec2 uv, int id) :
            position(position), normal(normal), uv(uv)
        {
            isBorder = true;
            triangleCount = 0;
            triangleStart = 0;
        }
    };

    struct Reference {
        int vertexId;
        int triangleId;

        Reference(int vertexId, int triangleId) :
            vertexId(vertexId), triangleId(triangleId)
        { 
        }

        Reference() : vertexId(0), triangleId(0)
        {

        }
    };

    struct Mesh
    {
        std::vector<float> VertexPositions;
        std::vector<float> VertexNormals;
        std::vector<float> UVCoordinates;
        std::vector<unsigned> indices;

        std::vector<VertexElement*> VertexElements;
        unsigned char positionElementIndex; /// Position element is special, critical to function
        unsigned char normalElementIndex;   /// Normal element is special, almost critical to function
    };

    /// Contains a copy of vertex element data
    struct ScratchBuffer
    {
        unsigned char* data;
        unsigned char dataSize;

        /// Construct, should always be dataSize == vertexStride
        ScratchBuffer(unsigned dataSize) : dataSize(dataSize) { data = new unsigned char[dataSize]; }
        /// Release data block
        ~ScratchBuffer() { delete[] data; }

        void set(unsigned char* data, unsigned offset) { memcpy(this->data, data + offset, dataSize); }

        /// Generic assignment to an element of type T at the given offset
        template<typename T>
        void setElement(const T& elemData, unsigned offset) { *((T*)data + offset) = elemData; }

        /// Generic read of a an element of type T at the given offset
        template<typename T>
        T getElement(unsigned offset) { return *((T*)(data + offset)); }

        /// Marches through N interpolatable elements lerping their values
        template<typename T>
        void lerpSet(unsigned char* lhs, unsigned char* rhs, float deltaT, unsigned offset, unsigned count)
        {
            for (unsigned i = 0; i < size; ++i)
            {
                T left = *((T*)(lhs + offset + i * sizeof(T)));
                T right = *((T*)(rhs + offset + i * sizeof(T)));
                *((T*)(data + offset + i * sizeof(T))) = SprueLerp(left, right, deltaT);
            }
        }
    };

    enum ElementType
    {
        FloatingPoint,
        Integer,
        Char,
        Short,
        UnsignedInteger,
        UnsignedChar,
        UnsignedShort,
        ColorPacked         // RGBA packed into 32 bits
    };

    /// Wrapper around interleaved vertex data for accessing a specific element
    struct VertexElement
    {
        unsigned char* elementData;     // Pointer to the element data
        unsigned short elementOffset;   // Relative offset within the vertex data
        ElementType elementPrimitive;   // Data type contained
        unsigned char elementCount;     // number of components of the data types (Float3, Int4, etc)
        bool interpolated;              // Data should be interpolated whenever possible
        bool normalized;                // Data should be normalized (MUST be a float2, float3, or float4)

        inline unsigned char* getData(unsigned vertexIndex, unsigned vertexStride) const
        {
            return elementData + (vertexIndex * vertexStride);
        }

        inline Vec3 getVec3(unsigned vertexIndex, unsigned vertexStride)
        {
            return Vec3((float*)(elementData + elementOffset + (vertexIndex * vertexStride)));
        }

        inline Vec2 getVec2(unsigned vertexIndex, unsigned vertexStride)
        {
            return Vec2((float*)(elementData + elementOffset + (vertexIndex * vertexStride)));
        }

        template<typename T>
        int getGeneric(unsigned vertexIndex, unsigned vertexStride) {
            return *((T*)(elementData + elementOffset + (vertexIndex * vertexStride)));
        }
    };

    /**
    * An implementation of the Quadratic Error simplification algorithm.
    * Original paper : http://www1.cs.columbia.edu/~cs4162/html05s/garland97.pdf
    * Ported mostly from QSlim and http://voxels.blogspot.de/2014/05/quadric-mesh-simplification-with-source.html
    */
    class MeshSimplification 
    {
    private:
        std::vector<DecimationTriangle*> triangles;
        std::vector<DecimationVertex*> vertices;
        std::vector<Reference> references;
        Mesh* mesh;

        bool initialized;
        int deletedTriangles;

    public:
        MeshSimplification() :
            initialized(false),
            mesh(0x0)
        {
        }

        Mesh* simplify(Mesh* mesh, float quality, float aggressiveness, int iterations) {
            initWithMesh(mesh);
            runDecimation(quality, aggressiveness, iterations);
            return reconstructMesh();
        }

    private:
        void runDecimation(float quality, float aggressiveness, int decimationIterations) 
        {
            int targetCount = (triangles.size() * quality);
            deletedTriangles = 0;

            auto triangleCount = triangles.size();

            for (int i = 0; i < decimationIterations || (triangleCount - deletedTriangles <= targetCount); ++i)
                iterationFunction(i, aggressiveness);
        }

        void iterationFunction(int iteration, float aggressiveness)
        {
            if (iteration % 5 == 0)
                updateMesh(iteration == 0);

            for (auto i = 0; i < triangles.size(); ++i)
                triangles[i]->isDirty = false;

            auto threshold = 0.000000001 * powf((iteration + 3), aggressiveness);

            for (int i = 0; ; ++i)
            {
                auto tIdx = ((triangles.size() / 2) + i) % triangles.size();
                auto t = triangles[i];

                if (!t)
                    return;
                if (t->error[3] > threshold)
                    return;
                if (t->deleted)
                    return;
                if (t->isDirty)
                    return;

                for (auto j = 0; j < 3; ++j) {
                    if (t->error[j] < threshold) {
                        std::vector<bool> deleted0;
                        std::vector<bool> deleted1;

                        auto i0 = t->vertices[j];
                        auto i1 = t->vertices[(j + 1) % 3];
                        auto v0 = vertices[i0];
                        auto v1 = vertices[i1];

                        if (v0->isBorder != v1->isBorder)
                            continue;

                        auto p = Vec3::zero;
                        auto n = Vec3::zero;
                        auto uv = Vec2::zero;

                        calculateError(v0, v1, &p, &n, &uv);

                        if (isFlipped(v0, i1, p, deleted0, t->borderFactor))
                            continue;
                        if (isFlipped(v1, i0, p, deleted1, t->borderFactor))
                            continue;

                        v0->position = p;
                        v0->normal = n;
                        v0->uv = uv;
                        v0->q = v1->q + v0->q;
                        auto tStart = references.size();

                        deletedTriangles = updateTriangles(v0->id, v0, deleted0, deletedTriangles);
                        deletedTriangles = updateTriangles(v0->id, v1, deleted1, deletedTriangles);

                        auto tCount = references.size() - tStart;

                        if (tCount <= v0->triangleCount) 
                        {
                            if (tCount) {
                                for (auto c = 0; c < tCount; c++) 
                                    references[v0->triangleStart + c] = references[tStart + c];
                            }
                        }
                        else
                            v0->triangleStart = tStart;

                        v0->triangleCount = tCount;
                        break;
                    }
                }
            }
        }

        void initWithMesh(Mesh* mesh) 
        {
            if (!mesh) 
                return;
        
            this->mesh = mesh;
            for (int i = 0; i < mesh->VertexPositions.size(); ++i)
                vertices.push_back(new DecimationVertex(
                    Vec3(mesh->VertexPositions.data() + i * 3), 
                    Vec3(mesh->VertexNormals.data() + i * 3), 
                    Vec2(mesh->UVCoordinates.data() + i * 2), i));
        
            for (int i = 0; i < mesh->indices.size(); i += 3)
            {
                int i0 = mesh->indices[i + 0];
                int i1 = mesh->indices[i + 1];
                int i2 = mesh->indices[i + 2];
                triangles.push_back(new DecimationTriangle(vertices[i0]->id, vertices[i1]->id, vertices[i2]->id));
            }

            init();
        }

        void init() 
        {
            for (auto t : triangles)
            {
                t->normal = (vertices[t->vertices[1]]->position - (vertices[t->vertices[0]]->position)).Cross(vertices[t->vertices[2]]->position - (vertices[t->vertices[0]]->position)).Normalized();
                for (auto j = 0; j < 3; j++)
                    vertices[t->vertices[j]]->q += QuadraticMatrix::FromData(t->normal.x, t->normal.y, t->normal.z, -(t->normal.Dot(vertices[t->vertices[0]]->position)));
            }
        
            for (auto t : triangles)
            {
                for (auto j = 0; j < 3; ++j)
                    t->error[j] = calculateError(vertices[t->vertices[j]], vertices[t->vertices[(j + 1) % 3]]);
                t->error[3] = SprueMin(t->error[0], SprueMin(t->error[1], t->error[2]));
            }

            initialized = true;
        }

        Mesh* reconstructMesh() 
        {
            std::vector<DecimationTriangle*> newTriangles;

            for (auto i = 0; i < vertices.size(); ++i) 
                vertices[i]->triangleCount = 0;

            for (auto i = 0; i < triangles.size(); ++i) 
            {
                if (!triangles[i]->deleted) {
                    auto t = triangles[i];
                    for (auto j = 0; j < 3; ++j) {
                        vertices[t->vertices[j]]->triangleCount = 1;
                    }
                    newTriangles.push_back(t);
                }
            }

            std::vector<unsigned> newVerticesOrder;

            //compact vertices, get the IDs of the vertices used.
            auto dst = 0;
            for (auto i = 0; i < vertices.size(); ++i) 
            {
                if (vertices[i]->triangleCount) 
                {
                    vertices[i]->triangleStart = dst;
                    vertices[dst]->position = vertices[i]->position;
                    vertices[dst]->normal = vertices[i]->normal;
                    vertices[dst]->uv = vertices[i]->uv;
                    newVerticesOrder.push_back(i);
                    dst++;
                }
            }

            for (auto i = 0; i < newTriangles.size(); ++i) 
            {
                auto t = newTriangles[i];
                for (auto j = 0; j < 3; ++j)
                    t->vertices[j] = vertices[t->vertices[j]]->triangleStart;
            }

            vertices = std::vector<DecimationVertex*>(vertices.begin(), vertices.begin() + dst);

            std::vector<float> newPositionData(vertices.size());
            std::vector<float> newNormalData(vertices.size());
            std::vector<float> newUVsData(vertices.size());

            for (auto i = 0; i < newVerticesOrder.size(); ++i)
            {
                newPositionData.push_back(vertices[i]->position.x);
                newPositionData.push_back(vertices[i]->position.y);
                newPositionData.push_back(vertices[i]->position.z);
                newNormalData.push_back(vertices[i]->normal.x);
                newNormalData.push_back(vertices[i]->normal.y);
                newNormalData.push_back(vertices[i]->normal.z);
                newUVsData.push_back(vertices[i]->uv.x);
                newUVsData.push_back(vertices[i]->uv.y);
            }

            std::vector<unsigned> newIndicesArray(newTriangles.size() * 3);
            for (auto i = 0; i < newTriangles.size(); ++i) 
            {
                newIndicesArray.push_back(newTriangles[i]->vertices[0]);
                newIndicesArray.push_back(newTriangles[i]->vertices[1]);
                newIndicesArray.push_back(newTriangles[i]->vertices[2]);
            }

            Mesh* newMesh = new Mesh();
            newMesh->indices = newIndicesArray;
            newMesh->VertexPositions = newPositionData;
            newMesh->VertexNormals = newNormalData;
            newMesh->UVCoordinates = newUVsData;
            //preparing the skeleton support
            //JRS if (_mesh->skeleton) {
            //JRS     //newMesh.skeleton = this._mesh.skeleton.clone("", "");
            //JRS     //newMesh.getScene().beginAnimation(newMesh.skeleton, 0, 100, true, 1.0);
            //JRS }

            //JRS return newMesh;
            return newMesh;
        }

        bool isFlipped(DecimationVertex* vertex1, int index2, Vec3 point, std::vector<bool> deletedArray, float borderFactor)
        {
            for (auto i = 0; i < vertex1->triangleCount; ++i)
            {
                auto t = triangles[references[vertex1->triangleStart + i].triangleId];
                if (t->deleted)
                    continue;

                auto s = references[vertex1->triangleStart + i].vertexId;

                auto id1 = t->vertices[(s + 1) % 3];
                auto id2 = t->vertices[(s + 2) % 3];

                if ((id1 == index2 || id2 == index2) && borderFactor < 2) {
                    deletedArray[i] = true;
                    continue;
                }

                auto d1 = (vertices[id1]->position - (point)).Normalized();
                auto d2 = (vertices[id2]->position - (point)).Normalized();
                if (abs(d1.Dot(d2)) > 0.999f)
                    return true;

                auto normal = d1.Cross(d2).Normalized();
                deletedArray[i] = false;

                if (normal.Dot(t->normal) < 0.2) 
                    return true;
            }

            return false;
        }

        int updateTriangles(int vertexId, DecimationVertex* vertex, std::vector<bool> deletedArray, int deletedTriangles) 
        {
            auto newDeleted = deletedTriangles;
            for (auto i = 0; i < vertex->triangleCount; ++i) 
            {
                auto ref = references[vertex->triangleStart + i];
                auto t = triangles[ref.triangleId];
                if (t->deleted)
                    continue;

                if (deletedArray[i]) 
                {
                    t->deleted = true;
                    newDeleted++;
                    continue;
                }
                t->vertices[ref.vertexId] = vertexId;
                t->isDirty = true;
                t->error[0] = calculateError(vertices[t->vertices[0]], vertices[t->vertices[1]]) + (t->borderFactor / 2.0f);
                t->error[1] = calculateError(vertices[t->vertices[1]], vertices[t->vertices[2]]) + (t->borderFactor / 2.0f);
                t->error[2] = calculateError(vertices[t->vertices[2]], vertices[t->vertices[0]]) + (t->borderFactor / 2.0f);
                t->error[3] = SprueMin(t->error[0], SprueMin(t->error[1], t->error[2]));
                references.push_back(ref);
            }
            return newDeleted;
        }

        void identifyBorder() 
        {
            for (auto i = 0; i < vertices.size(); ++i) 
            {
                std::vector<int> vCount;
                std::vector<int> vId;
                auto v = vertices[i];
                for (auto j = 0; j < v->triangleCount; ++j)
                {
                    auto triangle = triangles[references[v->triangleStart + j].triangleId];
                    for (auto ii = 0; ii < 3; ii++) 
                    {
                        auto ofs = 0;
                        auto id = triangle->vertices[ii];
                        while (ofs < vCount.size()) 
                        {
                            if (vId[ofs] == id) 
                                break;
                            ++ofs;
                        }
                        if (ofs == vCount.size()) 
                        {
                            vCount.push_back(1);
                            vId.push_back(id);
                        }
                        else 
                            vCount[ofs]++;
                    }
                }

                for (auto j = 0; j < vCount.size(); ++j) 
                {
                    if (vCount[j] == 1)
                        vertices[vId[j]]->isBorder = true;
                    else
                        vertices[vId[j]]->isBorder = false;
                }

            }
        }

        void updateMesh(bool identifyBorders = false) 
        {
            if (!identifyBorders) 
            {
                auto dst = 0;
                std::vector<DecimationTriangle*> newTrianglesVector;
                for (auto i = 0; i < triangles.size(); ++i)
                {
                    if (!triangles[i]->deleted)
                        newTrianglesVector.push_back(triangles[i]);
                }
                triangles = newTrianglesVector;
            }

            for (auto i = 0; i < vertices.size(); ++i) 
            {
                vertices[i]->triangleCount = 0;
                vertices[i]->triangleStart = 0;
            }

            for (auto i = 0; i < triangles.size(); ++i) 
            {
                auto t = triangles[i];
                for (auto j = 0; j < 3; ++j) {
                    auto v = vertices[t->vertices[j]];
                    v->triangleCount++;
                }
            }

            auto tStart = 0;

            for (auto i = 0; i < vertices.size(); ++i) 
            {
                vertices[i]->triangleStart = tStart;
                tStart += vertices[i]->triangleCount;
                vertices[i]->triangleCount = 0;
            }

            std::vector<Reference> newReferences;
            newReferences.resize(triangles.size() * 3);
            for (auto i = 0; i < triangles.size(); ++i) 
            {
                auto t = triangles[i];
                for (auto j = 0; j < 3; ++j) 
                {
                    auto v = vertices[t->vertices[j]];
                    auto& ref = newReferences[v->triangleStart + v->triangleCount];
                    ref.vertexId = j; 
                    ref.triangleId = i;
                    v->triangleCount++;
                }
            }
            references = newReferences;

            if (identifyBorders) 
            {
                identifyBorder();
            }
        }


        float vertexError(const QuadraticMatrix& q, Vec3 point) 
        {
            auto x = point.x;
            auto y = point.y;
            auto z = point.z;
            return q.data[0] * x * x + 2 * q.data[1] * x * y + 2 * q.data[2] * x * z + 2 * q.data[3] * x + q.data[4] * y * y
                + 2 * q.data[5] * y * z + 2 * q.data[6] * y + q.data[7] * z * z + 2 * q.data[8] * z + q.data[9];
        }

        float calculateError(DecimationVertex* vertex1, DecimationVertex* vertex2, Vec3* pointResult = 0x0, Vec3* normalResult = 0x0, Vec2* uvResult = 0x0) {
            auto q = vertex1->q + vertex2->q;
            auto border = vertex1->isBorder && vertex2->isBorder;
            auto error = 0;
            auto qDet = q.det(0, 1, 2, 1, 4, 5, 2, 5, 7);

            if (qDet != 0 && !border) 
            {
                if (pointResult) {
                    pointResult->x = -1 / qDet * (q.det(1, 2, 3, 4, 5, 6, 5, 7, 8));
                    pointResult->y = 1 / qDet * (q.det(0, 2, 3, 1, 5, 6, 2, 7, 8));
                    pointResult->z = -1 / qDet * (q.det(0, 1, 3, 1, 4, 6, 2, 5, 8));
                    error = vertexError(q, *pointResult);
                    //TODO this should be correctly calculated
                    if (normalResult)
                        *normalResult = vertex1->normal;
                    if (uvResult)
                        *uvResult = vertex1->uv;
                }
            }
            else 
            {
                auto p3 = (vertex1->position + vertex2->position) / Vec3(2.0f, 2.0f, 2.0f);
                auto norm3 = ((vertex1->normal + vertex2->normal) / Vec3(2.0f, 2.0f, 2.0f)).Normalized();
                
                auto error1 = vertexError(q, vertex1->position);
                auto error2 = vertexError(q, vertex2->position);
                auto error3 = vertexError(q, p3);
                float error = SprueMin(error1, SprueMin(error2, error3));
                
                if (SprueEquals(error, error1)) 
                {
                    if (pointResult) 
                        *pointResult = (vertex1->position);
                    if (normalResult)
                        *normalResult = (vertex1->normal);
                    if (uvResult)
                        *uvResult = (vertex1->uv);
                }
                else if (SprueEquals(error, error2))
                {
                    if (pointResult) 
                        *pointResult = (vertex2->position);
                    if (normalResult)
                        *normalResult = (vertex2->normal);
                    if (uvResult)
                        *uvResult = (vertex2->uv);
                }
                else 
                {
                    if (pointResult) 
                        *pointResult = (p3);
                    if (normalResult)
                        *normalResult = (norm3);
                    if (uvResult)
                        *uvResult = (vertex1->uv);
                }
            }
            return error;
        }
    };
}