// This code is in the public domain -- castanyo@yahoo.es

#include "MeshBuilder.h"
#include "TriMesh.h"
#include "QuadTriMesh.h"
#include "halfedge/Mesh.h"
#include "halfedge/Vertex.h"
#include "halfedge/Face.h"

#include "weld/Weld.h"

#include "nvmath/Box.h"
//#include "nvmath/Vector.inl"

//#include "nvcore/StrLib.h"
#include "nvcore/RadixSort.h"
//#include "nvcore/Ptr.h"
//#include "nvcore/Array.inl"
//#include "nvcore/HashMap.inl"

#include <algorithm>
#include <string>
#include <unordered_map>

using namespace SprueEngine;
using namespace nv;

/*
By default the mesh builder creates 3 streams (position, normal, texcoord), I'm planning to add support for extra streams as follows:

enum StreamType { StreamType_Float, StreamType_Vec2, StreamType_Vec3, StreamType_Vec4 };

uint addStream(const char *, uint idx, StreamType);

uint addAttribute(float)
uint addAttribute(Vec2)
uint addAttribute(Vec3)
uint addAttribute(Vec4)

struct Vertex
{
    uint pos;
    uint nor;
    uint tex;
    uint * attribs;	// NULL or NIL terminated array?
};

All streams must be added before hand, so that you know the size of the attribs array.

The vertex hash function could be kept as is, but the == operator should be extended to test 
the extra atributes when available.

That might require a custom hash implementation, or an extension of the current one. How to
handle the variable number of attributes in the attribs array?

bool operator()(const Vertex & a, const Vertex & b) const
{ 
    if (a.pos != b.pos || a.nor != b.nor || a.tex != b.tex) return false;
    if (a.attribs == NULL && b.attribs == NULL) return true;
    return 0 == memcmp(a.attribs, b.attribs, ???);
}

We could use a NIL terminated array, or provide custom user data to the equals functor.

vertexMap.setUserData((void *)vertexAttribCount);

bool operator()(const Vertex & a, const Vertex & b, void * userData) const { ... }

*/

#define VECTOR_POP_BACK(V) (V.pop_back())

#define VECTOR_REMOVE_AT(V, I) (V.erase(V.begin() + I))

namespace 
{
    struct Material
    {
        Material() : faceCount(0) {}
        Material(const std::string & str) : name(str), faceCount(0) {}

        std::string name;
        uint32_t faceCount;
    };

    struct Vertex
    {
        //Vertex() {}
        //Vertex(uint p, uint n, uint t0, uint t1, uint c) : pos(p), nor(n), tex0(t0), tex1(t1), col(c) {}

        friend bool operator==(const Vertex & a, const Vertex & b)
        {
            return a.pos == b.pos && a.nor == b.nor && a.tex[0] == b.tex[0] && a.tex[1] == b.tex[1] && a.col[0] == b.col[0] && a.col[1] == b.col[1] && a.col[2] == b.col[2];
        }

        uint32_t pos;
        uint32_t nor;
        uint32_t tex[2];
        uint32_t col[3];
    };

    struct Face
    {
        uint32_t id;
        uint32_t firstIndex;
        uint32_t indexCount;
        uint32_t material;
        uint32_t group;
    };

} // namespace


namespace std
{
    inline uint32_t sdbmHash(const void * data_in, uint32_t size, uint32_t h = 5381)
    {
        const uint8_t * data = (const uint8_t *)data_in;
        uint32_t i = 0;
        while (i < size) {
            h = (h << 16) + (h << 6) - h + (uint32_t)data[i++];
        }
        return h;
    }

    inline uint32_t sdbmFloatHash(const float * f, uint32_t count, uint32_t h = 5381)
    {
        for (uint32_t i = 0; i < count; i++) {
            //nvDebugCheck(nv::isFinite(*f));
            union { float f; uint32_t i; } x = { f[i] };
            if (x.i == 0x80000000) x.i = 0;
            h = sdbmHash(&x, 4, h);
        }
        return h;
    }

    // This is a much better hash than the default and greatly improves performance!
    template <> struct hash<Vertex>
    {
        uint32_t operator()(const Vertex & v) const { return v.pos + v.nor + v.tex[0]/* + v.col*/; }
    };

    template <> struct hash<SprueEngine::Vec2>
    {
        uint32_t operator()(const Vec2& v) const { return sdbmFloatHash(v.ptr(), 2); }
    };

    template <> struct hash<SprueEngine::Vec3>
    {
        uint32_t operator()(const Vec3& v) const { return sdbmFloatHash(v.ptr(), 3); }
    };

    template <> struct hash<SprueEngine::Vec4>
    {
        uint32_t operator()(const Vec4& v) const { return sdbmFloatHash(v.ptr(), 4); }
    };

    template <> struct equal_to<Vec2> { bool operator()(const Vec2& lhs, const Vec2& rhs) const { return lhs.Equals(rhs); } };
    template <> struct equal_to<Vec3> { bool operator()(const Vec3& lhs, const Vec3& rhs) const { return lhs.Equals(rhs); } };
    template <> struct equal_to<Vec4> { bool operator()(const Vec4& lhs, const Vec4& rhs) const { return lhs.Equals(rhs); } };
}

struct MeshBuilder::PrivateData
{
    PrivateData() : currentGroup(NIL), currentMaterial(NIL), maxFaceIndexCount(NIL) {}

    uint32_t pushVertex(uint32_t p, uint32_t n, uint32_t t0, uint32_t t1, uint32_t c0, uint32_t c1, uint32_t c2);
    uint32_t pushVertex(const Vertex & v);

    std::vector<Vec3> posArray;
    std::vector<Vec3> norArray;
    std::vector<Vec2> texArray[2];
    std::vector<Vec4> colArray[3];

    std::vector<Vertex> vertexArray;
    std::unordered_map<Vertex, uint32_t> vertexMap;

    std::unordered_map<std::string, uint32_t> materialMap;
    std::vector<Material> materialArray;

    uint32_t currentGroup;
    uint32_t currentMaterial;

    std::vector<uint32_t> indexArray;
    std::vector<Face> faceArray;

    uint32_t maxFaceIndexCount;
};


uint32_t MeshBuilder::PrivateData::pushVertex(uint32_t p, uint32_t n, uint32_t t0, uint32_t t1, uint32_t c0, uint32_t c1, uint32_t c2)
{
    Vertex v;
    v.pos = p;
    v.nor = n;
    v.tex[0] = t0;
    v.tex[1] = t1;
    v.col[0] = c0;
    v.col[1] = c1;
    v.col[2] = c2;
    return pushVertex(v);
}

uint32_t MeshBuilder::PrivateData::pushVertex(const Vertex & v)
{
    // Lookup vertex v in map.
    uint32_t idx;
    auto found = vertexMap.find(v);
    if (found != vertexMap.end())
        return found->second;

    idx = vertexArray.size();
    vertexArray.push_back(v);
    vertexMap[v] = idx;

    return idx;
}


MeshBuilder::MeshBuilder() : d(new PrivateData())
{
}

MeshBuilder::~MeshBuilder()
{
    assert(d != NULL);
    delete d;
}


// Builder methods.
uint32_t MeshBuilder::addPosition(const Vec3 & v)
{
    d->posArray.push_back(v);
    return d->posArray.size() - 1;
}

uint32_t MeshBuilder::addNormal(const Vec3 & v)
{
    d->norArray.push_back(v);
    return d->norArray.size() - 1;
}

uint32_t MeshBuilder::addTexCoord(const Vec2 & v, uint32_t set/*=0*/)
{
    d->texArray[set].push_back(v);
    return d->texArray[set].size() - 1;
}

uint32_t MeshBuilder::addColor(const Vec4 & v, uint32_t set/*=0*/)
{
    d->colArray[set].push_back(v);
    return d->colArray[set].size() - 1;
}

void MeshBuilder::beginGroup(uint32_t id)
{
    d->currentGroup = id;
}

void MeshBuilder::endGroup()
{
    d->currentGroup = NIL;
}

// Add named material, check for uniquenes.
uint32_t MeshBuilder::addMaterial(const char * name)
{
    uint32_t index;
    auto found = d->materialMap.find(name);
    if (found != d->materialMap.end()) {
        assert(d->materialArray[index].name == name);
    }
    else {
        index = d->materialArray.size();
        d->materialMap[name] = index;
        
        Material material(name);
        d->materialArray.push_back(material);
    }
    return index;
}

void MeshBuilder::beginMaterial(uint32_t id)
{
    d->currentMaterial = id;
}

void MeshBuilder::endMaterial()
{
    d->currentMaterial = NIL;
}

void MeshBuilder::beginPolygon(uint32_t id/*=0*/)
{
    Face face;
    face.id = id;
    face.firstIndex = d->indexArray.size();
    face.indexCount = 0;
    face.material = d->currentMaterial;
    face.group = d->currentGroup;

    d->faceArray.push_back(face);
}

uint32_t MeshBuilder::addVertex(uint32_t p, uint32_t n/*= NIL*/, uint32_t t0/*= NIL*/, uint32_t t1/*= NIL*/, uint32_t c0/*= NIL*/, uint32_t c1/*= NIL*/, uint32_t c2/*= NIL*/)
{
    // @@ In theory there's no need to add vertices before faces, but I'm adding this to debug problems in our maya exporter:
    assert(p < d->posArray.size());
    assert(n == NIL || n < d->norArray.size());
    assert(t0 == NIL || t0 < d->texArray[0].size());
    assert(t1 == NIL || t1 < d->texArray[1].size());
    //nvDebugCheck(c0 == NIL || c0 < d->colArray[0].size());
    if (c0 > d->colArray[0].size()) c0 = 0;    // @@ This seems to be happening in loc_swamp_catwalk.mb! No idea why.
    assert(c1 == NIL || c1 < d->colArray[1].size());
    assert(c2 == NIL || c2 < d->colArray[2].size());

    uint32_t idx = d->pushVertex(p, n, t0, t1, c0, c1, c2);
    d->indexArray.push_back(idx);
    d->faceArray.back().indexCount++;
    return idx;
}

uint32_t MeshBuilder::addVertex(const Vec3 & pos)
{
    uint32_t p = addPosition(pos);
    return addVertex(p);
}

#if 0
uint MeshBuilder::addVertex(const Vec3 & pos, const Vec3 & nor, const Vec2 & tex0, const Vec2 & tex1, const Vec4 & col0, const Vec4 & col1)
{
    uint p = addPosition(pos);
    uint n = addNormal(nor);
    uint t0 = addTexCoord(tex0, 0);
    uint t1 = addTexCoord(tex1, 1);
    uint c0 = addColor(col0);
    uint c1 = addColor(col1);
    return addVertex(p, n, t0, t1, c0, c1);
}
#endif

// Return true if the face is valid and was added to the mesh.
bool MeshBuilder::endPolygon()
{
    const Face & face = d->faceArray.back();
    const uint32_t count = face.indexCount;

    // Validate polygon here.
    bool invalid = count <= 2;

    if (!invalid) {
        // Skip zero area polygons. Or polygons with degenerate edges (which will result in zero-area triangles).
        const uint32_t first = face.firstIndex;
        for (uint32_t j = count - 1, i = 0; i < count; j = i, i++) {
            uint32_t v0 = d->indexArray[first + i];
            uint32_t v1 = d->indexArray[first + j];

            uint32_t p0 = d->vertexArray[v0].pos;
            uint32_t p1 = d->vertexArray[v1].pos;

            if (p0 == p1) {
                invalid = true;
                break;
            }

            //if (d->posArray[p0].Equals(d->posArray[p1])) {
            //    invalid = true;
            //    break;
            //}
        }

        uint32_t v0 = d->indexArray[first];
        uint32_t p0 = d->vertexArray[v0].pos;
        Vec3 x0 = d->posArray[p0];

        float area = 0.0f;
        for (uint32_t j = 1, i = 2; i < count; j = i, i++) {
            uint32_t v1 = d->indexArray[first + i];
            uint32_t v2 = d->indexArray[first + j];

            uint32_t p1 = d->vertexArray[v1].pos;
            uint32_t p2 = d->vertexArray[v2].pos;

            Vec3 x1 = d->posArray[p1];
            Vec3 x2 = d->posArray[p2];

            Triangle t(x0, x1, x2);
            if (t.IsDegenerate())
                invalid = true;

            area += ((x1-x0).Cross(x2-x0)).Length();
        }

        if (0.5 * area < 1e-6) {    // Reduce this threshold if artists have legitimate complains.
            invalid = true;
        }
        




        // @@ This is not complete. We may still get zero area triangles after triangulation.
        // However, our plugin triangulates before building the mesh, so hopefully that's not a problem.

    }

    if (invalid)
    {
        d->indexArray.resize(d->indexArray.size() - count);
        VECTOR_POP_BACK(d->faceArray);
        return false;
    }
    else
    {
        if (d->currentMaterial != NIL) {
            d->materialArray[d->currentMaterial].faceCount++;
        }

        d->maxFaceIndexCount = std::max(d->maxFaceIndexCount, count);
        return true;
    }
}


uint32_t MeshBuilder::weldPositions()
{
    std::vector<uint32_t> xrefs;
    Weld<Vec3> weldVec3;

    if (d->posArray.size()) {
        // Weld vertex attributes.
        weldVec3(d->posArray, xrefs);

        // Remap vertex indices.
        const uint32_t vertexCount = d->vertexArray.size();
        for (uint32_t v = 0; v < vertexCount; v++)
        {
            Vertex & vertex = d->vertexArray[v];
            if (vertex.pos != 0) 
                vertex.pos = xrefs[vertex.pos];
        }
    }

    return d->posArray.size();
}

uint32_t MeshBuilder::weldNormals()
{
    std::vector<uint32_t> xrefs;
    Weld<Vec3> weldVec3;

    if (d->norArray.size()) {
        // Weld vertex attributes.
        weldVec3(d->norArray, xrefs);

        // Remap vertex indices.
        const uint32_t vertexCount = d->vertexArray.size();
        for (uint32_t v = 0; v < vertexCount; v++)
        {
            Vertex & vertex = d->vertexArray[v];
            if (vertex.nor != NIL) vertex.nor = xrefs[vertex.nor];
        }
    }

    return d->norArray.size();
}

uint32_t MeshBuilder::weldTexCoords(uint32_t set/*=0*/)
{
    std::vector<uint32_t> xrefs;
    Weld<Vec2> weldVec2;

    if (d->texArray[set].size()) {
        // Weld vertex attributes.
        weldVec2(d->texArray[set], xrefs);

        // Remap vertex indices.
        const uint32_t vertexCount = d->vertexArray.size();
        for (uint32_t v = 0; v < vertexCount; v++)
        {
            Vertex & vertex = d->vertexArray[v];
            if (vertex.tex[set] != NIL) vertex.tex[set] = xrefs[vertex.tex[set]];
        }
    }

    return d->texArray[set].size();
}

uint32_t  MeshBuilder::weldColors(uint32_t set/*=0*/)
{
    std::vector<uint32_t> xrefs;
    Weld<Vec4> weldVec4;

    if (d->colArray[set].size()) {
        // Weld vertex attributes.
        weldVec4(d->colArray[set], xrefs);

        // Remap vertex indices.
        const uint32_t vertexCount = d->vertexArray.size();
        for (uint32_t v = 0; v < vertexCount; v++)
        {
            Vertex & vertex = d->vertexArray[v];
            if (vertex.col[set] != NIL) vertex.col[set] = xrefs[vertex.col[set]];
        }
    }

    return d->colArray[set].size();
}

void MeshBuilder::weldVertices() {

    if (d->vertexArray.size() == 0) {
        // Nothing to do.
        return;
    }

    std::vector<uint32_t> xrefs;
    Weld<Vertex> weldVertex;

    // Weld vertices.
    weldVertex(d->vertexArray, xrefs);

    // Remap face indices.
    const uint32_t indexCount = d->indexArray.size();
    for (uint32_t i = 0; i < indexCount; i++)
    {
        d->indexArray[i] = xrefs[d->indexArray[i]];
    }

    // Remap vertex map.
    for (auto& kvp : d->vertexMap)
    //for (size_t i = 0; i < d->vertexMap.size(); ++i)
    {
        //JRS TODO
        d->vertexMap[kvp.first] = xrefs[kvp.second];
    }
}


void MeshBuilder::optimize()
{
    if (d->vertexArray.size() == 0)
    {
        return;
    }

    weldPositions();
    weldNormals();
    weldTexCoords(0);
    weldTexCoords(1);
    weldColors();

    weldVertices();
}






void MeshBuilder::removeUnusedMaterials(std::vector<uint32_t> & newMaterialId)
{
    uint32_t materialCount = d->materialArray.size();

    // Reset face counts.
    for (uint32_t i = 0; i < materialCount; i++) {
        d->materialArray[i].faceCount = 0;
    }

    // Count faces.
    for (size_t i = 0; i < d->faceArray.size(); ++i) {
        Face & face = d->faceArray[i];

        if (face.material != NIL) {
            assert(face.material < materialCount);

            d->materialArray[face.material].faceCount++;
        }
    }

    // Remove unused materials.
    newMaterialId.resize(materialCount);

    for (uint32_t i = 0, m = 0; i < materialCount; i++)
    {
        if (d->materialArray[m].faceCount > 0)
        {
            newMaterialId[i] = m++;
        }
        else
        {
            newMaterialId[i] = NIL;
            VECTOR_REMOVE_AT(d->materialArray, m);
        }
    }

    materialCount = d->materialArray.size();

    // Update face material ids.
    for (size_t i = 0; i < d->faceArray.size(); ++i) {
        Face & face = d->faceArray[i];

        if (face.material != NIL) {
            uint32_t id = newMaterialId[face.material];
            assert(id != NIL && id < materialCount);

            face.material = id;
        }
    }
}

void MeshBuilder::sortFacesByGroup()
{
    const uint32_t faceCount = d->faceArray.size();

    std::vector<uint32_t> faceGroupArray;
    faceGroupArray.resize(faceCount);
    
    for (uint32_t i = 0; i < faceCount; i++) {
        faceGroupArray[i] = d->faceArray[i].group;
    }

    RadixSort radix;
    radix.sort(faceGroupArray);

    std::vector<Face> newFaceArray;
    newFaceArray.resize(faceCount);

    for (uint32_t i = 0; i < faceCount; i++) {
        newFaceArray[i] = d->faceArray[radix.rank(i)];
    }

    swap(newFaceArray, d->faceArray);
}

void MeshBuilder::sortFacesByMaterial()
{
    const uint32_t faceCount = d->faceArray.size();

    std::vector<uint32_t> faceMaterialArray;
    faceMaterialArray.resize(faceCount);
    
    for (uint32_t i = 0; i < faceCount; i++) {
        faceMaterialArray[i] = d->faceArray[i].material;
    }

    RadixSort radix;
    radix.sort(faceMaterialArray);

    std::vector<Face> newFaceArray;
    newFaceArray.resize(faceCount);

    for (uint32_t i = 0; i < faceCount; i++) {
        newFaceArray[i] = d->faceArray[radix.rank(i)];
    }

    swap(newFaceArray, d->faceArray);
}


void MeshBuilder::reset()
{
    assert(d != NULL);
    delete d;
    d = new PrivateData();
}

void MeshBuilder::done()
{
    if (d->currentGroup != NIL) {
        endGroup();
    }

    if (d->currentMaterial != NIL) {
        endMaterial();
    }
}

// Hints.
void MeshBuilder::hintTriangleCount(uint32_t count)
{
    d->indexArray.reserve(d->indexArray.size() + count * 4);
}

void MeshBuilder::hintVertexCount(uint32_t count)
{
    d->vertexArray.reserve(d->vertexArray.size() + count);
    d->vertexMap.reserve(d->vertexMap.size() + count);
}

void MeshBuilder::hintPositionCount(uint32_t count)
{
    d->posArray.reserve(d->posArray.size() + count);
}

void MeshBuilder::hintNormalCount(uint32_t count)
{
    d->norArray.reserve(d->norArray.size() + count);
}

void MeshBuilder::hintTexCoordCount(uint32_t count, uint32_t set/*=0*/)
{
    d->texArray[set].reserve(d->texArray[set].size() + count);
}

void MeshBuilder::hintColorCount(uint32_t count, uint32_t set/*=0*/)
{
    d->colArray[set].reserve(d->colArray[set].size() + count);
}


// Helpers.
void MeshBuilder::addTriangle(uint32_t v0, uint32_t v1, uint32_t v2)
{
    beginPolygon();
    addVertex(v0);
    addVertex(v1);
    addVertex(v2);
    endPolygon();
}

void MeshBuilder::addQuad(uint32_t v0, uint32_t v1, uint32_t v2, uint32_t v3)
{
    beginPolygon();
    addVertex(v0);
    addVertex(v1);
    addVertex(v2);
    addVertex(v3);
    endPolygon();
}


// Get tri mesh.
TriMesh * MeshBuilder::buildTriMesh() const
{
    const uint32_t faceCount = d->faceArray.size();
    uint32_t triangleCount = 0;
    for (uint32_t f = 0; f < faceCount; f++) {
        triangleCount += d->faceArray[f].indexCount - 2;
    }
    
    const uint32_t vertexCount = d->vertexArray.size();
    TriMesh * mesh = new TriMesh(triangleCount, vertexCount);

    // Build faces.
    std::vector<TriMesh::Face> & faces = mesh->faces();

    for(uint32_t f = 0; f < faceCount; f++)
    {
        int firstIndex = d->faceArray[f].firstIndex;
        int indexCount = d->faceArray[f].indexCount;

        int v0 = d->indexArray[firstIndex + 0];
        int v1 = d->indexArray[firstIndex + 1];

        for(int t = 0; t < indexCount - 2; t++) {
            int v2 = d->indexArray[firstIndex + t + 2];

            TriMesh::Face face;
            face.id = faces.size();
            face.v[0] = v0;
            face.v[1] = v1;
            face.v[2] = v2;
            faces.push_back(face);

            v1 = v2;
        }
    }

    // Build vertices.
    std::vector<BaseMesh::Vertex> & vertices = mesh->vertices();

    for(uint32_t i = 0; i < vertexCount; i++)
    {
        BaseMesh::Vertex vertex;
        vertex.id = i;
        if (d->vertexArray[i].pos != NIL) 
            vertex.pos = d->posArray[d->vertexArray[i].pos];
        if (d->vertexArray[i].nor != NIL) 
            vertex.nor = d->norArray[d->vertexArray[i].nor];
        if (d->vertexArray[i].tex[0] != NIL) 
            vertex.tex = d->texArray[0][d->vertexArray[i].tex[0]];

        vertices.push_back(vertex);
    }

    return mesh;
}

// Get quad/tri mesh.
QuadTriMesh * MeshBuilder::buildQuadTriMesh() const
{
    const uint32_t faceCount = d->faceArray.size();
    const uint32_t vertexCount = d->vertexArray.size();
    QuadTriMesh * mesh = new QuadTriMesh(faceCount, vertexCount);

    // Build faces.
    std::vector<QuadTriMesh::Face> & faces = mesh->faces();

    for (uint32_t f = 0; f < faceCount; f++)
    {
        int firstIndex = d->faceArray[f].firstIndex;
        int indexCount = d->faceArray[f].indexCount;

        QuadTriMesh::Face face;
        face.id = f;

        face.v[0] = d->indexArray[firstIndex + 0];
        face.v[1] = d->indexArray[firstIndex + 1];
        face.v[2] = d->indexArray[firstIndex + 2];

        // Only adds triangles and quads. Ignores polygons.
        if (indexCount == 3) {
            face.v[3] = NIL;
            faces.push_back(face);
        }
        else if (indexCount == 4) {
            face.v[3] = d->indexArray[firstIndex + 3];
            faces.push_back(face);
        }
    }

    // Build vertices.
    std::vector<BaseMesh::Vertex> & vertices = mesh->vertices();

    for(uint32_t i = 0; i < vertexCount; i++)
    {
        BaseMesh::Vertex vertex;
        vertex.id = i;
        if (d->vertexArray[i].pos != NIL) 
            vertex.pos = d->posArray[d->vertexArray[i].pos];
        if (d->vertexArray[i].nor != NIL) 
            vertex.nor = d->norArray[d->vertexArray[i].nor];
        if (d->vertexArray[i].tex[0] != NIL) 
            vertex.tex = d->texArray[0][d->vertexArray[i].tex[0]];

        vertices.push_back(vertex);
    }

    return mesh;
}

// Get half edge mesh.
HalfEdge::Mesh * MeshBuilder::buildHalfEdgeMesh(bool weldPositions, Error * error/*=NULL*/, std::vector<uint32_t> * badFaces/*=NULL*/) const
{
    if (error != NULL) *error = Error_None;

    const uint32_t vertexCount = d->vertexArray.size();
    std::auto_ptr<HalfEdge::Mesh> mesh(new HalfEdge::Mesh());

    for(uint32_t v = 0; v < vertexCount; v++)
    {
        HalfEdge::Vertex * vertex = mesh->addVertex(d->posArray[d->vertexArray[v].pos]);
        if (d->vertexArray[v].nor != NIL) 
            vertex->nor = d->norArray[d->vertexArray[v].nor];
        if (d->vertexArray[v].tex[0] != NIL) 
            vertex->tex = Vec2(d->texArray[0][d->vertexArray[v].tex[0]]);
        //??if (d->vertexArray[v].col[0] != NIL) 
        //??    vertex->col = d->colArray[0][d->vertexArray[v].col[0]];
    }

    if (weldPositions) {
        mesh->linkColocals();
    }
    else {
        // Build canonical map from position indices.
        std::vector<uint32_t> canonicalMap(vertexCount);
        
        for (size_t i = 0; i < d->vertexArray.size(); ++i) {
            canonicalMap.push_back(d->vertexArray[i].pos);
        }

        mesh->linkColocalsWithCanonicalMap(canonicalMap);
    }

    const uint32_t faceCount = d->faceArray.size();
    for (uint32_t f = 0; f < faceCount; f++)
    {
        const uint32_t firstIndex = d->faceArray[f].firstIndex;
        const uint32_t indexCount = d->faceArray[f].indexCount;

        HalfEdge::Face * face = mesh->addFace(d->indexArray, firstIndex, indexCount);
        
        // @@ This is too late, removing the face here will leave the mesh improperly connected.
        /*if (face->area() <= FLT_EPSILON) {
            mesh->remove(face);
            face = NULL;
        }*/

        if (face == NULL) {
            // Non manifold mesh.
            if (error != NULL) *error = Error_NonManifoldEdge;
            if (badFaces != NULL) {
                badFaces->push_back(d->faceArray[f].id);
            }
            //return NULL; // IC: Ignore error and continue building the mesh.
        }

        if (face != NULL) {
            face->group = d->faceArray[f].group;
            face->material = d->faceArray[f].material;
        }
    }

    mesh->linkBoundary();

    // We cannot fix functions here, because this would introduce new vertices and these vertices won't have the corresponding builder data.

    // Maybe the builder should perform the search for T-junctions and update the vertex data directly.

    // For now, we don't fix T-junctions at export time, but only during parameterization.

    //mesh->fixBoundaryJunctions();

    //mesh->sewBoundary();

    return mesh.release();
}

// Get half edge mesh.
#if 0
Reducer::Mesh * MeshBuilder::buildReducerMesh(bool weldPositions, Error * error/*=NULL*/, std::vector<uint32_t> * badFaces/*=NULL*/) const
{
    if (error != NULL) *error = Error_None;

    const uint32_t vertexCount = d->vertexArray.size();
    Reducer::Builder builder(0U);

    for(uint32_t v = 0; v < vertexCount; v++)
    {
        /*Reducer::Vertex * vertex =*/ builder.addVertex(d->posArray[d->vertexArray[v].pos]);
        //if (d->vertexArray[v].col[0] != NIL) vertex->col = d->colArray[0][d->vertexArray[v].col[0]];
    }

    if (weldPositions) {
        builder.linkColocalVertices();
    }
    else {
        // Build canonical map from position indices.
        std::vector<uint32_t> canonicalMap(vertexCount);
        
        foreach (i, d->vertexArray) {
            canonicalMap.append(d->vertexArray[i].pos);
        }

        builder.linkColocalVerticesWithCanonicalMap(canonicalMap);
    }

    const uint faceCount = d->faceArray.size();
    for (uint f = 0; f < faceCount; f++)
    {
        uint firstIndex = d->faceArray[f].firstIndex;
        uint indexCount = d->faceArray[f].indexCount;
        uint material = d->faceArray[f].material;

        Reducer::Face * face = builder.addFace(d->indexArray, firstIndex, indexCount);
        
        // @@ This is too late, removing the face here will leave the mesh improperly connected.
        /*if (face->area() <= FLT_EPSILON) {
            mesh->remove(face);
            face = NULL;
        }*/

        if (face == NULL) {
            // Non manifold mesh.
            if (error != NULL) *error = Error_NonManifoldEdge;
            if (badFaces != NULL) {
                badFaces->append(d->faceArray[f].id);
            }
            //return NULL; // IC: Ignore error and continue building the mesh.
        }

        if (face != NULL) {
            face->material = material;
        }
    }

    // We cannot fix functions here, because this would introduce new vertices and these vertices won't have the corresponding builder data.

    // Maybe the builder should perform the search for T-junctions and update the vertex data directly.

    // For now, we don't fix T-junctions at export time, but only during parameterization.

    //mesh->fixBoundaryJunctions();

    //mesh->sewBoundary();

    return builder.release();
}
#endif

bool MeshBuilder::buildPositions(std::vector<Vec3> & positionArray)
{
    const uint32_t vertexCount = d->vertexArray.size();
    positionArray.resize(vertexCount);

    for (uint32_t v = 0; v < vertexCount; v++)
    {
        assert(d->vertexArray[v].pos != NIL);
        positionArray[v] = d->posArray[d->vertexArray[v].pos];
    }

    return true;
}

bool MeshBuilder::buildNormals(std::vector<Vec3> & normalArray)
{
    bool anyNormal = false;

    const uint32_t vertexCount = d->vertexArray.size();
    normalArray.resize(vertexCount);

    for (uint32_t v = 0; v < vertexCount; v++)
    {
        if (d->vertexArray[v].nor == NIL) {
            normalArray[v] = Vec3(0, 0, 1);
        }
        else {
            anyNormal = true;
            normalArray[v] = d->norArray[d->vertexArray[v].nor];
        }
    }

    return anyNormal;
}

bool MeshBuilder::buildTexCoords(std::vector<Vec2> & texCoordArray, uint32_t set/*=0*/)
{
    bool anyTexCoord = false;

    const uint32_t vertexCount = d->vertexArray.size();
    texCoordArray.resize(vertexCount);

    for (uint32_t v = 0; v < vertexCount; v++)
    {
        if (d->vertexArray[v].tex[set] == NIL) {
            texCoordArray[v] = Vec2(0, 0);
        }
        else {
            anyTexCoord = true;
            texCoordArray[v] = d->texArray[set][d->vertexArray[v].tex[set]];
        }
    }

    return anyTexCoord;
}

bool MeshBuilder::buildColors(std::vector<Vec4> & colorArray, uint32_t set/*=0*/)
{
    bool anyColor = false;

    const uint32_t vertexCount = d->vertexArray.size();
    colorArray.resize(vertexCount);

    for (uint32_t v = 0; v < vertexCount; v++)
    {
        if (d->vertexArray[v].col[set] == NIL) {
            colorArray[v] = Vec4(0, 0, 0, 1);
        }
        else {
            anyColor = true;
            colorArray[v] = d->colArray[set][d->vertexArray[v].col[set]];
        }
    }

    return anyColor;
}

void MeshBuilder::buildVertexToPositionMap(std::vector<int> &map)
{
	const uint32_t vertexCount = d->vertexArray.size();
	map.resize(vertexCount);

	for (unsigned i = 0; i < d->vertexArray.size(); ++i) 
    {
		map[i] = d->vertexArray[i].pos;
	}
}



uint32_t MeshBuilder::vertexCount() const
{
    return d->vertexArray.size();
}


uint32_t MeshBuilder::positionCount() const
{
    return d->posArray.size();
}

uint32_t MeshBuilder::normalCount() const
{
    return d->norArray.size();
}

uint32_t MeshBuilder::texCoordCount(uint32_t set/*=0*/) const
{
    return d->texArray[set].size();
}

uint32_t MeshBuilder::colorCount(uint32_t set/*=0*/) const
{
    return d->colArray[set].size();
}


uint32_t MeshBuilder::materialCount() const
{
    return d->materialArray.size();
}

const char * MeshBuilder::material(uint32_t i) const
{
    return d->materialArray[i].name.c_str();
}


uint32_t MeshBuilder::positionIndex(uint32_t vertex) const
{
    return d->vertexArray[vertex].pos;
}
uint32_t MeshBuilder::normalIndex(uint32_t vertex) const
{
    return d->vertexArray[vertex].nor;
}
uint32_t MeshBuilder::texCoordIndex(uint32_t vertex, uint32_t set/*=0*/) const
{
    return d->vertexArray[vertex].tex[set];
}
uint32_t MeshBuilder::colorIndex(uint32_t vertex, uint32_t set/*=0*/) const
{
    return d->vertexArray[vertex].col[set];
}
