#include "FBXLoader.h"

#include "../FString.h"
#include "../GeneralUtility.h"
#include "../Animation/LegacySequence.h"
#include "../Geometry/MeshData.h"
#include "../Geometry/Skeleton.h"
#include "../Logging.h"
#include "../Resource.h"

#include <fbxsdk.h>
#include <fbxsdk/utils/fbxgeometryconverter.h>

namespace SprueEngine
{

const std::string FBXLoader::resourceURI_("Mesh");
const StringHash  FBXLoader::typeHash_("Mesh");

Mat4x4 ConvertFbxMatrix(const FbxMatrix& matrix)
{
    auto mat = matrix.Transpose();
    return Mat4x4(mat.Buffer()->Buffer());
}

Mat4x4 ConvertFbxMatrix(const FbxAMatrix& matrix)
{
    auto mat = matrix.Transpose();
    return Mat4x4(mat.Buffer()->Buffer());
}

FbxMatrix ToFbxMatrix(const Mat4x4& matrix)
{
    auto mat = matrix.Transposed();
    return FbxMatrix(
        mat.ptr()[0],  mat.ptr()[1],  mat.ptr()[2], mat.ptr()[3],
        mat.ptr()[4],  mat.ptr()[5],  mat.ptr()[6], mat.ptr()[7],
        mat.ptr()[8],  mat.ptr()[9],  mat.ptr()[10], mat.ptr()[11],
        mat.ptr()[12], mat.ptr()[13], mat.ptr()[14], mat.ptr()[15]
        );
}

// Get the matrix of the given pose
FbxAMatrix FBX_GetPoseMatrix(FbxPose* pPose, int pNodeIndex)
{
    FbxAMatrix lPoseMatrix;
    FbxMatrix lMatrix = pPose->GetMatrix(pNodeIndex);

    memcpy((double*)lPoseMatrix, (double*)lMatrix, sizeof(lMatrix.mData));

    return lPoseMatrix;
}

// Get the global position of the node for the current pose.
// If the specified node is not part of the pose or no pose is specified, get its
// global position at the current time.
FbxAMatrix FBX_GetGlobalPosition(FbxNode* pNode, const FbxTime& pTime, FbxPose* pPose, FbxAMatrix* pParentGlobalPosition = 0x0)
{
    FbxAMatrix lGlobalPosition;
    bool        lPositionFound = false;

    if (pPose)
    {
        int lNodeIndex = pPose->Find(pNode);

        if (lNodeIndex > -1)
        {
            // The bind pose is always a global matrix.
            // If we have a rest pose, we need to check if it is
            // stored in global or local space.
            if (pPose->IsBindPose() || !pPose->IsLocalMatrix(lNodeIndex))
            {
                lGlobalPosition = FBX_GetPoseMatrix(pPose, lNodeIndex);
            }
            else
            {
                // We have a local matrix, we need to convert it to
                // a global space matrix.
                FbxAMatrix lParentGlobalPosition;

                if (pParentGlobalPosition)
                {
                    lParentGlobalPosition = *pParentGlobalPosition;
                }
                else
                {
                    if (pNode->GetParent())
                    {
                        lParentGlobalPosition = FBX_GetGlobalPosition(pNode->GetParent(), pTime, pPose);
                    }
                }

                FbxAMatrix lLocalPosition = FBX_GetPoseMatrix(pPose, lNodeIndex);
                lGlobalPosition = lParentGlobalPosition * lLocalPosition;
            }

            lPositionFound = true;
        }
    }

    if (!lPositionFound)
    {
        // There is no pose entry for that node, get the current global position instead.

        // Ideally this would use parent global position and local position to compute the global position.
        // Unfortunately the equation 
        //    lGlobalPosition = pParentGlobalPosition * lLocalPosition
        // does not hold when inheritance type is other than "Parent" (RSrs).
        // To compute the parent rotation and scaling is tricky in the RrSs and Rrs cases.
        lGlobalPosition = pNode->EvaluateGlobalTransform(pTime);
    }

    return lGlobalPosition;
}

std::string FBXLoader::GetResourceURIRoot() const
{
    return resourceURI_;
}

StringHash FBXLoader::GetResourceTypeID() const
{
    return typeHash_;
}

std::shared_ptr<Resource> FBXLoader::LoadResource(const char* path) const
{
    FbxManager* lSdkManager = FbxManager::Create();

    FbxIOSettings * ios = FbxIOSettings::Create(lSdkManager, IOSROOT);
    lSdkManager->SetIOSettings(ios);

    lSdkManager->GetIOSettings()->SetBoolProp(IMP_FBX_MATERIAL, true);
    lSdkManager->GetIOSettings()->SetBoolProp(IMP_FBX_TEXTURE, true);
    lSdkManager->GetIOSettings()->SetBoolProp(IMP_FBX_LINK, false);
    lSdkManager->GetIOSettings()->SetBoolProp(IMP_FBX_GOBO, false);
    lSdkManager->GetIOSettings()->SetBoolProp(IMP_FBX_ANIMATION, true);
    lSdkManager->GetIOSettings()->SetBoolProp(IMP_FBX_GLOBAL_SETTINGS, true);

    FbxImporter* lImporter = FbxImporter::Create(lSdkManager, "");
    bool lImportStatus = lImporter->Initialize(path, -1, lSdkManager->GetIOSettings());

    if (!lImportStatus) {
        SPRUE_LOG_ERROR(FString("Call to FbxImporter::Initialize() failed.\nError returned: %1\n\n", lImporter->GetStatus().GetErrorString()).str());
        lImporter->Destroy();
        ios->Destroy();
        lSdkManager->Destroy();
        lImporter->Destroy();
        return 0x0;
    }

    FbxScene* scene = FbxScene::Create(lSdkManager, "myScene");
    if (!lImporter->Import(scene) || !scene)
    {
        SPRUE_LOG_ERROR(FString("Failed import of FbxScene: %1", path).str());
        lImporter->Destroy();
        ios->Destroy();
        lSdkManager->Destroy();
        if (scene)
            scene->Destroy();
        return 0x0;
    }

    auto headerInfo = lImporter->GetFileHeaderInfo();
    bool fromBlender = false;
    if (headerInfo && headerInfo->mCreator.Lower().Find("blender") != -1)
        fromBlender = true;

    // Convert into our axis system
    FbxAxisSystem dxStyleAxes(FbxAxisSystem::eOpenGL);
    dxStyleAxes.ConvertScene(scene);

    // Convert any quads/polys into triangles
    FbxGeometryConverter geoConverter(lSdkManager);
    geoConverter.Triangulate(scene, true);

    Skeleton* skeleton = ReadSkeleton(scene);

    SPRUE_LOG_INFO(FString("Processing FBX Scene: %1", scene->GetName()).str());
    std::vector<MeshData*> meshes;
    for (unsigned i = 0; i < scene->GetNodeCount(); ++i)
    {
        FbxNode* node = scene->GetNode(i);
        if (FbxMarker* marker = node->GetMarker())
        {
            if (marker->GetType() == FbxMarker::eStandard) // Standard marker for whatever purpose
            {

            }
            else if (marker->GetType() == FbxMarker::eEffectorIK) // IK effector marker
            {

            }
        }
        else if (auto mesh = node->GetMesh())
        {
            SPRUE_LOG_INFO(FString("Processing FBX Mesh: %1", node->GetName()).str());
            Mat4x4 transform = ConvertFbxMatrix(node->EvaluateGlobalTransform());
            /// Maps Control-Point-Index -> Vertex-Index for duplicates, vertex buffer can safely be assumed to be valid
            std::multimap<unsigned, unsigned> controlPointsTable;

            int polyCount = mesh->GetPolygonCount();
            std::vector<Vertex> vertices;
            std::vector<unsigned> indices;
            vertices.reserve(mesh->GetControlPointsCount());
            indices.reserve(polyCount * 3);

            unsigned vertexIndex = 0;
            for (int poly = 0; poly < polyCount; ++poly)
            {
                for (int vert = 0; vert < mesh->GetPolygonSize(poly); ++vert, ++vertexIndex)
                {
                    unsigned vidx = vertices.size();
                    //if (vert == 3)
                    //{
                    //    indices.push_back(indices[indices.size()-3]);
                    //    indices.push_back(indices[indices.size()-1]);
                    //}
                    indices.push_back(vidx);
                    vertices.push_back(Vertex());
                    //if (vert == 2)
                    //    std::swap(indices[indices.size() - 3], indices[indices.size() - 1]);

                    int vertIndex = mesh->GetPolygonVertex(poly, vert);
                    controlPointsTable.insert(std::make_pair(vertIndex, vidx));

                    /// Position
                    FbxVector4 position = mesh->GetControlPointAt(vertIndex);
                    vertices[vidx].position_.Set(position.mData[0], position.mData[1], position.mData[2]);
                    vertices[vidx].position_ = transform * vertices[vidx].position_;

                    // Normals
                    int normalCt = mesh->GetElementNormalCount();
                    if (normalCt)
                        vertices[vidx].normal_ = ReadNormal(vertIndex, vertexIndex, mesh);
                    
                    // UV coordinates
                    for (int uvLayer = 0; uvLayer < mesh->GetElementUVCount() && uvLayer < 2; ++uvLayer)
                    {
                        int uvIndex = mesh->GetTextureUVIndex(poly, vert);
                        if (uvLayer == 0)
                            vertices[vidx].uv_ = ReadUV(vertIndex, uvIndex, uvLayer, mesh);
                        else
                        {
                            // set as lightmap UV (UV1)
                        }
                    }
                }
            }

            // Bone weights, only bother with them if we have a skeleton
            if (skeleton)
            {
                const int boneSets = mesh->GetDeformerCount(FbxDeformer::eSkin);
                for (unsigned i = 0; i < boneSets; ++i)
                {
                    int clusterCount = ((FbxSkin*)mesh->GetDeformer(i, FbxDeformer::eSkin))->GetClusterCount();
                    for (int clusterIdx = 0; clusterIdx < clusterCount; ++clusterIdx)
                    {
                        FbxCluster* cluster = ((FbxSkin*)mesh->GetDeformer(i, FbxDeformer::eSkin))->GetCluster(clusterIdx);

                        int boneIndex = 0;
                        if (Joint* found = skeleton->GetByUserData(cluster->GetLink()))
                            boneIndex = skeleton->IndexOf(found);
                        else
                            continue;

                        const int indexCount = cluster->GetControlPointIndicesCount();
                        int* indices = cluster->GetControlPointIndices();
                        double* weights = cluster->GetControlPointWeights();

                        for (int k = 0; k < indexCount; k++)
                        {
                            const int ctrlPoint = indices[k];
                            const float weight = (float)weights[k];

                            // Map control point index onto propert duplicate vertices
                            auto range = controlPointsTable.equal_range(ctrlPoint);
                            for (auto duplicate = range.first; duplicate  != range.second; ++duplicate)
                                vertices[duplicate->second].boneWeights_.AddBoneWeight(boneIndex, weight);
                        }
                    }
                }
            }

            std::vector<SprueEngine::MorphTarget> morphTargets;
            /// Morph targets
            int morphTargetCt = mesh->GetDeformerCount(FbxDeformer::eBlendShape);
            for (int morphTargetIndex = 0; morphTargetIndex < morphTargetCt; ++morphTargetIndex)
            {
                FbxBlendShape* morphTarget = (FbxBlendShape*)mesh->GetDeformer(morphTargetIndex, FbxDeformer::eBlendShape);
                int channelCt = morphTarget->GetBlendShapeChannelCount();

                for (int channelIdx = 0; channelIdx < channelCt; ++channelIdx)
                {
                    SprueEngine::MorphTarget target;
                    target.data.resize(vertices.size());

                    FbxBlendShapeChannel* channel = morphTarget->GetBlendShapeChannel(channelIdx);
                    target.name = channel->GetName();
                    int targetShapeCount = channel->GetTargetShapeCount();
                    for (int targetShapeIdx = 0; targetShapeIdx < targetShapeCount; ++targetShapeIdx)
                    {
                        FbxShape* shape = channel->GetTargetShape(targetShapeIdx);
                        // counts
                        const int ctrlPtCt = shape->GetControlPointsCount();
                        FbxLayerElementArrayTemplate<FbxVector4>* normals = 0x0, *binormals = 0x0, *tangents = 0x0;

                        const bool hasNormals = shape->GetNormals(&normals);
                        const bool hasBinormal = shape->GetBinormals(&binormals);
                        const bool hasTangents = shape->GetTangents(&tangents);

                        if (ctrlPtCt > 0)
                        {
                            Vertex v;
                            for (int ctrlPtIdx = 0; ctrlPtIdx < ctrlPtCt; ++ctrlPtIdx)
                            {
                                FbxVector4 pos = shape->GetControlPointAt(ctrlPtIdx);
                                FbxVector4 nor = hasNormals ? (*normals)[ctrlPtIdx] : FbxVector4(0, 0, 0, 0);
                                auto range = controlPointsTable.equal_range(ctrlPtIdx);
                                for (auto duplicate = range.first; duplicate != range.second; ++duplicate)
                                {
                                    if (fromBlender) // sigh
                                    {
                                        target.data[duplicate->second].Position = Vec3(-pos[0], pos[2], pos[1]);
                                        target.data[duplicate->second].Normal = Vec3(-nor[0], nor[2], nor[1]);
                                    }
                                    else
                                    {
                                        target.data[duplicate->second].Position = Vec3(pos[0], pos[1], pos[2]);
                                        target.data[duplicate->second].Normal = Vec3(nor[0], nor[1], nor[2]);
                                    }
                                }
                            }
                        }
                    }
                    if (target.data.size() > 0)
                        morphTargets.push_back(target);
                }
            }

            // If we have geometry than push back a mesh
            if (vertices.size() && indices.size())
            {
                const char* meshName = node->GetName() != 0x0 ? node->GetName() : mesh->GetName();
                if (MeshData* mesh = ToMeshData(meshName, vertices, indices))
                {
                    mesh->morphTargets_ = morphTargets;
                    mesh->CalculateBounds();
                    if (mesh->normalBuffer_.empty())
                        mesh->CalculateNormals();
                    if (mesh->tangentBuffer_.empty())
                        mesh->CalculateTangents();
                    meshes.push_back(mesh);
                }
            }
        }
    }

#if 0
    // Animation
    std::vector<LegacySequence*> sequences;
    if (skeleton)
    {
        const int srcObjects = scene->GetSrcObjectCount();
        for (int i = 0; i < srcObjects; ++i)
        {
            if (FbxAnimStack* animStack = scene->GetSrcObject<FbxAnimStack>(i))
            {
                LegacySequence* sequence = new LegacySequence();
                sequence->SetName(animStack->GetName());
                sequence->GetTimelines().resize(skeleton->GetAllJoints().size());

                if (FbxTakeInfo* take = scene->GetTakeInfo(animStack->GetName()))
                {
                    FbxTime start = take->mLocalTimeSpan.GetStart();
                    FbxTime end = take->mLocalTimeSpan.GetStop();
                    const int takeLength = end.GetFrameCount(FbxTime::eFrames30) - start.GetFrameCount(FbxTime::eFrames30);

                    for (FbxLongLong frame = start.GetFrameCount(FbxTime::eFrames30); frame <= end.GetFrameCount(FbxTime::eFrames30); ++frame)
                    {
                        FbxTime curTime;
                        curTime.SetFrame(frame, FbxTime::eFrames30);
                        for (unsigned jointIdx = 0; jointIdx < skeleton->GetAllJoints().size(); ++jointIdx)
                        {
                            Joint* joint = skeleton->GetAllJoints()[jointIdx];
                            FbxNode* jointNode = (FbxNode*)joint->GetUserData();
                            FbxMatrix transformOffset = jointNode->EvaluateGlobalTransform(curTime);

                            sequence->GetTimelines()[jointIdx].jointIndex_ = jointIdx;
                            LegacyKeyframe key;
                            
                            FbxVector4 pos, scl, shear; 
                            fbxsdk_2015_1::FbxQuaternion rot; 
                            double junk;
                            transformOffset.GetElements(pos, rot, shear, scl, junk);
                            
                            key.position_.Set(pos[0], pos[1], pos[2]);
                            key.rotation_.Set(rot[0], rot[1], rot[2], rot[3]);
                            key.scale_.Set(scl[0], scl[1], scl[2]);

                            sequence->GetTimelines()[jointIdx].frames_.push_back(key);
                        }
                    }
                }

                sequences.push_back(sequence);
            }
        }
    }
#endif

    scene->Destroy(true);
    lImporter->Destroy();
    ios->Destroy();
    lSdkManager->Destroy();

    //TODO add the LegacySequence vector to the mesh resource.
    std::shared_ptr<MeshResource> meshRes = std::make_shared<MeshResource>();
    meshRes->GetMeshes() = meshes;
    if (skeleton)
        meshRes->SetSkeleton(skeleton);

    return meshRes;
}

bool FBXLoader::CanLoad(const char* str) const
{
    return EndsWith(ToLower(str), ".fbx");
}

Vec3 FBXLoader::ReadNormal(int ctrlPoint, unsigned vertIndex, FbxMesh* mesh) const
{
    Vec3 outNorm;
    FbxGeometryElementNormal* vertexNormal = mesh->GetElementNormal(0);
    switch (vertexNormal->GetMappingMode())
    {
    case FbxGeometryElement::eByControlPoint:
        switch (vertexNormal->GetReferenceMode())
        {
        case FbxGeometryElement::eDirect: {
                auto vec = vertexNormal->GetDirectArray().GetAt(ctrlPoint);
                outNorm.x = vec.mData[0];
                outNorm.y = vec.mData[1];
                outNorm.z = vec.mData[2];
            } break;
        case FbxGeometryElement::eIndexToDirect: {
                int index = vertexNormal->GetIndexArray().GetAt(ctrlPoint);
                auto vec = vertexNormal->GetDirectArray().GetAt(index);
                outNorm.x = vec.mData[0];
                outNorm.y = vec.mData[1];
                outNorm.z = vec.mData[2];
            } break;
        default: {
                SPRUE_LOG_ERROR("Error in FbxMesh vertex normal");
                return outNorm;
            }
        }
        break;

    case FbxGeometryElement::eByPolygonVertex:
        switch (vertexNormal->GetReferenceMode())
        {
        case FbxGeometryElement::eDirect: {
                auto vec = vertexNormal->GetDirectArray().GetAt(vertIndex);
                outNorm.x = vec.mData[0];
                outNorm.y = vec.mData[1];
                outNorm.z = vec.mData[2];
            } break;
        case FbxGeometryElement::eIndexToDirect: {
                int index = vertexNormal->GetIndexArray().GetAt(vertIndex);
                auto vec = vertexNormal->GetDirectArray().GetAt(index);
                outNorm.x = vec.mData[0];
                outNorm.y = vec.mData[1];
                outNorm.z = vec.mData[2];
            } break;
        default: {
                SPRUE_LOG_ERROR("Error in FbxMesh vertex normal");
                return outNorm;
            }
        }
        break;
    }
    return outNorm;
}

Vec2 FBXLoader::ReadUV(int ctrlPoint, unsigned uvIndex, int layer, FbxMesh* mesh) const
{
    Vec2 outUV;
    if (layer >= 2 || mesh->GetElementUVCount() <= layer)
    {
        SPRUE_LOG_ERROR("Error in FbxMesh UV layers");
        return outUV;
    }

    FbxGeometryElementUV* vertexUV = mesh->GetElementUV(layer);
    switch (vertexUV->GetMappingMode())
    {
    case FbxGeometryElement::eByControlPoint:
        switch (vertexUV->GetReferenceMode())
        {
        case FbxGeometryElement::eDirect: {
                outUV.x = vertexUV->GetDirectArray().GetAt(ctrlPoint).mData[0];
                outUV.y = vertexUV->GetDirectArray().GetAt(ctrlPoint).mData[1];
            } break;

        case FbxGeometryElement::eIndexToDirect: {
                int index = vertexUV->GetIndexArray().GetAt(ctrlPoint);
                outUV.x = vertexUV->GetDirectArray().GetAt(index).mData[0];
                outUV.y = vertexUV->GetDirectArray().GetAt(index).mData[1];
            } break;
        default: {
                SPRUE_LOG_ERROR("Error in FbxMesh UV coordinates");
                return outUV;
            }
        }
        break;
    case FbxGeometryElement::eByPolygonVertex:
        switch (vertexUV->GetReferenceMode())
        {
        case FbxGeometryElement::eDirect:
        case FbxGeometryElement::eIndexToDirect: {
                outUV.x = vertexUV->GetDirectArray().GetAt(uvIndex).mData[0];
                outUV.y = vertexUV->GetDirectArray().GetAt(uvIndex).mData[1];
            } break;
        default: {
                SPRUE_LOG_ERROR("Error in FbxMesh UV coordinates");
                return outUV;
            }
        } break;
    }
    return outUV;
}

Skeleton* FBXLoader::ReadSkeleton(fbxsdk::FbxScene* scene) const
{
    int boneIndex = 0;
    Skeleton* skeleton = new Skeleton();

    for (int childIndex = 0; childIndex < scene->GetNodeCount(); ++childIndex)
    {
        FbxNode* curNode = scene->GetNode(childIndex);
        if (curNode->GetNodeAttribute() && curNode->GetNodeAttribute()->GetAttributeType() == FbxNodeAttribute::eSkeleton)
        {
            BuildSkeletonRecurse(skeleton, curNode, 0x0);
            break;
        }
    }

    if (skeleton->GetAllJoints().size() == 0)
    {
        auto rootNode = scene->GetRootNode();
        for (int childIndex = 0; childIndex < rootNode->GetChildCount(); ++childIndex)
        {
            FbxNode* curNode = rootNode->GetChild(childIndex);
            BuildSkeletonRecurse(skeleton, curNode, 0x0);
        }
    }

    if (skeleton->GetAllJoints().size())
        return skeleton;

    delete skeleton;
    return 0x0;
}

void FBXLoader::BuildSkeletonRecurse(Skeleton* skeleton, fbxsdk::FbxNode* node, Joint* currentJoint) const
{
    if (node->GetNodeAttribute() && node->GetNodeAttribute()->GetAttributeType() == FbxNodeAttribute::eSkeleton)
    {
        do {
            FbxSkeleton* skel = node->GetSkeleton();
            int poseCt = skel->GetScene()->GetPoseCount();
            if (poseCt > 1)
                SPRUE_LOG_WARNING(FString("Contains %1 poses", poseCt).str());
            auto pose = skel->GetScene()->GetPose(0);
            if (skel->GetSkeletonType() == FbxSkeleton::eEffector)
                break;
            if (skel->GetSkeletonType() == FbxSkeleton::eLimb)
                SPRUE_LOG_WARNING("Found an eLimb");
            else if (skel->GetSkeletonType() == FbxSkeleton::eLimbNode)
                SPRUE_LOG_WARNING("Found an eLimbNode");
            else if (skel->GetSkeletonType() == FbxSkeleton::eRoot)
                SPRUE_LOG_WARNING("Found an eRoot");

            Joint* joint = new Joint();
            joint->SetUserData(node);
            joint->SetName(node->GetName());

            if (currentJoint)
                currentJoint->AddChild(joint);
            else
                skeleton->AddJoint(joint);

            //auto matrix = node->EvaluateGlobalTransform();
            auto matrix = FBX_GetGlobalPosition(node, 0, pose);
            //auto matrix = poseMat;
            joint->SetModelSpaceTransform(ConvertFbxMatrix(matrix).Float3x4Part());

            currentJoint = joint;

        } while (false);
    }

    for (int i = 0; i < node->GetChildCount(); ++i)
        BuildSkeletonRecurse(skeleton, node->GetChild(i), currentJoint);
}

MeshData* FBXLoader::ToMeshData(const char* name, std::vector<Vertex>& vertices, std::vector<unsigned>& indices) const
{
    if (vertices.size() > 0)
    {
        MeshData* ret = new MeshData();
        ret->SetName(name);
        ret->indexBuffer_ = indices;
        ret->positionBuffer_.reserve(vertices.size());
        ret->normalBuffer_.reserve(vertices.size());
        ret->uvBuffer_.reserve(vertices.size());
        for (unsigned i = 0; i < vertices.size(); ++i)
        {
            auto vertData = vertices[i];
            ret->positionBuffer_.push_back(vertData.position_);
            ret->normalBuffer_.push_back(vertData.normal_);
            ret->uvBuffer_.push_back(vertData.uv_);
        }
        return ret;
    }
    return 0x0;
}

void FBXWriteNormals(FbxGeometryElementNormal* layer, const Vec3* data, unsigned ct)
{
    //layer->SetReferenceMode(FbxGeometryElement::eDirect);
    for (unsigned elemIdx = 0; elemIdx < ct; ++elemIdx)
        layer->GetDirectArray().Add(FbxVector4(data[elemIdx].x, data[elemIdx].y, data[elemIdx].z));
}

void FbxWriteTangents(FbxGeometryElementTangent* layer, const Vec4* data, unsigned ct)
{
    for (unsigned elemIdx = 0; elemIdx < ct; ++elemIdx)
    {
        layer->GetDirectArray().Add(FbxVector4(data[elemIdx].x, data[elemIdx].y, data[elemIdx].z, data[elemIdx].w));
    }
}

void FBXWriteColor(FbxGeometryElementVertexColor* layer, const RGBA* data, unsigned ct)
{
    //layer->SetReferenceMode(FbxGeometryElement::eDirect);
    for (unsigned elemIdx = 0; elemIdx < ct; ++elemIdx)
        layer->GetDirectArray().Add(FbxColor(data[elemIdx].r, data[elemIdx].g, data[elemIdx].b, data[elemIdx].a));
}

void FBXWriteUV(FbxGeometryElementUV* layer, const Vec2* data, unsigned ct)
{
    //layer->SetReferenceMode(FbxGeometryElement::eDirect);
    for (unsigned elemIdx = 0; elemIdx < ct; ++elemIdx)
        layer->GetDirectArray().Add(FbxVector2(data[elemIdx].x, data[elemIdx].y));
}

FbxCluster* GetOrCreateCluster(std::vector<FbxCluster*>& clusters, int idx, FbxSkin* skin, FbxNode* bone)
{
    if (idx == -1)
        return 0x0;
    if (clusters[idx] != 0x0)
        return clusters[idx];

    SPRUE_LOG_INFO("FBX: creating a cluster");
    FbxCluster* cluster = FbxCluster::Create(skin->GetFbxManager(), bone->GetName());
    cluster->SetLink(bone);
    cluster->SetLinkMode(FbxCluster::ELinkMode::eTotalOne);
    FbxAMatrix mat; mat.SetIdentity();
    cluster->SetTransformMatrix(mat);
    cluster->SetTransformLinkMatrix(bone->EvaluateGlobalTransform());
    skin->AddCluster(cluster);
    clusters[idx] = cluster;
    return cluster;
}

void AddMeshToScene(FbxScene* scene, const MeshData* meshData, std::vector<FbxNode*>& inlineBones)
{
    auto he = meshData->BuildHalfEdgeMesh();
    FbxMesh* mesh = FbxMesh::Create(scene, meshData->GetName().c_str());
    FbxArray<FbxVector4> controlPoints;
    
    mesh->SetControlPointCount(meshData->positionBuffer_.size());
    for (unsigned i = 0; i < meshData->positionBuffer_.size(); ++i)
    {
        auto& buffer = meshData->positionBuffer_;
        mesh->SetControlPointAt(FbxVector4(buffer[i].x, buffer[i].y, buffer[i].z, 0.0f), i);
    }

    // Write indices
    for (unsigned i = 0; i < meshData->indexBuffer_.size(); i += 3)
    {
        unsigned indices[3] = {
            meshData->indexBuffer_[i],
            meshData->indexBuffer_[i + 1],
            meshData->indexBuffer_[i + 2]
        };

        mesh->BeginPolygon(-1, -1, false);
        for (unsigned v = 0; v < 3; ++v)
            mesh->AddPolygon(indices[v]);
        mesh->EndPolygon();
    }

    if (meshData->GetNormalBuffer().size())
    {
        FbxGeometryElementNormal* normals = mesh->CreateElementNormal();
        normals->SetMappingMode(FbxGeometryElement::eByControlPoint);
        normals->SetReferenceMode(fbxsdk::FbxLayerElement::EReferenceMode::eDirect);
        FBXWriteNormals(normals, meshData->normalBuffer_.data(), meshData->normalBuffer_.size());
    }

    if (meshData->GetUVBuffer().size())
    {
        FbxGeometryElementUV* uv = mesh->CreateElementUV("uv");
        uv->SetMappingMode(FbxGeometryElement::eByControlPoint);
        uv->SetReferenceMode(fbxsdk::FbxLayerElement::EReferenceMode::eDirect);
        FBXWriteUV(uv, meshData->uvBuffer_.data(), meshData->uvBuffer_.size());
    }

    if (meshData->GetColorBuffer().size())
    {
        FbxGeometryElementVertexColor* colors = mesh->CreateElementVertexColor();
        colors->SetMappingMode(FbxGeometryElement::eByControlPoint);
        colors->SetReferenceMode(fbxsdk::FbxLayerElement::EReferenceMode::eDirect);
        FBXWriteColor(colors, meshData->colorBuffer_.data(), meshData->colorBuffer_.size());
    }

    //TODO: tangents? binormals?
    if (meshData->tangentBuffer_.size())
    {
        FbxGeometryElementTangent* tangents = mesh->CreateElementTangent();
        tangents->SetMappingMode(FbxGeometryElement::eByControlPoint);
        FbxWriteTangents(tangents, meshData->tangentBuffer_.data(), meshData->tangentBuffer_.size());
    }

    //TODO: write skeleton

    //TODO: bone weights
    if (meshData->boneIndices_.size())
    {
        SPRUE_LOG_INFO("FBX: writing bone weights");
        bool anyWeights = false;
        for (int i = 0; i < meshData->boneIndices_.size(); ++i)
        {
            if (meshData->boneIndices_[i].x_ != -1 ||
                meshData->boneIndices_[i].y_ != -1 ||
                meshData->boneIndices_[i].z_ != -1 ||
                meshData->boneIndices_[i].w_ != -1)
            {
                anyWeights = true;
                break;
            }
        }

        if (anyWeights)
        {
            std::vector<FbxCluster*> clusters(inlineBones.size(), 0x0);

            FbxSkin* skin = FbxSkin::Create(mesh, meshData->GetName().c_str());
            mesh->AddDeformer(skin);

            for (int i = 0; i < meshData->boneIndices_.size(); ++i)
            {
                for (int weightIdx = 0; weightIdx < 4; ++weightIdx)
                {
                    int boneIdx = meshData->boneIndices_[i][weightIdx];
                    if (boneIdx == -1)
                        continue;
                    if (FbxCluster* clust = GetOrCreateCluster(clusters, boneIdx, skin, inlineBones[boneIdx]))
                        clust->AddControlPointIndex(i, meshData->boneWeights_[i][weightIdx]);
                }
            }
        }
    }

    //TODO: morph targets

    FbxNode* lNode = FbxNode::Create(scene, meshData->GetName().c_str());
    lNode->SetNodeAttribute(mesh);
    lNode->SetShadingMode(FbxNode::eTextureShading);
}

void WriteBone(Joint* joint, FbxScene* scene, FbxPose* pose, FbxObject* parent, std::vector<FbxNode*>& inlineBones)
{
    FbxNode* node = FbxNode::Create(parent, joint->GetName().c_str());
    inlineBones[joint->GetIndex()] = node;
    //inlineBones.push_back(node);
    if (joint->GetParent())
    {
        auto localPos = joint->GetParent()->GetTransform().Inverted() * joint->GetPosition();
        node->LclTranslation.Set(FbxDouble3(localPos.x, localPos.y, localPos.z));
    }
    else
        node->LclTranslation.Set(FbxDouble3(joint->GetPosition().x, joint->GetPosition().y, joint->GetPosition().z));

    FbxSkeleton* skel = FbxSkeleton::Create(scene, joint->GetName().c_str());
    skel->SetSkeletonType(joint->GetParent() ? fbxsdk::FbxSkeleton::eLimbNode : fbxsdk::FbxSkeleton::eRoot);
    pose->Add(node, ToFbxMatrix(joint->GetTransform()));
    node->SetNodeAttribute(skel);

    if (joint->GetParent())
    {
        float length = (joint->GetParent()->GetPosition() - joint->GetPosition()).Length();
        skel->LimbLength.Set(length);
    }

    for (int i = 0; i < joint->GetChildren().size(); ++i)
        WriteBone(joint->GetChildren()[i], scene, pose, node, inlineBones);
}

void WriteSkeleton(Skeleton* skeleton, FbxScene* scene, std::vector<FbxNode*>& inlineBones)
{
    FbxPose* pose = FbxPose::Create(scene->GetFbxManager(), "Bind");
    pose->SetIsBindPose(true);

    inlineBones.resize(skeleton->GetAllJoints().size());
    WriteBone(skeleton->GetRootJoint(), scene, pose, scene, inlineBones);
    scene->AddPose(pose);
}

void FBXLoader::SaveModel(const MeshResource* meshData, const char* fileName, bool legacy)
{
    FbxManager* lSdkManager = FbxManager::Create();
    FbxIOSettings * ios = FbxIOSettings::Create(lSdkManager, IOSROOT);
    lSdkManager->SetIOSettings(ios);

    std::string fn(fileName);
    int format = -1;
    if (fn.find(".dae") != std::string::npos)
        format = lSdkManager->GetIOPluginRegistry()->FindWriterIDByDescription("Collada DAE (*.dae)");

    FbxExporter* lExporter = FbxExporter::Create(lSdkManager, "");
    
    if (legacy)
        lExporter->SetFileExportVersion(FBX_2012_00_COMPATIBLE);
    bool lExportStatus = lExporter->Initialize(fileName, format, lSdkManager->GetIOSettings());
    if (!lExportStatus)
    {
        SPRUE_LOG_ERROR("Failed to prepare export of FBX file");
        lExporter->Destroy();
        ios->Destroy();
        lSdkManager->Destroy();
        return;
    }

    FbxScene* lScene = FbxScene::Create(lSdkManager, fileName);
    lScene->GetGlobalSettings().SetAxisSystem(fbxsdk::FbxAxisSystem::eOpenGL);

    std::vector<FbxNode*> inlineBones;
    if (meshData->GetSkeleton() != 0x0)
        WriteSkeleton(meshData->GetSkeleton(), lScene, inlineBones);

    //??lScene->GetGlobalSettings().SetSystemUnit(fbxsdk::FbxSystemUnit::dm);
    auto& meshList = meshData->GetMeshes();
    for (unsigned i = 0; i < meshList.size(); ++i)
    {
        if (meshList[i]->GetPositionBuffer().empty())
            continue;
        AddMeshToScene(lScene, meshList[i], inlineBones);
    }

    if (!lExporter->Export(lScene))
    {
        SPRUE_LOG_ERROR("Failed to export FBX file");
    }

    lScene->Destroy();
    lExporter->Destroy();
    ios->Destroy();
    lSdkManager->Destroy();
}

}