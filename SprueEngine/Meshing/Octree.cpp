/*

Implementations of Octree member functions.

Copyright (C) 2011  Tao Ju

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public License
(LGPL) as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

#include "SprueEngine/Meshing/Octree.h"
#include "SprueEngine/IMeshable.h"
#include "SprueEngine/Math/LeastSquares.h"
#include "SprueEngine/ObjectPool.h"
#include "SprueEngine/Logging.h"
//#include	"density.h"

static ObjectPool<OctreeNode, DefaultMemoryAllocator<OctreeNode> > MeshingOctreeNodePool(64 * 64 * 64 * 8);


// ----------------------------------------------------------------------------

const int MATERIAL_AIR = 0;
const int MATERIAL_SOLID = 1;

const double QEF_ERROR = 1e-4;
const int QEF_SWEEPS = 8;

// ----------------------------------------------------------------------------

const Vec3 CHILD_MIN_OFFSETS[] =
{
    // needs to match the vertMap from Dual Contouring impl
    Vec3(0, 0, 0),
    Vec3(0, 0, 1),
    Vec3(0, 1, 0),
    Vec3(0, 1, 1),
    Vec3(1, 0, 0),
    Vec3(1, 0, 1),
    Vec3(1, 1, 0),
    Vec3(1, 1, 1),
};

// ----------------------------------------------------------------------------
// data from the original DC impl, drives the contouring process

const int edgevmap[12][2] =
{
    { 0, 4 }, { 1, 5 }, { 2, 6 }, { 3, 7 },	// x-axis 
    { 0, 2 }, { 1, 3 }, { 4, 6 }, { 5, 7 },	// y-axis
    { 0, 1 }, { 2, 3 }, { 4, 5 }, { 6, 7 }	// z-axis
};

const int edgemask[3] = { 5, 3, 6 };

const int vertMap[8][3] =
{
    { 0, 0, 0 },
    { 0, 0, 1 },
    { 0, 1, 0 },
    { 0, 1, 1 },
    { 1, 0, 0 },
    { 1, 0, 1 },
    { 1, 1, 0 },
    { 1, 1, 1 }
};

const int faceMap[6][4] = { { 4, 8, 5, 9 }, { 6, 10, 7, 11 }, { 0, 8, 1, 10 }, { 2, 9, 3, 11 }, { 0, 4, 2, 6 }, { 1, 5, 3, 7 } };
const int cellProcFaceMask[12][3] = { { 0, 4, 0 }, { 1, 5, 0 }, { 2, 6, 0 }, { 3, 7, 0 }, { 0, 2, 1 }, { 4, 6, 1 }, { 1, 3, 1 }, { 5, 7, 1 }, { 0, 1, 2 }, { 2, 3, 2 }, { 4, 5, 2 }, { 6, 7, 2 } };
const int cellProcEdgeMask[6][5] = { { 0, 1, 2, 3, 0 }, { 4, 5, 6, 7, 0 }, { 0, 4, 1, 5, 1 }, { 2, 6, 3, 7, 1 }, { 0, 2, 4, 6, 2 }, { 1, 3, 5, 7, 2 } };

const int faceProcFaceMask[3][4][3] = {
    { { 4, 0, 0 }, { 5, 1, 0 }, { 6, 2, 0 }, { 7, 3, 0 } },
    { { 2, 0, 1 }, { 6, 4, 1 }, { 3, 1, 1 }, { 7, 5, 1 } },
    { { 1, 0, 2 }, { 3, 2, 2 }, { 5, 4, 2 }, { 7, 6, 2 } }
};

const int faceProcEdgeMask[3][4][6] = {
    { { 1, 4, 0, 5, 1, 1 }, { 1, 6, 2, 7, 3, 1 }, { 0, 4, 6, 0, 2, 2 }, { 0, 5, 7, 1, 3, 2 } },
    { { 0, 2, 3, 0, 1, 0 }, { 0, 6, 7, 4, 5, 0 }, { 1, 2, 0, 6, 4, 2 }, { 1, 3, 1, 7, 5, 2 } },
    { { 1, 1, 0, 3, 2, 0 }, { 1, 5, 4, 7, 6, 0 }, { 0, 1, 5, 0, 4, 1 }, { 0, 3, 7, 2, 6, 1 } }
};

const int edgeProcEdgeMask[3][2][5] = {
    { { 3, 2, 1, 0, 0 }, { 7, 6, 5, 4, 0 } },
    { { 5, 1, 4, 0, 1 }, { 7, 3, 6, 2, 1 } },
    { { 6, 4, 2, 0, 2 }, { 7, 5, 3, 1, 2 } },
};

const int processEdgeMask[3][4] = { 
    { 3, 2, 1, 0 }, 
    { 7, 5, 6, 4 }, 
    { 11, 10, 9, 8 } 
};

// -------------------------------------------------------------------------------

OctreeNode* SimplifyOctree(OctreeNode* node, float threshold)
{
    if (!node)
    {
        return NULL;
    }

    if (node->type != Node_Internal)
    {
        // can't simplify!
        return node;
    }

    QefSolver qef;
    int signs[8] = { -1, -1, -1, -1, -1, -1, -1, -1 };
    int midsign = -1;
    int edgeCount = 0;
    bool isCollapsible = true;

    for (int i = 0; i < 8; i++)
    {
        node->children[i] = SimplifyOctree(node->children[i], threshold);
        if (node->children[i])
        {
            OctreeNode* child = node->children[i];
            if (child->type == Node_Internal)
            {
                isCollapsible = false;
            }
            else
            {
                qef.add(child->drawInfo->qef);

                midsign = (child->drawInfo->corners >> (7 - i)) & 1;
                signs[i] = (child->drawInfo->corners >> i) & 1;

                edgeCount++;
            }
        }
    }

    if (!isCollapsible)
    {
        // at least one child is an internal node, can't collapse
        return node;
    }

    Vec3 qefPosition;
    qef.solve(qefPosition, QEF_ERROR, QEF_SWEEPS, QEF_ERROR);
    float error = qef.getError();

    // convert to glm Vec3 for ease of use
    Vec3 position(qefPosition.x, qefPosition.y, qefPosition.z);

    // at this point the masspoint will actually be a sum, so divide to make it the average
    if (error > threshold)
    {
        // this collapse breaches the threshold
        return node;
    }

    if (position.x < node->min.x || position.x >(node->min.x + node->size) ||
        position.y < node->min.y || position.y >(node->min.y + node->size) ||
        position.z < node->min.z || position.z >(node->min.z + node->size))
    {
        const auto& mp = qef.getMassPoint();
        position = Vec3(mp.x, mp.y, mp.z);
    }

    // change the node from an internal node to a 'psuedo leaf' node
    OctreeDrawInfo* drawInfo = new OctreeDrawInfo;

    for (int i = 0; i < 8; i++)
    {
        if (signs[i] == -1)
        {
            // Undetermined, use centre sign instead
            drawInfo->corners |= (midsign << i);
        }
        else
        {
            drawInfo->corners |= (signs[i] << i);
        }
    }

    drawInfo->averageNormal = Vec3(0.0f, 0.0f, 0.0f);
    for (int i = 0; i < 8; i++)
    {
        if (node->children[i])
        {
            OctreeNode* child = node->children[i];
            if (child->type == Node_Psuedo || child->type == Node_Leaf)
            {
                drawInfo->averageNormal += child->drawInfo->averageNormal;
            }
        }
    }

    drawInfo->averageNormal.Normalize();
    drawInfo->position = position;
    drawInfo->qef = qef.getData();

    for (int i = 0; i < 8; i++)
    {
        DestroyOctree(node->children[i]);
        node->children[i] = nullptr;
    }

    node->type = Node_Psuedo;
    node->drawInfo = drawInfo;

    return node;
}

// ----------------------------------------------------------------------------

void GenerateVertexIndices(OctreeNode* node, VertexBuffer& vertexBuffer)
{
    if (!node)
    {
        return;
    }

    if (node->type != Node_Leaf)
    {
        for (int i = 0; i < 8; i++)
        {
            GenerateVertexIndices(node->children[i], vertexBuffer);
        }
    }

    if (node->type != Node_Internal)
    {
        OctreeDrawInfo* d = node->drawInfo;
        if (!d)
        {
            SPRUE_LOG_ERROR("Error! Could not add vertex!\n");
            return;
        }

        d->index = (int)vertexBuffer.size();
        vertexBuffer.push_back(MeshVertex(d->position, d->averageNormal));
    }
}

// ----------------------------------------------------------------------------

void ContourProcessEdge(OctreeNode* node[4], int dir, IndexBuffer& indexBuffer)
{
    int minSize = 1000000;		// arbitrary big number
    int minIndex = 0;
    int indices[4] = { -1, -1, -1, -1 };
    bool flip = false;
    bool signChange[4] = { false, false, false, false };

    for (int i = 0; i < 4; i++)
    {
        const int edge = processEdgeMask[dir][i];
        const int c1 = edgevmap[edge][0];
        const int c2 = edgevmap[edge][1];

        const int m1 = (node[i]->drawInfo->corners >> c1) & 1;
        const int m2 = (node[i]->drawInfo->corners >> c2) & 1;

        if (node[i]->size < minSize)
        {
            minSize = node[i]->size;
            minIndex = i;
            flip = m1 != MATERIAL_AIR;
        }

        indices[i] = node[i]->drawInfo->index;

        signChange[i] =
            (m1 == MATERIAL_AIR && m2 != MATERIAL_AIR) ||
            (m1 != MATERIAL_AIR && m2 == MATERIAL_AIR);
    }

    if (signChange[minIndex])
    {
        if (!flip)
        {
            indexBuffer.push_back(indices[0]);
            indexBuffer.push_back(indices[1]);
            indexBuffer.push_back(indices[3]);

            indexBuffer.push_back(indices[0]);
            indexBuffer.push_back(indices[3]);
            indexBuffer.push_back(indices[2]);
        }
        else
        {
            indexBuffer.push_back(indices[0]);
            indexBuffer.push_back(indices[3]);
            indexBuffer.push_back(indices[1]);

            indexBuffer.push_back(indices[0]);
            indexBuffer.push_back(indices[2]);
            indexBuffer.push_back(indices[3]);
        }
    }
}

// ----------------------------------------------------------------------------

void ContourEdgeProc(OctreeNode* node[4], int dir, IndexBuffer& indexBuffer)
{
    if (!node[0] || !node[1] || !node[2] || !node[3])
    {
        return;
    }

    if (node[0]->type != Node_Internal && node[1]->type != Node_Internal && node[2]->type != Node_Internal && node[3]->type != Node_Internal)
    {
        ContourProcessEdge(node, dir, indexBuffer);
    }
    else
    {
        for (int i = 0; i < 2; i++)
        {
            OctreeNode* edgeNodes[4];
            const int c[4] =
            {
                edgeProcEdgeMask[dir][i][0],
                edgeProcEdgeMask[dir][i][1],
                edgeProcEdgeMask[dir][i][2],
                edgeProcEdgeMask[dir][i][3],
            };

            for (int j = 0; j < 4; j++)
            {
                if (node[j]->type == Node_Leaf || node[j]->type == Node_Psuedo)
                {
                    edgeNodes[j] = node[j];
                }
                else
                {
                    edgeNodes[j] = node[j]->children[c[j]];
                }
            }

            ContourEdgeProc(edgeNodes, edgeProcEdgeMask[dir][i][4], indexBuffer);
        }
    }
}

// ----------------------------------------------------------------------------

void ContourFaceProc(OctreeNode* node[2], int dir, IndexBuffer& indexBuffer)
{
    if (!node[0] || !node[1])
    {
        return;
    }

    if (node[0]->type == Node_Internal ||
        node[1]->type == Node_Internal)
    {
        for (int i = 0; i < 4; i++)
        {
            OctreeNode* faceNodes[2];
            const int c[2] =
            {
                faceProcFaceMask[dir][i][0],
                faceProcFaceMask[dir][i][1],
            };

            for (int j = 0; j < 2; j++)
            {
                if (node[j]->type != Node_Internal)
                {
                    faceNodes[j] = node[j];
                }
                else
                {
                    faceNodes[j] = node[j]->children[c[j]];
                }
            }

            ContourFaceProc(faceNodes, faceProcFaceMask[dir][i][2], indexBuffer);
        }

        const int orders[2][4] =
        {
            { 0, 0, 1, 1 },
            { 0, 1, 0, 1 },
        };
        for (int i = 0; i < 4; i++)
        {
            OctreeNode* edgeNodes[4];
            const int c[4] =
            {
                faceProcEdgeMask[dir][i][1],
                faceProcEdgeMask[dir][i][2],
                faceProcEdgeMask[dir][i][3],
                faceProcEdgeMask[dir][i][4],
            };

            const int* order = orders[faceProcEdgeMask[dir][i][0]];
            for (int j = 0; j < 4; j++)
            {
                if (node[order[j]]->type == Node_Leaf ||
                    node[order[j]]->type == Node_Psuedo)
                {
                    edgeNodes[j] = node[order[j]];
                }
                else
                {
                    edgeNodes[j] = node[order[j]]->children[c[j]];
                }
            }

            ContourEdgeProc(edgeNodes, faceProcEdgeMask[dir][i][5], indexBuffer);
        }
    }
}

// ----------------------------------------------------------------------------

void ContourCellProc(OctreeNode* node, IndexBuffer& indexBuffer)
{
    if (node == NULL)
    {
        return;
    }

    if (node->type == Node_Internal)
    {
        for (int i = 0; i < 8; i++)
        {
            ContourCellProc(node->children[i], indexBuffer);
        }

        for (int i = 0; i < 12; i++)
        {
            OctreeNode* faceNodes[2];
            const int c[2] = { cellProcFaceMask[i][0], cellProcFaceMask[i][1] };

            faceNodes[0] = node->children[c[0]];
            faceNodes[1] = node->children[c[1]];

            ContourFaceProc(faceNodes, cellProcFaceMask[i][2], indexBuffer);
        }

        for (int i = 0; i < 6; i++)
        {
            OctreeNode* edgeNodes[4];
            const int c[4] =
            {
                cellProcEdgeMask[i][0],
                cellProcEdgeMask[i][1],
                cellProcEdgeMask[i][2],
                cellProcEdgeMask[i][3],
            };

            for (int j = 0; j < 4; j++)
            {
                edgeNodes[j] = node->children[c[j]];
            }

            ContourEdgeProc(edgeNodes, cellProcEdgeMask[i][4], indexBuffer);
        }
    }
}

// ----------------------------------------------------------------------------

Vec3 ApproximateZeroCrossingPosition(const SprueEngine::IMeshable* model, const Vec3& p0, const Vec3& p1)
{
    // approximate the zero crossing by finding the min value along the edge
    //float p0Val = model->CalculateDensity(p0);
    //float p1Val = model->CalculateDensity(p1);
    //return SprueLerp(p0, p1, NORMALIZE(0.0f, p0Val, p1Val));
    float minValue = 100000.f;
    float t = 0.f;
    float currentT = 0.f;
    const int steps = 32;
    const float increment = 1.f / (float)steps;
    bool isFirstDensity = true;
    while (currentT <= 1.f)
    {
        const Vec3 p = p0 + ((p1 - p0) * currentT);
        const float density = fabs(model->CalculateDensity(p));
        if (density < minValue)
        {
            minValue = density;
            t = currentT;
        }

        currentT += increment;
    }
    
    return p0 + ((p1 - p0) * t);
}

// ----------------------------------------------------------------------------

Vec3 CalculateSurfaceNormal(const SprueEngine::IMeshable* model, const Vec3& p)
{
    const float H = 0.001f;
    //const float base = model->CalculateDensity(p);
    //const float dx = base - model->CalculateDensity(p - Vec3(H, 0.f, 0.f));
    //const float dy = base - model->CalculateDensity(p - Vec3(0.f, H, 0.f));
    //const float dz = base - model->CalculateDensity(p - Vec3(0.f, 0.f, H));
    const float dx = model->CalculateDensity(p + Vec3(H, 0.f, 0.f)) - model->CalculateDensity(p - Vec3(H, 0.f, 0.f));
    const float dy = model->CalculateDensity(p + Vec3(0.f, H, 0.f)) - model->CalculateDensity(p - Vec3(0.f, H, 0.f));
    const float dz = model->CalculateDensity(p + Vec3(0.f, 0.f, H)) - model->CalculateDensity(p - Vec3(0.f, 0.f, H));

    return Vec3(dx, dy, dz).Normalized();
}

// ----------------------------------------------------------------------------

OctreeNode* ConstructLeaf(const SprueEngine::IMeshable* model, OctreeNode* leaf)
{
    if (!leaf || leaf->size != 1)
    {
        return nullptr;
    }

    int corners = 0;
    if (!model->OverrideCornerDetection(leaf->min, corners))
    {
        for (int i = 0; i < 8; i++)
        {
            const Vec3 cornerPos = leaf->min + CHILD_MIN_OFFSETS[i];
            const float density = model->CalculateDensity(cornerPos);
            const int material = density < 0.f ? MATERIAL_SOLID : MATERIAL_AIR;
            corners |= (material << i);
        }
    }

    if (corners == 0 || corners == 255)
    {
        // voxel is full inside or outside the volume
        MeshingOctreeNodePool.Delete(leaf);
        //delete leaf;
        return nullptr;
    }

    // otherwise the voxel contains the surface, so find the edge intersections
    const int MAX_CROSSINGS = 6;
    int edgeCount = 0;
    Vec3 averageNormal(0, 0, 0);
    QefSolver qef;

    for (int i = 0; i < 12 && edgeCount < MAX_CROSSINGS; i++)
    {
        const int c1 = edgevmap[i][0];
        const int c2 = edgevmap[i][1];

        const int m1 = (corners >> c1) & 1;
        const int m2 = (corners >> c2) & 1;

        if ((m1 == MATERIAL_AIR && m2 == MATERIAL_AIR) ||
            (m1 == MATERIAL_SOLID && m2 == MATERIAL_SOLID))
        {
            // no zero crossing on this edge
            continue;
        }

        const Vec3 p1 = leaf->min + CHILD_MIN_OFFSETS[c1];
        const Vec3 p2 = leaf->min + CHILD_MIN_OFFSETS[c2];
        Vec3 p;
        Vec3 n;
        float dist = 0.0f;
        if (model->OverrideEdgeSampling(leaf->min, i, n, dist))
        {
            if (dist == FLT_MAX)
                continue;
            p = SprueLerp(p1, p2, dist);
            n.Normalize();
        }
        else
        {
            p = ApproximateZeroCrossingPosition(model, p1, p2);
            n = CalculateSurfaceNormal(model, p);
        }
        
        qef.add(p.x, p.y, p.z, n.x, n.y, n.z);

        averageNormal += n;

        edgeCount++;
    }

    Vec3 qefPosition;
    qef.solve(qefPosition, QEF_ERROR, QEF_SWEEPS, QEF_ERROR);

    OctreeDrawInfo* drawInfo = new OctreeDrawInfo;
    drawInfo->position = qefPosition;
    drawInfo->qef = qef.getData();

    const Vec3 min = leaf->min;
    const Vec3 max = leaf->min + (float)leaf->size;
    if (drawInfo->position.x < min.x || drawInfo->position.x > max.x ||
        drawInfo->position.y < min.y || drawInfo->position.y > max.y ||
        drawInfo->position.z < min.z || drawInfo->position.z > max.z)
    {
        // Doing this creates shit, lerp a bit?
        const auto& mp = qef.getMassPoint();
        drawInfo->position = mp;
    }

    drawInfo->averageNormal = (averageNormal / (float)edgeCount).Normalized();
    drawInfo->corners = corners;

    leaf->type = Node_Leaf;
    leaf->drawInfo = drawInfo;

    return leaf;
}

// -------------------------------------------------------------------------------

OctreeNode* ConstructOctreeNodes(const SprueEngine::IMeshable* model, OctreeNode* node)
{
    if (!node)
    {
        return nullptr;
    }

    if (node->size == 1)
    {
        return ConstructLeaf(model, node);
    }

    const int childSize = node->size / 2;
    bool hasChildren = false;

    for (int i = 0; i < 8; i++)
    {
        OctreeNode* child = MeshingOctreeNodePool.New();//new OctreeNode;
        child->size = childSize;
        child->min = node->min + (CHILD_MIN_OFFSETS[i] * childSize);
        child->type = Node_Internal;

        node->children[i] = ConstructOctreeNodes(model, child);
        hasChildren |= (node->children[i] != nullptr);
    }

    if (!hasChildren)
    {
        MeshingOctreeNodePool.Delete(node);
        //delete node;
        return nullptr;
    }

    return node;
}

// -------------------------------------------------------------------------------

OctreeNode* BuildOctree(const SprueEngine::IMeshable* model, const Vec3& min, const int size, const float threshold)
{
    OctreeNode* root = MeshingOctreeNodePool.New();//new OctreeNode;
    root->min = min;
    root->size = size;
    root->type = Node_Internal;

    root = ConstructOctreeNodes(model, root);

    if (root && threshold > 0.0f)
        root = SimplifyOctree(root, threshold);

    return root;
}

// ----------------------------------------------------------------------------

void GenerateMeshFromOctree(OctreeNode* node, VertexBuffer& vertexBuffer, IndexBuffer& indexBuffer)
{
    if (!node)
    {
        return;
    }

    vertexBuffer.clear();
    indexBuffer.clear();

    GenerateVertexIndices(node, vertexBuffer);
    ContourCellProc(node, indexBuffer);
}

// -------------------------------------------------------------------------------

void DestroyOctree(OctreeNode* node)
{
    if (!node)
    {
        return;
    }

    for (int i = 0; i < 8; i++)
    {
        DestroyOctree(node->children[i]);
    }

    if (node->drawInfo)
    {
        delete node->drawInfo;
    }

    MeshingOctreeNodePool.Delete(node);
    //delete node;
}

// -------------------------------------------------------------------------------