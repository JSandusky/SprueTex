#include "Stdafx.h"

using namespace System::Runtime::InteropServices;
using namespace Microsoft::Xna::Framework;

#define STRING_TO_CHAR(SRC) (const char*)(Marshal::StringToHGlobalAnsi(SRC)).ToPointer();
#define CHAR_TO_STRING(SRC) gcnew System::String(SRC)


#include "Geometry.h"

#include <SprueEngine/Resource.h>
#include <SprueEngine/Geometry/MeshData.h>
#include <SprueEngine/Geometry/Skeleton.h>
#include <SprueEngine/Geometry/LaplacianOperations.h>

#include <SprueEngine/Resource.h>
#include <SprueEngine/Loaders/FBXLoader.h>
#include <SprueEngine/Loaders/OBJLoader.h>

namespace SprueBindings
{
    extern void FillSkeleton(SprueEngine::Skeleton* target, PluginLib::SkeletonData^ source);

    inline Vector3 FromVec3(SprueEngine::Vec3 v)
    {
        return Vector3(v.x, v.y, v.z);
    }

    inline Quaternion FromQuat(SprueEngine::Quat q)
    {
        return Quaternion(q.x, q.y, q.z, q.w);
    }

    void ProcessJointTree(PluginLib::SkeletonData^ holder, PluginLib::JointData^ parent, SprueEngine::Joint* joint)
    {
        PluginLib::JointData^ thisJoint = gcnew PluginLib::JointData();
        thisJoint->Name = CHAR_TO_STRING(joint->GetName().c_str());
        thisJoint->Position = FromVec3(joint->GetModelSpacePosition());
        thisJoint->Rotation = FromQuat(joint->GetModelSpaceRotation());
        thisJoint->Scale = FromVec3(joint->GetModelSpaceScale());
        thisJoint->Flags = joint->GetFlags();
        thisJoint->Capabilities = joint->GetCapabilities();

        holder->AddJoint(parent, thisJoint);
        for (int i = 0; i < joint->GetChildren().size(); ++i)
            ProcessJointTree(holder, thisJoint, joint->GetChildren()[i]);
    }

    PluginLib::SkeletonData^ ConvertSkeleton(SprueEngine::Skeleton* skeleton)
    {
        if (skeleton == 0x0)
            return nullptr;

        PluginLib::SkeletonData^ ret = gcnew PluginLib::SkeletonData();
        ProcessJointTree(ret, nullptr, skeleton->GetRootJoint());
        return ret;
    }

    MeshData::MeshData()
    {
        meshData_ = 0x0;
    }

    MeshData::MeshData(SprueEngine::MeshData* data)
    {
        meshData_ = data;
    }

    MeshData::~MeshData()
    {
        if (meshData_ != 0x0)
            delete meshData_;
    }

    SprueEngine::Vec3 Convert(Vector3 vec)
    {
        return SprueEngine::Vec3(vec.X, vec.Y, vec.Z);
    }

    SprueEngine::Vec4 Convert(Vector4 vec)
    {
        return SprueEngine::Vec4(vec.X, vec.Y, vec.Z, vec.W);
    }

#define ARRAY_TO_VECTOR(VECTOR, ARRAY, VECTYPE, ARRTYPE) if (ARRAY != nullptr && ARRAY->Length > 0) { \
    VECTOR.resize(ARRAY->Length); \
    { pin_ptr<ARRTYPE> data = &ARRAY[0]; memcpy(VECTOR.data(), data, sizeof(VECTYPE) * ARRAY->Length); } } \
    else VECTOR.clear()

    void MeshData::WriteToAPI()
    {
        if (meshData_ == 0x0)
            meshData_ = new SprueEngine::MeshData();

        ARRAY_TO_VECTOR(meshData_->positionBuffer_, Positions, SprueEngine::Vec3, Vector3);
        ARRAY_TO_VECTOR(meshData_->normalBuffer_, Normals, SprueEngine::Vec3, Vector3);
        ARRAY_TO_VECTOR(meshData_->tangentBuffer_, Tangents, SprueEngine::Vec4, Vector4);
        ARRAY_TO_VECTOR(meshData_->uvBuffer_, UVCoordinates, SprueEngine::Vec2, Vector2);
        ARRAY_TO_VECTOR(meshData_->boneWeights_, BoneWeights, SprueEngine::Vec4, Vector4);
        ARRAY_TO_VECTOR(meshData_->boneIndices_, BoneIndices, SprueEngine::IntVec4, PluginLib::IntVector4);
        ARRAY_TO_VECTOR(meshData_->indexBuffer_, Indices, int, int);
    }


#define VECTOR_TO_ARRAY(VECTOR, ARRAY, VECTYPE, ARRTYPE) if (!VECTOR.empty()) { \
    if (ARRAY != nullptr) ARRAY->Resize<ARRTYPE>(ARRAY, VECTOR.size()); \
    else ARRAY = gcnew array<ARRTYPE>(VECTOR.size()); \
    { pin_ptr<ARRTYPE> data = &ARRAY[0]; memcpy(data, VECTOR.data(), sizeof(VECTYPE) * VECTOR.size()); } } \
    else ARRAY = nullptr


    void MeshData::ReadFromAPI()
    {
        if (meshData_)
        {
            VECTOR_TO_ARRAY(meshData_->positionBuffer_, Positions, SprueEngine::Vec3, Vector3);
            VECTOR_TO_ARRAY(meshData_->normalBuffer_, Normals, SprueEngine::Vec3, Vector3);
            VECTOR_TO_ARRAY(meshData_->tangentBuffer_, Tangents, SprueEngine::Vec4, Vector4);
            VECTOR_TO_ARRAY(meshData_->uvBuffer_, UVCoordinates, SprueEngine::Vec2, Vector2);
            VECTOR_TO_ARRAY(meshData_->boneWeights_, BoneWeights, SprueEngine::Vec4, Vector4);
            VECTOR_TO_ARRAY(meshData_->boneIndices_, BoneIndices, SprueEngine::IntVec4, PluginLib::IntVector4);
            VECTOR_TO_ARRAY(meshData_->indexBuffer_, Indices, int, int);

            if (meshData_->GetMorphTargets().size() > 0)
            {
                MorphTargets = gcnew array<MeshMorphTarget^>(meshData_->GetMorphTargets().size());
                for (int tgtIndex = 0; tgtIndex < meshData_->GetMorphTargets().size(); ++tgtIndex)
                {
                    auto& target = meshData_->GetMorphTargets()[tgtIndex];
                    MeshMorphTarget^ tgt = gcnew MeshMorphTarget();
                    MorphTargets[tgtIndex] = tgt;
                    tgt->Positions = gcnew array<Vector3>(target.data.size());
                    tgt->Normals = gcnew array<Vector3>(target.data.size());
                    tgt->Name = gcnew System::String(target.name.c_str());

                    for (int i = 0; i < target.data.size(); ++i)
                    {
                        tgt->Positions[i] = FromVec3(target.data[i].Position);
                        tgt->Normals[i] = FromVec3(target.data[i].Normal);
                    }
                }
            }
        }
        else
        {
            Positions = nullptr;
            Normals = nullptr;
            Tangents = nullptr;
            UVCoordinates = nullptr;
            BoneWeights = nullptr;
            BoneIndices = nullptr;
            Indices = nullptr;
            MorphTargets = nullptr;
        }
    }

    bool MeshData::CalculateNormals()
    {
        if (meshData_ != 0x0)
        {
            meshData_->CalculateNormals();
            return true;
        }
        return false;
    }
    bool MeshData::CalculateTangents()
    {
        if (meshData_ != 0x0)
        {
            meshData_->CalculateTangents();
            return true;
        }
        return false;
    }
    bool MeshData::NormalizeBoneWeights()
    {
        if (meshData_ != 0x0)
        {
            meshData_->NormalizeBoneWeights();
            return true;
        }
        return false;
    }
    bool MeshData::Subdivide(bool smooth)
    {
        if (meshData_ != 0x0)
        {
            if (auto subdividedMesh = meshData_->Subdivide(smooth))
            {
                meshData_->positionBuffer_ = subdividedMesh->positionBuffer_;
                meshData_->normalBuffer_ = subdividedMesh->normalBuffer_;
                meshData_->tangentBuffer_ = subdividedMesh->tangentBuffer_;
                meshData_->uvBuffer_ = subdividedMesh->uvBuffer_;
                meshData_->indexBuffer_ = subdividedMesh->indexBuffer_;

                meshData_->indexBuffer_ = subdividedMesh->indexBuffer_;
                meshData_->boneWeights_ = subdividedMesh->boneWeights_;
                meshData_->boneIndices_ = subdividedMesh->boneIndices_;
                return true;
            }
        }
        return false;
    }
    bool MeshData::Decimate(float intensity)
    {
        if (meshData_ != 0x0)
        {
            if (auto decimatedMesh = meshData_->Decimate(intensity))
            {
                meshData_->indexBuffer_ = decimatedMesh->indexBuffer_;
                meshData_->positionBuffer_ = decimatedMesh->positionBuffer_;
                meshData_->normalBuffer_ = decimatedMesh->normalBuffer_;
                meshData_->tangentBuffer_ = decimatedMesh->tangentBuffer_;
                meshData_->uvBuffer_ = decimatedMesh->uvBuffer_;

                meshData_->indexBuffer_ = decimatedMesh->indexBuffer_;
                meshData_->boneWeights_ = decimatedMesh->boneWeights_;
                meshData_->boneIndices_ = decimatedMesh->boneIndices_;
                return true;
            }
        }
        return false;
    }
    bool MeshData::Smooth(float power)
    {
        if (meshData_ != 0x0)
        {
            meshData_->Smooth(power);
            return true;
        }
        return false;
    }
    bool MeshData::TransformUV(Vector2 offset, Vector2 scale)
    {
        if (meshData_ != 0x0)
        {
            meshData_->TransformUV(SprueEngine::Vec2(offset.X, offset.Y), SprueEngine::Vec2(scale.X, scale.Y));
            return true;
        }
        return false;
    }

    bool MeshData::ComputeNormalUVs()
    {
        if (meshData_ != nullptr)
        {
            SprueEngine::Vec2 minPt(floatMax, floatMax);
            SprueEngine::Vec2 maxPt(floatMin, floatMin);

            for (int i = 0; i < meshData_->positionBuffer_.size(); ++i)
            {
                auto vert = meshData_->positionBuffer_[i];
                minPt.x = SprueMin(minPt.x, vert.x);
                minPt.y = SprueMin(minPt.y, vert.z);
                maxPt.x = SprueMax(maxPt.x, vert.x);
                maxPt.y = SprueMax(maxPt.y, vert.z);
            }

            meshData_->uvBuffer_.resize(meshData_->positionBuffer_.size());
            for (int i = 0; i < meshData_->uvBuffer_.size(); ++i)
            {
                auto vert = meshData_->positionBuffer_[i];
                meshData_->uvBuffer_[i] = SprueEngine::Vec2(NORMALIZE(vert.x, minPt.x, maxPt.x), NORMALIZE(vert.z, minPt.y, maxPt.y));
            }
        }
        return false;
    }

    bool MeshData::ComputeUVCoordinates(int width, int height, int quality, float stretch, float gutter, UVCallback^ callback, PluginLib::IErrorPublisher^ errors)
    {
        if (meshData_ != 0x0)
        {
            int triCount = meshData_->indexBuffer_.size() / 3;
            SprueEngine::UV_CALLBACK callbackFunc = 0x0;
            if (callback != nullptr)
            {
                auto ptr = Marshal::GetFunctionPointerForDelegate(callback);
                callbackFunc = static_cast<SprueEngine::UV_CALLBACK>(ptr.ToPointer());
                meshData_->ComputeUVCoordinates(width, height, quality, 24, stretch, gutter, callbackFunc);
                if (meshData_->uvBuffer_.empty())
                    meshData_->ComputeUVCoordinates(width, height, quality, 50, stretch, gutter, callbackFunc);
            }
            else
                meshData_->ComputeUVCoordinates(width, height, quality, 24, stretch, gutter);
            //if (meshData_->uvBuffer_.empty())
            //    meshData_->ComputeUVCoordinates(width*4, height*4, quality, 0, stretch, gutter);

            int newTriCount = meshData_->indexBuffer_.size() / 3;
            if (newTriCount != triCount)
            {
                errors->Error(System::String::Format("Bad UV output, started with {0} tris, got back {1} tris", triCount, newTriCount));
            }
            return !meshData_->uvBuffer_.empty();
        }
        return false;
    }

    ModelData^ ModelData::LoadFBX(System::String^ path, PluginLib::IErrorPublisher^ errorOutput)
    {
        if (path != nullptr && System::IO::File::Exists(path))
        {
            auto fbxLoader = new SprueEngine::FBXLoader();
            const char* cPath = STRING_TO_CHAR(path);
            if (auto loaded = fbxLoader->LoadResource(cPath))
            {
                if (auto modelData = dynamic_cast<SprueEngine::MeshResource*>(loaded.get()))
                {
                    ModelData^ ret = gcnew ModelData();
                    ret->Meshes = gcnew List<MeshData^>();
                    auto skeleton = ConvertSkeleton(modelData->GetSkeleton());
                    for (int i = 0; i < modelData->GetMeshCount(); ++i)
                    {
                        ret->Meshes->Add(gcnew MeshData(modelData->GetMesh(i)->CloneRaw()));
                        if (!modelData->GetMesh(i)->GetName().empty())
                            ret->Meshes[i]->Name = gcnew String(modelData->GetMesh(i)->GetName().c_str());
                        ret->Meshes[i]->Skeleton = skeleton;
                    }

                    if (modelData->GetMeshCount() == 0)
                        errorOutput->PublishError(System::String::Format("No mesh data contained in FBX model: {0}", System::IO::Path::GetFileName(path)), PluginLib::ErrorLevels::ERROR);

                    return ret;
                }
                else
                    errorOutput->PublishError(System::String::Format("No data was loaded for FBX model: {0}", System::IO::Path::GetFileName(path)), PluginLib::ErrorLevels::ERROR);
            }
            else
                errorOutput->PublishError(System::String::Format("Unable to load FBX model: {0}", System::IO::Path::GetFileName(path)), PluginLib::ErrorLevels::ERROR);
        }
        else
            errorOutput->PublishError(System::String::Format("File {0} does not exist to load as an FBX model", System::IO::Path::GetFileName(path)), PluginLib::ErrorLevels::ERROR);
        return nullptr;
    }

    ModelData::~ModelData()
    {
        
    }

    ModelData^ ModelData::LoadModel(System::String^ path, PluginLib::IErrorPublisher^ errorOutput)
    {
        if (path->ToLowerInvariant()->EndsWith(".obj"))
            return LoadOBJ(path, errorOutput);
        else if (path->ToLowerInvariant()->EndsWith(".fbx"))
            return LoadFBX(path, errorOutput);
        else if (path->ToLowerInvariant()->EndsWith(".dae"))
            return LoadFBX(path, errorOutput);
        return nullptr;
    }

    ModelData^ ModelData::LoadOBJ(System::String^ path, PluginLib::IErrorPublisher^ errorOutput)
    {
        if (path != nullptr && System::IO::File::Exists(path))
        {
            auto objLoader = new SprueEngine::OBJLoader();
            const char* cPath = STRING_TO_CHAR(path);
            if (auto loaded = objLoader->LoadResource(cPath))
            {
                if (auto modelData = dynamic_cast<SprueEngine::MeshResource*>(loaded.get()))
                {
                    ModelData^ ret = gcnew ModelData();
                    ret->Meshes = gcnew List<MeshData^>();
                    for (int i = 0; i < modelData->GetMeshCount(); ++i)
                    {
                        ret->Meshes->Add(gcnew MeshData(modelData->GetMesh(i)->CloneRaw()));
                        if (!modelData->GetMesh(i)->GetName().empty())
                            ret->Meshes[i]->Name = gcnew String(modelData->GetMesh(i)->GetName().c_str());
                    }

                    if (modelData->GetMeshCount() == 0)
                        errorOutput->PublishError(System::String::Format("No mesh data contained in OBJ model: {0}", System::IO::Path::GetFileName(path)), PluginLib::ErrorLevels::ERROR);

                    return ret;
                }
                else
                    errorOutput->PublishError(System::String::Format("No data was loaded for OBJ model: {0}", System::IO::Path::GetFileName(path)), PluginLib::ErrorLevels::ERROR);
            }
            else
                errorOutput->PublishError(System::String::Format("Unable to load OBJ model: {0}", System::IO::Path::GetFileName(path)), PluginLib::ErrorLevels::ERROR);
        }
        else
            errorOutput->PublishError(System::String::Format("File {0} does not exist to load as an OBJ model", System::IO::Path::GetFileName(path)), PluginLib::ErrorLevels::ERROR);
        return nullptr;
    }

    bool ModelData::SaveFBX(ModelData^ model, System::String^ path, bool newest, PluginLib::IErrorPublisher^ errorOutput)
    {
        if (model->Meshes->Count == 0)
            return false;

        SprueEngine::FBXLoader loader;
        SprueEngine::MeshResource meshResource;
        meshResource.SetOwnsData(false);
        for (int i = 0; i < model->Meshes->Count; ++i)
            meshResource.GetMeshes().push_back(model->Meshes[i]->GetInternalMeshData());

        SprueEngine::Skeleton* skeleton = 0x0;
        if (model->Meshes[0]->Skeleton != nullptr)
        {
            skeleton = new SprueEngine::Skeleton();
            FillSkeleton(skeleton, model->Meshes[0]->Skeleton);
            meshResource.SetSkeleton(skeleton);
        }
        
        auto cString = STRING_TO_CHAR(path);
        loader.SaveModel(&meshResource, cString, !newest);

        if (skeleton != 0x0)
            delete skeleton;
        return true;
    }

    bool ModelData::SaveDAE(ModelData^ model, System::String^ path, PluginLib::IErrorPublisher^ errorOutput)
    {
        SprueEngine::FBXLoader loader;
        SprueEngine::MeshResource meshResource;
        meshResource.SetOwnsData(false);
        for (int i = 0; i < model->Meshes->Count; ++i)
        {
            meshResource.GetMeshes().push_back(model->Meshes[i]->GetInternalMeshData());
        }

        SprueEngine::Skeleton* skeleton = 0x0;
        if (model->Meshes[0]->Skeleton != nullptr)
        {
            skeleton = new SprueEngine::Skeleton();
            FillSkeleton(skeleton, model->Meshes[0]->Skeleton);
            meshResource.SetSkeleton(skeleton);
        }

        auto cString = STRING_TO_CHAR(path);
        loader.SaveModel(&meshResource, cString, false);
        return true;
    }

    bool ModelData::SaveOBJ(ModelData^ model, System::String^ path, PluginLib::IErrorPublisher^ errorOutput)
    {
        SprueEngine::OBJLoader loader;
        std::vector<SprueEngine::MeshData*> meshes;
        for (int i = 0; i < model->Meshes->Count; ++i)
            meshes.push_back(model->Meshes[i]->GetInternalMeshData());
        auto cString = STRING_TO_CHAR(path);
        loader.SaveModel(meshes, cString);
        return true;
    }

    bool ModelData::SaveGEX(ModelData^ model, System::String^ path, PluginLib::IErrorPublisher^ errorOutput)
    {
        return false;
    }
}
