#include "LaplacianOperations.h"

#include <SprueEngine/Math/BoneWeights.h>
#include <SprueEngine/FString.h>
#include <SprueEngine/MathGeoLib/Geometry/Line.h>
#include <SprueEngine/Geometry/kdTree.h>
#include <SprueEngine/Logging.h>
#include <SprueEngine/Geometry/MeshData.h>
#include <SprueEngine/Geometry/MeshOctree.h>
#include <SprueEngine/Math/NearestIndex.h>
#include <SprueEngine/Geometry/Skeleton.h>

#include <SprueEngine/Libs/nvmesh/halfedge/Edge.h>
#include <SprueEngine/Libs/nvmesh/halfedge/Face.h>
#include <SprueEngine/Libs/nvmesh/halfedge/Mesh.h>
#include <SprueEngine/Libs/nvmesh/halfedge/Vertex.h>

#include <SprueEngine/Libs/Eigen/Sparse>
#include <SprueEngine/Libs/Eigen/Dense>

#include <SprueEngine/Libs/igl/cotmatrix.h>
#include <SprueEngine/Libs/igl/massmatrix.h>
#include <SprueEngine/Libs/igl/mat_min.h>
#include <SprueEngine/Libs/igl/normalize_row_sums.h>

using namespace Eigen;

namespace SprueEngine
{

#define BONE_GLOW_ITERATIONS 6
#define BONE_GLOW_ITERATION_WEIGHT (1.0f/BONE_GLOW_ITERATIONS)

    /// Performs the weighting calculation for the given vertex.
    void ComputeLaplacian(nv::HalfEdge::Vertex* vertex, Eigen::SparseMatrix<double>& laplaceMatrix, Vec3& point, bool useCotangentWeights)
    {
        float weightSum = 0.0f;
        int neighborCt = 0;
        auto edgeIt = vertex->edges();
        while (!edgeIt.isDone())
        {
            ++neighborCt;
            edgeIt.advance();
        }

        edgeIt = vertex->edges();

        if (useCotangentWeights)
        {
            // Cotangent weights preserve shape better
            while (!edgeIt.isDone())
            {
                nv::HalfEdge::Edge* currentEdge = edgeIt.current();
                const float weight = currentEdge->cotangentWeight();

                //auto prevEdge = currentEdge->prev->pair;
                //auto nextEdge = currentEdge->pair->next;
                //
                //Vec3 nextVec = currentEdge->to()->pos - nextEdge->to()->pos;
                //Vec3 prevVec = currentEdge->from()->pos - nextEdge->to()->pos;
                //const float alpha = nextVec.AngleBetween(prevVec);
                //
                //nextVec = currentEdge->to()->pos - prevEdge->to()->pos;
                //prevVec = currentEdge->from()->pos - prevEdge->to()->pos;
                //const float beta = nextVec.AngleBetween(prevVec);
                //
                //const float weight = ((1.0f / tanf(alpha)) + (1.0f / tanf(beta))) / 2.0f;
                //
                laplaceMatrix.coeffRef(vertex->id, currentEdge->to()->id) += weight;
                point += currentEdge->to()->pos * weight;
                weightSum += weight;
                edgeIt.advance();
            }

            edgeIt = vertex->edges();
            while (!edgeIt.isDone())
            {
                laplaceMatrix.coeffRef(vertex->id, edgeIt.current()->to()->id) /= weightSum;
                edgeIt.advance();
            }
            point /= weightSum;
        }
        else
        {
            // Umbrella weights
            while (!edgeIt.isDone())
            {
                unsigned otherIdx = edgeIt.current()->to()->id;
                point += edgeIt.current()->to()->pos;
                laplaceMatrix.coeffRef(vertex->id, otherIdx) = -1.0f / neighborCt;
                edgeIt.advance();
            }
            point /= neighborCt;
        }
    }

    std::vector< std::pair<unsigned, Vec3> > LaplacianOperations::GetClosestHandles(MeshData* meshData, const std::vector< std::pair<Vec3, Vec3> >& points)
    {
        // expect to get back at least as many results as we have points, seams will result in more than that though for handling colocal vertices.
        std::vector< std::pair<unsigned, Vec3> > ret;
        ret.reserve(points.size());

        if (nv::HalfEdge::Mesh* mesh = meshData->BuildHalfEdgeMesh())
        {
            for (auto handle : points)
            {
                NearestIndex<nv::HalfEdge::Vertex*> closest(0x0);
                //float closest = 1000.0f;
                //nv::HalfEdge::Vertex* closestVert = 0x0;

                auto vertIt = mesh->vertices();
                while (!vertIt.isDone())
                {
                    auto vertex = vertIt.current();

                    const float dist = (handle.first - vertex->pos).Length();
                    closest.Check(dist, vertex);
                    vertIt.advance();
                }

                if (closest.closest_)
                {
                    // Need to take the colocals so we deal with seams correctly
                    auto colocals = closest.closest_->colocals();
                    while (!colocals.isDone())
                    {
                        ret.push_back(std::make_pair(colocals.current()->id, handle.second));
                        colocals.advance();
                    }
                }
            }

            delete mesh;
        }

        return ret;
    }

    void LaplacianOperations::Deform(MeshData* meshData, const std::vector< std::pair<unsigned, Vec3> >& handles, float strength, bool useCotangentWeights)
    {
        Eigen::SparseMatrix<double> laplaceMatrix;
        Eigen::SparseMatrix<double> laplaceTransMatrix;
        Eigen::Matrix<double, Eigen::Dynamic, Eigen::Dynamic> deltaMatrix;

        const unsigned vertexCt = meshData->positionBuffer_.size();
        laplaceMatrix.resize(vertexCt, vertexCt);
        laplaceMatrix.setIdentity();

        nv::HalfEdge::Mesh* mesh = meshData->BuildHalfEdgeMesh();
        if (!mesh)
            return;

        auto vertIt = mesh->vertices();
        while (!vertIt.isDone())
        {
            auto vertex = vertIt.current();
            const unsigned vertIdx = vertex->id;

            Vec3 pt(0,0,0);
            ComputeLaplacian(vertex, laplaceMatrix, pt, useCotangentWeights);

            Vec3 delta = vertex->pos - pt;
            deltaMatrix.coeffRef(vertIdx, 0) = delta.x;
            deltaMatrix.coeffRef(vertIdx, 1) = delta.y;
            deltaMatrix.coeffRef(vertIdx, 2) = delta.z;

            vertIt.advance();
        }

        // Add deformation handles
        laplaceMatrix.conservativeResize(vertexCt + handles.size(), vertexCt);
        deltaMatrix.conservativeResize(vertexCt + handles.size(), 3);
        for (unsigned i = 0; i < handles.size(); ++i)
        {
            laplaceMatrix.coeffRef(vertexCt + i, handles[i].first) = 1.0f;
            const Vec3 pos = handles[i].second;
            deltaMatrix.coeffRef(vertexCt + i, 0) = pos.x;
            deltaMatrix.coeffRef(vertexCt + i, 1) = pos.y;
            deltaMatrix.coeffRef(vertexCt + i, 2) = pos.z;
        }

        laplaceTransMatrix = laplaceMatrix.transpose();

        Eigen::SparseMatrix<double> AtA = laplaceTransMatrix * laplaceMatrix;
        Eigen::Matrix<double, Eigen::Dynamic, Eigen::Dynamic> AtB = laplaceTransMatrix * deltaMatrix;
        Eigen::SimplicialCholesky<Eigen::SparseMatrix<double>, Eigen::Upper > solver(AtA);
        if (solver.info() != Eigen::Success)
        {
            SPRUE_LOG_ERROR("Failed to factorize for Laplacian deformation");
            delete mesh;
            return;
        }

        Eigen::Matrix<double, Eigen::Dynamic, Eigen::Dynamic> deformedPoints = solver.solve(AtB);
        if (solver.info() != Eigen::Success)
        {
            SPRUE_LOG_ERROR("Failed to solve Laplacian deformation");
            delete mesh;
            return;
        }

        // Update positions
        for (unsigned i = 0; i < meshData->positionBuffer_.size(); ++i)
            meshData->positionBuffer_[i] = meshData->positionBuffer_[i].Lerp(Vec3(deformedPoints.coeff(i, 0), deformedPoints.coeff(i, 1), deformedPoints.coeff(i, 2)), strength);

        delete mesh;
    }

    static void PrepareMeshData(const MeshData* meshData, MatrixXf& vertices, MatrixXi& faces, SparseMatrix<float>& L, SparseMatrix<float>& M)
    {
        vertices.resize(meshData->positionBuffer_.size(), 3);
        faces.resize(meshData->indexBuffer_.size() / 3, 3);

        for (int i = 0; i < meshData->positionBuffer_.size(); ++i)
        {
            vertices.coeffRef(i, 0) = meshData->positionBuffer_[i].x;
            vertices.coeffRef(i, 1) = meshData->positionBuffer_[i].y;
            vertices.coeffRef(i, 2) = meshData->positionBuffer_[i].z;
        }

        for (int i = 0; i < meshData->indexBuffer_.size(); i += 3)
        {
            faces.coeffRef(i / 3, 0) = meshData->indexBuffer_[i];
            faces.coeffRef(i / 3, 1) = meshData->indexBuffer_[i + 1];
            faces.coeffRef(i / 3, 2) = meshData->indexBuffer_[i + 2];
        }

        igl::cotmatrix(vertices, faces, L);
        igl::massmatrix(vertices, faces, igl::MASSMATRIX_TYPE_DEFAULT, M);        
    }

    /// Pinnochio style direct access weights
    static std::pair<float, float> CalculateHeatWeight(SparseMatrix<float>& PP, int vertIndIdx, int vertexIdx, int boneIdx, MeshData* meshData, kdTree& octree, std::vector<Skeleton::Bone>& bones)
    {
        const auto vertexPos = meshData->positionBuffer_[vertexIdx];
        const auto vertexNor = meshData->normalBuffer_[vertexIdx];
        const auto& currentBone = bones[boneIdx];

        bool visible = false;
        float weightSum = 0.0f;
        float minDist = FLT_MAX;

        auto onBone = currentBone.segment_.ClosestPoint(vertexPos);
        for (float i = 0; i < BONE_GLOW_ITERATIONS + 1.0f; ++i)
        {
            const float fraction = i * (1.0f / BONE_GLOW_ITERATIONS);

            const auto currentBonePos = currentBone.segment_.a.Lerp(currentBone.segment_.b, fraction);
            const auto toVertex = vertexPos - currentBonePos;
            if (toVertex.LengthSq() > currentBone.segment_.LengthSq() * 2)
                return std::make_pair<float, float>(0.0f, FLT_MAX);
            
            Vec3 hitPos;
            if (!octree.Hit(LineSegment(currentBonePos, vertexPos), vertexIdx))                                                                               
            {
                const float dist = currentBone.segment_.Distance(vertexPos);
                minDist = std::min(minDist, dist);
            }
        }

        return std::make_pair(weightSum, minDist);
    }

    /// Bone glow weights
    /// Return value as <WEIGHT, DISTANCE>
    static std::pair<float,float> CalculateGlowWeight(SparseMatrix<float>& PP, int vertIndIdx, int vertexIdx, int boneIdx, MeshData* meshData, kdTree& octree, std::vector<Skeleton::Bone>& bones)
    {
        const auto vertexPos = meshData->positionBuffer_[vertexIdx];
        const auto vertexNor = meshData->normalBuffer_[vertexIdx];
        const auto& currentBone = bones[boneIdx];

        bool visible = false;
        float weightSum = 0.0f;
        float minDist = FLT_MAX;

        auto onBone = currentBone.segment_.ClosestPoint(vertexPos);
        for (float i = 0; i < BONE_GLOW_ITERATIONS + 1.0f; ++i)
        {
            const float fraction = i * (1.0f / BONE_GLOW_ITERATIONS);
        
            const auto currentBonePos = currentBone.segment_.a.Lerp(currentBone.segment_.b, fraction);
            const auto toVertex = vertexPos - currentBonePos;
            const auto toVertexNor = toVertex.Normalized();
            //??if (toVertex.LengthSq() > currentBone.segment_.LengthSq() * 2)
            //??    return std::make_pair<float, float>(0.0f, FLT_MIN);
            
            Vec3 hitPos;
            if (!octree.Hit(LineSegment(currentBonePos, vertexPos), vertexIdx))
            {
                if (fabsf(currentBone.segment_.Dir().Dot(toVertex.Normalized())) < 0.45)
                    continue;

                minDist = std::min(minDist, currentBone.segment_.Distance(vertexPos));
                float lambert = fabsf(toVertexNor.Dot(vertexNor));
                lambert = SprueMax(lambert, 0.0f);
                weightSum += (lambert / toVertex.LengthSq()) * (toVertexNor.Cross(currentBone.segment_.Dir()).Length()) * BONE_GLOW_ITERATION_WEIGHT * currentBone.segment_.Length();
            }
        }

        PP.coeffRef(vertIndIdx, currentBone.startIndex_) = weightSum;// *currentBone.segment_.Length();
        return std::make_pair(weightSum, minDist);
    }

    /// Equalize the bone weights for each vertex
    static void BalanceBoneWeights(SparseMatrix<float>& PP)
    {
        for (int i = 0; i < PP.rows(); ++i)
        {
            const float sum = PP.row(i).sum();
            const float div = 1.0f / sum;
            for (int b = 0; b < PP.cols(); ++b)
                PP.coeffRef(i, b) *= div;
        }
    }

    static std::vector<unsigned> UniqueVertices(nv::HalfEdge::Mesh* mesh)
    {
        std::vector<unsigned> ret;

        for (int i = 0; i < mesh->vertexCount(); ++i)
            if (mesh->vertexAt(i)->isFirstColocal())
                ret.push_back(mesh->vertexAt(i)->id);

        return ret;
    }

    typedef std::pair<float,float> (*WEIGHTING_FUNCTION)(SparseMatrix<float>& PP, int vertIndIdx, int vertexIdx, int boneIdx, MeshData* meshData, kdTree& octree, std::vector<Skeleton::Bone>& bones);

    void LaplacianOperations::CalculateBoneWeightsV3(MeshData* meshData, Skeleton* skeleton, ResponseCurve curve, BoneWeightCancelCallback callback)
    {
        if (!meshData)
        {
            SPRUE_LOG_WARNING("Cannot calculate bone weights for a mesh whose data does not exist");
            return;
        }

        const unsigned rawVertexCt = meshData->positionBuffer_.size();
        const unsigned rawNormalCt = meshData->normalBuffer_.size();
        const unsigned rawIndexCt = meshData->indexBuffer_.size();
        if (rawVertexCt == 0 || rawNormalCt == 0 || rawIndexCt == 0 || rawVertexCt != rawNormalCt)
        {
            SPRUE_LOG_WARNING("Cannot calculate bone weights for a mesh without vertices");
            return;
        }

        if (!skeleton)
        {
            SPRUE_LOG_WARNING("Cannot calculate bone weights for a mesh without a skeleton");
            return;
        }

        std::vector<Skeleton::Bone> bones = skeleton->GetBones();
        const unsigned numBones = bones.size();
        auto joints = skeleton->GetAllJoints();
        const unsigned numJoints = joints.size();

        MatrixXf V;
        MatrixXi F;
        SparseMatrix<float> L, M;

        nv::HalfEdge::Mesh* heMesh = meshData->BuildHalfEdgeMesh();
        if (heMesh == 0x0)
        {
            SPRUE_LOG_ERROR("Unable to build half-edge mesh for bone weight calculation");
            return;
        }

        std::vector<unsigned> vertexIndices = UniqueVertices(heMesh);

        V.resize(vertexIndices.size(), 3);
        F.resize(meshData->indexBuffer_.size() / 3, 3);

        for (unsigned i = 0; i < vertexIndices.size(); ++i)
        {
            auto vert = heMesh->vertexAt(vertexIndices[i]);
            V.coeffRef(i, 0) = vert->pos.x;
            V.coeffRef(i, 1) = vert->pos.y;
            V.coeffRef(i, 2) = vert->pos.z;
        }

        for (unsigned i = 0; i < meshData->indexBuffer_.size(); i += 3)
        {
            const int indices[] = {
                meshData->indexBuffer_[i],
                meshData->indexBuffer_[i+1],
                meshData->indexBuffer_[i+2],
            };

            auto v = std::find(vertexIndices.begin(), vertexIndices.end(), heMesh->vertexAt(indices[0])->firstColocal()->id);
            F.coeffRef(i/3, 0) = std::distance(vertexIndices.begin(), v);

            v = std::find(vertexIndices.begin(), vertexIndices.end(), heMesh->vertexAt(indices[1])->firstColocal()->id);
            F.coeffRef(i/3, 1) = std::distance(vertexIndices.begin(), v);
            
            v = std::find(vertexIndices.begin(), vertexIndices.end(), heMesh->vertexAt(indices[2])->firstColocal()->id);
            F.coeffRef(i/3, 2) = std::distance(vertexIndices.begin(), v);
        }

        igl::cotmatrix(V, F, L);
        igl::massmatrix(V, F, igl::MASSMATRIX_TYPE_DEFAULT, M);

        VectorXf H;
        H.resize(vertexIndices.size());
        H.setZero();
        SparseMatrix<float> P;
        P.resize(vertexIndices.size(), numJoints);

        kdTreeConstructionData octreeData;
        octreeData.positionBuffer_ = meshData->positionBuffer_.data();
        octreeData.positionBufferLength_ = meshData->positionBuffer_.size();
        octreeData.indexBuffer_ = meshData->indexBuffer_.data();
        octreeData.indexBufferLength_ = meshData->indexBuffer_.size();
        octreeData.Pack();
        kdTree octree(octreeData);

        WEIGHTING_FUNCTION weightingMethod = CalculateHeatWeight;//CalculateGlowWeight;

        for (unsigned vertIndIdx = 0; vertIndIdx < vertexIndices.size(); ++vertIndIdx)
        {
            const unsigned vertexIdx = vertexIndices[vertIndIdx];

            int nearestJoint = -1;
            float minDist = FLT_MAX;
            for (unsigned boneIdx = 0; boneIdx < numBones; ++boneIdx)
            {
                const auto& currentBone = bones[boneIdx];
                if (currentBone.startIndex_ == 0)
                    continue;
                auto weight = weightingMethod(P, vertIndIdx, vertexIdx, boneIdx, meshData, octree, bones);
                if (weight.second < minDist)
                {
                    minDist = weight.second;
                    nearestJoint = bones[boneIdx].startIndex_;
                }
            }
            if (minDist != FLT_MAX)
                H.coeffRef(vertIndIdx) = 1.0f / (minDist*minDist);
            if (nearestJoint != -1)
            {
                float& dist = P.coeffRef(vertIndIdx, nearestJoint);
                dist = SprueMax(1.0f, dist);
            }

            if (callback && !callback())
            {
                delete heMesh;
                return;
            }
        }

        if (weightingMethod == CalculateGlowWeight)
            BalanceBoneWeights(P);

        auto HH = H.asDiagonal();
        Eigen::SimplicialLLT<Eigen::SparseMatrix<float>, Eigen::Upper > solver;
        solver.compute(-L + M*HH);
        if (solver.info() != Eigen::Success)
        {
            delete heMesh;
            if (solver.info() == Eigen::InvalidInput)
                SPRUE_LOG_ERROR("Bone weight inputs are invaid");
            else
                SPRUE_LOG_ERROR("Numerical issue encountered in solving bone weights");
            return;
        }

        Eigen::Matrix<float, Eigen::Dynamic, Eigen::Dynamic> diffusedWeights = solver.solve(HH/**M*/*P);
        if (solver.info() == Eigen::Success)
        {
            std::vector<BoneWeights> boneWeights(rawVertexCt);
            for (unsigned j = 0; j < numJoints; ++j)
            {
                for (unsigned v = 0; v < vertexIndices.size(); ++v)
                {
                    float w = diffusedWeights.coeffRef(v, j);
                    auto vert = heMesh->vertexAt(vertexIndices[v]);
                    auto colocalIt = vert->colocals();
                    while (!colocalIt.isDone())
                    {
                        boneWeights[colocalIt.current()->id].AddBoneWeight(j, w);
                        colocalIt.advance();
                    }
                }
            }

            meshData->boneWeights_ = std::vector<SprueEngine::Vec4>(rawVertexCt, Vec4(0, 0, 0, 0));
            meshData->boneIndices_ = std::vector<SprueEngine::IntVec4>(rawVertexCt, IntVec4(-1, -1, -1, -1));

            for (unsigned v = 0; v < rawVertexCt; ++v)
            {
                auto ind = boneWeights[v].indices_;
                auto vec = boneWeights[v].weights_.Normalized();
                for (int i = 0; i < 4; ++i)
                {
                    if (ind[i] != -1)
                    {
                        vec[i] = curve.GetValue(vec[i]);
                        if (vec[i] <= 0.0f)
                            ind[i] = -1;
                    }
                }
                vec.Normalize();
                meshData->boneIndices_[v] = ind;
                meshData->boneWeights_[v] = vec;
            }
        }
        else
        {
            delete heMesh;
            SPRUE_LOG_WARNING("Failed to find a solution for skin weighting");
            return;
        }
    }

    void LaplacianOperations::CalculateBoneWeightsV2(MeshData* meshData, Skeleton* skeleton, BoneWeightCancelCallback callback)
    {
        if (!meshData)
        {
            SPRUE_LOG_WARNING("Cannot calculate bone weights for a mesh whose data does not exist");
            return;
        }

        const unsigned vertexCt = meshData->positionBuffer_.size();
        const unsigned normalCt = meshData->normalBuffer_.size();
        const unsigned indexCt = meshData->indexBuffer_.size();
        if (vertexCt == 0 || normalCt == 0 || indexCt == 0 || vertexCt != normalCt)
        {
            SPRUE_LOG_WARNING("Cannot calculate bone weights for a mesh without vertices");
            return;
        }

        if (!skeleton)
        {
            SPRUE_LOG_WARNING("Cannot calculate bone weights for a mesh without a skeleton");
            return;
        }

        std::vector<Skeleton::Bone> bones = skeleton->GetBones();
        const unsigned numBones = bones.size();
        auto joints = skeleton->GetAllJoints();
        const unsigned numJoints = joints.size();

        MatrixXf V;
        MatrixXi F;
        SparseMatrix<float> L, M;
        PrepareMeshData(meshData, V, F, L, M);

        VectorXf H;
        H.resize(vertexCt);
        H.setZero();
        SparseMatrix<float> P;
        P.resize(vertexCt, numJoints);

        kdTreeConstructionData octreeData;
        octreeData.positionBuffer_ = meshData->positionBuffer_.data();
        octreeData.positionBufferLength_ = meshData->positionBuffer_.size();
        octreeData.indexBuffer_ = meshData->indexBuffer_.data();
        octreeData.indexBufferLength_ = meshData->indexBuffer_.size();
        octreeData.Pack();
        kdTree octree(octreeData);

        for (unsigned vertexIdx = 0; vertexIdx < meshData->positionBuffer_.size(); ++vertexIdx)
        {
            int nearestJoint = -1;
            float minDist = FLT_MAX;
            for (unsigned boneIdx = 0; boneIdx < numBones; ++boneIdx)
            {
                const auto& currentBone = bones[boneIdx];
                if (currentBone.startIndex_ == 0)
                    continue;
                auto weight = CalculateGlowWeight(P, vertexIdx, vertexIdx, boneIdx, meshData, octree, bones);
                if (weight.second < minDist)
                {
                    minDist = weight.second;
                    nearestJoint = bones[boneIdx].startIndex_;
                }
            }
            if (minDist != FLT_MAX)
                H.coeffRef(vertexIdx) = minDist;
            if (nearestJoint != -1)
                P.coeffRef(vertexIdx, nearestJoint) = 1.0f;

            if (callback && !callback())
                return;
        }

        //BalanceBoneWeights(P);

        auto HH = H.asDiagonal();
        Eigen::SimplicialLLT<Eigen::SparseMatrix<float>, Eigen::Upper > solver;
        solver.compute(-L + M*HH);
        if (solver.info() != Eigen::Success)
        {
            SPRUE_LOG_ERROR("Numerical issue encountered in solving bone weights");
            return;
        }

        Eigen::Matrix<float, Eigen::Dynamic, Eigen::Dynamic> diffusedWeights = solver.solve(HH*M*P);
        if (solver.info() == Eigen::Success)
        {
            //igl::normalize_row_sums(diffusedWeights, diffusedWeights);
            std::vector<BoneWeights> boneWeights(vertexCt);
            for (unsigned j = 0; j < numJoints; ++j)
            {
                for (unsigned v = 0; v < vertexCt; ++v)
                    boneWeights[v].AddBoneWeight(j, diffusedWeights.coeff(v, j));
            }

            meshData->boneWeights_ = std::vector<SprueEngine::Vec4>(vertexCt, Vec4(0,0,0,0));
            meshData->boneIndices_ = std::vector<SprueEngine::IntVec4>(vertexCt, IntVec4(-1, -1, -1, -1));

            for (unsigned v = 0; v < vertexCt; ++v)
            {
                auto ind = boneWeights[v].indices_;
                meshData->boneIndices_[v] = ind;
                auto vec = boneWeights[v].weights_;// .Normalized();
                meshData->boneWeights_[v] = vec;
            }
        }
        else
        {
            SPRUE_LOG_WARNING("Failed to find a solution for skin weighting");
            return;
        }
    }

    void LaplacianOperations::CalculateBoneWeights(MeshData* meshData, Skeleton* skeleton)
    {
        if (!meshData)
        {
            SPRUE_LOG_WARNING("Cannot calculate bone weights for a mesh whose data does not exist");
            return;
        }

        const unsigned vertexCt = meshData->positionBuffer_.size();
        if (vertexCt == 0)
        {
            SPRUE_LOG_WARNING("Cannot calculate bone weights for a mesh without vertices");
            return;
        }

        if (!skeleton)
        {
            SPRUE_LOG_WARNING("Cannot calculate bone weights for a mesh without a skeleton");
            return;
        }

        std::vector<Skeleton::Bone> bones = skeleton->GetBones();
        const unsigned numBones = bones.size();
        auto joints = skeleton->GetAllJoints();
        const unsigned numJoints = joints.size();

        Eigen::MatrixXd V;
        V.resize(meshData->positionBuffer_.size(), 3);
        Eigen::MatrixXd C;
        C.resize(numJoints, 3);
        Eigen::MatrixXi F;
        F.resize(meshData->indexBuffer_.size() / 3, 3);
        Eigen::MatrixXi P;
        P.resize(numJoints, 1);
        Eigen::MatrixXi BE;
        BE.resize(numBones, 2);
        Eigen::MatrixXi CE;
        Eigen::MatrixXd W;
        W.resize(meshData->positionBuffer_.size(), numJoints + numBones);

        for (int i = 0; i < meshData->positionBuffer_.size(); ++i)
        {
            V.coeffRef(i, 0) = meshData->positionBuffer_[i].x;
            V.coeffRef(i, 1) = meshData->positionBuffer_[i].y;
            V.coeffRef(i, 2) = meshData->positionBuffer_[i].z;
        }

        for (int i = 0; i < meshData->indexBuffer_.size(); i += 3)
        {
            F.coeffRef(i / 3, 0) = meshData->indexBuffer_[i];
            F.coeffRef(i / 3, 1) = meshData->indexBuffer_[i + 1];
            F.coeffRef(i / 3, 2) = meshData->indexBuffer_[i + 2];
        }

        //for (int i = 0; i < numJoints; ++i)
        //{
        //    C.coeffRef(i, 0) = joints[i]->GetPosition().x;
        //    C.coeffRef(i, 1) = joints[i]->GetPosition().y;
        //    C.coeffRef(i, 2) = joints[i]->GetPosition().z;
        //    P.coeffRef(i, 0) = skeleton->IndexOf(joints[i]);
        //}
        //
        //for (int i = 0; i < numBones; ++i)
        //{
        //    BE.coeffRef(i, 0) = bones[i].startIndex_;
        //    BE.coeffRef(i, 1) = bones[i].endIndex_;
        //}
        //
        //Eigen::MatrixXd Vsurf = V.topLeftCorner(F.maxCoeff() + 1, V.cols());
        //Eigen::MatrixXd Wsurf;
        //igl::embree::bone_heat(Vsurf, F, C, Eigen::VectorXi(), BE, Eigen::MatrixXi(), Wsurf);

        Eigen::SparseMatrix<double> Q, L, M, PP;
        igl::cotmatrix(V, F, L);
        igl::massmatrix(V, F, igl::MASSMATRIX_TYPE_DEFAULT, M);
        PP.resize(vertexCt, numJoints);

#if 1
        nv::HalfEdge::Mesh* mesh = meshData->BuildHalfEdgeMesh();
        if (!mesh)
            return;

        std::vector<BoneWeights> boneWeights(vertexCt);

        MeshOctreeConstructionData octreeData;
        octreeData.vertices = meshData->positionBuffer_.data();
        octreeData.normals = meshData->normalBuffer_.data();
        octreeData.indices = meshData->indexBuffer_.data();
        octreeData.vertexCt = meshData->positionBuffer_.size();
        octreeData.indexCt = meshData->indexBuffer_.size();

        MeshOctree octree(octreeData);

        std::vector<float> boneHeat(numBones * vertexCt, 0.0f);
        std::vector<float> vertexAreas(vertexCt, 0.0f);

        std::vector<bool> visibility(numBones * vertexCt, false);
        auto vertIt = mesh->vertices();
        while (!vertIt.isDone())
        {
            auto vertex = vertIt.current();

            // Compute the surface area surround each vertex
            //auto edges = vertex->edges();
            //nv::HalfEdge::Edge* lastEdge = 0x0;
            //while (!edges.isDone())
            //{
            //    if (lastEdge)
            //        vertexAreas[vertex->id] += math::Triangle(vertex->pos, lastEdge->to()->pos, edges.current()->to()->pos).Area();
            //    lastEdge = edges.current();
            //    edges.advance();
            //}
            //vertexAreas[vertex->id] = 1.0f / (1e-10 + vertexAreas[vertex->id]);

            float minDist = FLT_MAX;
            for (unsigned boneIdx = 0; boneIdx < numBones; ++boneIdx)
            {
                Skeleton::Bone& currentBone = bones[boneIdx];
                if (currentBone.segment_.Length() < FLT_EPSILON)
                    continue;

                bool visible = false;
                for (unsigned i = 0; i < BONE_GLOW_ITERATIONS; ++i)
                {
                    Vec3 bonePt = currentBone.segment_.a + (currentBone.segment_.b - currentBone.segment_.a) * (i * BONE_GLOW_ITERATION_WEIGHT);
                
                    if (fabsf((vertex->pos - bonePt).Normalized().Dot(currentBone.segment_.Dir())) < 0.15f)
                    {
                        // Check for line of sight to the vertex
                        auto vecTo = (vertex->pos - bonePt);
                        float dist = meshData->IntersectRay(Ray(bonePt, vecTo.Normalized()), false);
                        if (dist <= SprueEquals(vecTo.Length(), dist))
                            visible = true;
                        //std::vector<unsigned> possibleResults;
                        //octree.CollectLine(bonePt, vertex->pos, possibleResults);
                        //visible = octree.TestLine(bonePt, vertex->pos, possibleResults, vertex->id);
                    }

                }

                if (visible)
                {
                    visibility[boneIdx * vertexCt + vertex->id] = true;
                    minDist = std::min(currentBone.segment_.Distance(vertex->pos), minDist);
                }
            }

            // Update the minimum value if needed
            for (unsigned boneIdx = 0; boneIdx < numBones && minDist != FLT_MAX; ++boneIdx)
            {
                if (!visibility[boneIdx * vertexCt + vertex->id])
                    continue;

                Skeleton::Bone& currentBone = bones[boneIdx];

                // Do not process the root bone
                if (currentBone.startIndex_ == 0)
                    continue;

                const Vec3 boneStart = currentBone.segment_.a;
                const float boneLength = currentBone.segment_.LengthSq();

                // Do not process infinitely small points
                if (boneLength < FLT_EPSILON)
                    continue;

                const Vec3 boneVec = currentBone.segment_.b - currentBone.segment_.a;
                const Vec3 normalizedBoneVec = currentBone.segment_.Dir();

                float heat = 0.0f;
                //for (unsigned i = 0; i < BONE_GLOW_ITERATIONS; ++i)
                //{
                //    Vec3 bonePt = boneStart + boneVec * (i * BONE_GLOW_ITERATION_WEIGHT);
                //    
                //    // Check for line of sight to the vertex
                //    std::vector<unsigned> possibleResults;
                //    octree.CollectLine(bonePt, vertex->pos, possibleResults);
                //    if (!octree.TestLine(bonePt, vertex->pos, possibleResults, vertex->id))
                //        continue;
                //    visible = true;
                //
                //    Vec3 toBone = (vertex->pos - bonePt);
                //    const float toBoneDot = fabsf(vertex->nor.Dot(normalizedBoneVec));// *fabsf(toBone.Normalized().Dot(normalizedBoneVec));
                //    float falloff = 1.0f - toBoneDot;
                //    heat += std::max(0.0f, falloff * BONE_GLOW_ITERATION_WEIGHT) * (boneLength / toBone.LengthSq());
                //}
                //if (!visible)
                //    continue;
                heat = currentBone.segment_.DistanceSq(vertex->pos);
                if (heat <= minDist)
                {
                    boneHeat[currentBone.startIndex_ * vertexCt + vertex->id] = NORMALIZE(heat, 0.0f, minDist);
                    boneWeights[vertex->id].AddBoneWeight(currentBone.startIndex_, boneHeat[currentBone.startIndex_ * vertexCt + vertex->id]);
                }
            }

            vertIt.advance();
        }

        Eigen::SparseMatrix<double> laplaceMatrix;
        laplaceMatrix.resize(vertexCt, vertexCt);
        laplaceMatrix.setIdentity();

        // Do regular laplacian
        vertIt = mesh->vertices();
        igl::cotmatrix(V, F, laplaceMatrix);
        //??while (!vertIt.isDone())
        //??{
        //??    auto vertex = vertIt.current();
        //??    Vec3 pt(0,0,0);
        //??    //ComputeLaplacian(vertex, laplaceMatrix, pt, true);
        //??    vertIt.advance();
        //??}
        //??laplaceMatrix = L;
        
        Eigen::SparseMatrix<double> laplaceTransMatrix = laplaceMatrix.transpose();
        Eigen::SparseMatrix<double> AtA = laplaceTransMatrix * laplaceMatrix;
        
        //MatrixXd D(vertexCt, numBones);
        //MatrixXd PP = MatrixXd::Zero(vertexCt, numBones);
        //VectorXd min_D;
        //VectorXd Hdiag = VectorXd::Zero(vertexCt);
        //VectorXi J;
        //
        //for (unsigned v = 0; v < vertexCt; ++v)
        //    for (unsigned n = 0; n < numBones; ++n)
        //        D(v, n) = boneHeat[n * vertexCt + v];
        //
        //igl::mat_min(D, 2, min_D, J);
        //
        //for (unsigned v = 0; v < vertexCt; ++v)
        //{
        //    PP(v, J(v)) = 1;
        //    if (visibility[J(v) * vertexCt + v])
        //    {
        //        double hii = pow(min_D(v), -2.);
        //        Hdiag(v) = (hii>1e10 ? 1e10 : hii);
        //    }
        //}
        //const auto & H = Hdiag.asDiagonal();
        //Q = (-L + M*H);
        //SimplicialLLT <SparseMatrix<double > > llt;
        //llt.compute(Q);
        //const auto& w = llt.solve(M*H*PP);
        //
        //for (unsigned v = 0; v < vertexCt; ++v)
        //{
        //    for (unsigned b = 0; b < numBones; ++b)
        //        boneWeights[v].AddBoneWeight(b, w(v, b));
        //}

        for (unsigned boneIdx = 0; boneIdx < numBones; ++boneIdx)
        {
            Skeleton::Bone& bone = bones[boneIdx];
            Eigen::Matrix<double, Eigen::Dynamic, Eigen::Dynamic> deltaMatrix;
            deltaMatrix.resize(vertexCt, 1);
            for (unsigned v = 0; v < vertexCt; ++v)
                deltaMatrix.coeffRef(v, 0) = boneHeat[bone.startIndex_ * vertexCt + v];// / vertexAreas[v];
        
            auto H = deltaMatrix.asDiagonal();
            Eigen::SimplicialLLT<Eigen::SparseMatrix<double>, Eigen::Upper > solver(-laplaceMatrix + M*H);
            Eigen::Matrix<double, Eigen::Dynamic, Eigen::Dynamic> AtB = laplaceTransMatrix * H;
            Eigen::Matrix<double, Eigen::Dynamic, Eigen::Dynamic> diffusedWeights = solver.solve(AtB);
            if (solver.info() == Eigen::Success)
            {
                for (unsigned v = 0; v < vertexCt; ++v)
                    boneWeights[v].AddBoneWeight(bone.startIndex_, diffusedWeights.coeff(v, 0));
            }
            else
                SPRUE_LOG_WARNING(FString("Failed to find a solution for bone at index: %1", bone.startIndex_));
        }

        // Now that we've computed everything it is time to transfer the weights over to the mesh data.
        //meshData->boneIndices_.resize(vertexCt);
        //meshData->boneWeights_.resize(vertexCt);
        meshData->colorBuffer_.resize(vertexCt);

        RGBA indexColors[] = {
            RGBA::Red,
            RGBA::Green,
            RGBA::Blue
        };

        auto toHeatMap = [](float value, float minimum, float maximum) {
            float ratio = 2 * (value - minimum) / (maximum - minimum);
            float b = int(std::max(0, (int)(255 * (1 - ratio))));
            float r = int(std::max(0, (int)(255 * (ratio - 1))));
            float g = 255 - b - r;
            return RGBA(r/255.0f, g/255.0f, b/255.0f, 1.0f);
        };

        meshData->boneIndices_.resize(boneWeights.size());
        meshData->boneWeights_.resize(boneWeights.size());
        for (unsigned v = 0; v < vertexCt; ++v)
        {
            auto ind = boneWeights[v].indices_;
            auto vec = boneWeights[v].weights_;// .Normalized();

            //meshData->colorBuffer_[v] = RGBA::Black;
            //for (int i = 0; i < 4; ++i)
            //{
            //    if (ind[i] == 1 && vec[i] > 0.01f)
            //        meshData->colorBuffer_[v] = toHeatMap(vec[i]*vec[i], 0.0f, 1.0f);
            //}

           // meshData->colorBuffer_[v] = ind.w_ != -1 ? indexColors[ind.w_] : RGBA::Black;
           // if (ind.z_ != -1)
           //     meshData->colorBuffer_[v] = SprueLerp(meshData->colorBuffer_[v], indexColors[ind.z_], 0.5f);
           //meshData->colorBuffer_[v] = RGBA(vec.w, vec.z, vec.y, 1.0f);

            meshData->boneIndices_[v] = ind;
            meshData->boneWeights_[v] = vec;

           //meshData->boneIndices_[v] = boneWeights[v].indices_;
           //meshData->boneWeights_[v] = boneWeights[v].weights_.Normalized();
        }
#endif
    }
}
