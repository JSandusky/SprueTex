#pragma once

#include <windows.h>
#include <DirectXMath.h>
#include <stdint.h>

namespace DirectX
{
    /// JRS: Point rep is a vertex index that is a unique position in space, ie "close enough" points will reuse indices
    /// JRS: Adjacency, Face * 3, each unsigned integer is a "face index" to a face adjacent to an edge.
        /// One-ring neighborhood:
            // 1) for a given vertex in a face
            // 2) all not-equal  vertices in the face are 1-ring neighbors
            // 3) Iterate all face adjacency, count 

    HRESULT ConvertPointRepsToAdjacency(const uint16_t* indices, size_t nFaces,
        const XMFLOAT3* positions, size_t nVerts,
        const uint32_t* pointRep,
        uint32_t* adjacency);

    HRESULT ConvertPointRepsToAdjacency(const uint32_t* indices, size_t nFaces,
        const XMFLOAT3* positions, size_t nVerts,
        const uint32_t* pointRep,
        uint32_t* adjacency);

    HRESULT GenerateAdjacencyAndPointReps(const uint16_t* indices, size_t nFaces,
        const XMFLOAT3* positions, size_t nVerts,
        float epsilon,
        uint32_t* pointRep, uint32_t* adjacency);

    HRESULT GenerateAdjacencyAndPointReps(const uint32_t* indices, size_t nFaces,
        const XMFLOAT3* positions, size_t nVerts,
        float epsilon,
        uint32_t* pointRep, uint32_t* adjacency);
}