#include <SprueEngine/Core/SprueModel.h>

#include <SprueEngine/Core/Context.h>
#include <SprueEngine/Meshing/CSG.h>
#include <SprueEngine/Deserializer.h>
#include <SprueEngine/Core/FinPiece.h>
#include <SprueEngine/Core/ModelPiece.h>
#include <SprueEngine/Meshing/Octree.h>
#include <SprueEngine/Serializer.h>
#include <SprueEngine/Core/SpaceDeformer.h>
#include <SprueEngine/Core/SpruePieces.h>
#include <SprueEngine/Meshing/MeshMerge.h>
#include <SprueEngine/Geometry/Skeleton.h>

#include <algorithm>
#include <ios>
#include <iostream>
#include <fstream>
#include <exception>

namespace SprueEngine
{

class SprueModel::CSGMeshVisitor : public TypeLimitedSceneObjectVisitor<ModelPiece>
{
public:
    CSGMeshVisitor(SprueModel* owner, bool withUV) :
        owner_(owner),
        useUVs_(withUV),
        anyMeshed_(false)
    {
        //PrepareCSGModelData(owner_->meshedModelParts_[0].get(), srcModel, true);
    }

    ~CSGMeshVisitor()
    {
        //if (anyMeshed_)
        //    TransferCSGModelData(owner_->meshedModelParts_[0].get(), srcModel, useUVs_);
    }

    virtual bool Visit(ModelPiece* child) override
    {
        if (!child->IsDisabled() && child->UseUVs() == useUVs_)
        {
            if (!child->GetMeshData())
                return false;
            if (!owner_->GetMeshedParts())
                return false;

            MeshMerger merger;
            //csgjs_model otherModel;
            //PrepareCSGModelData(child->GetMeshData()->GetMesh(0), otherModel, useUVs_);
            switch (child->GetMergeMode())
            {
            case MPMM_Merge:
                merger.CombineMeshes(owner_->GetMeshedParts()->GetMesh(0), owner_->GetMeshedParts()->GetMesh(0), child->GetMeshData()->GetMesh(0), child->GetWorldTransform());
                anyMeshed_ = true;
                break;
            case MPMM_Independent:
                owner_->GetMeshedParts()->GetMeshes().insert(owner_->GetMeshedParts()->GetMeshes().end(), child->GetMeshData()->GetMeshes().begin(), child->GetMeshData()->GetMeshes().end());
                anyMeshed_ = true;
                break;
            case MPMM_CSGAdd:
                merger.CSGUnion(owner_->GetMeshedParts()->GetMesh(0), owner_->GetMeshedParts()->GetMesh(0), child->GetMeshData()->GetMesh(0), child->GetWorldTransform(), true);
                anyMeshed_ = true;
                break;
            case MPMM_CSGSubstract:
                merger.CSGSubtract(owner_->GetMeshedParts()->GetMesh(0), owner_->GetMeshedParts()->GetMesh(0), child->GetMeshData()->GetMesh(0), child->GetWorldTransform(), true);
                anyMeshed_ = true;
                break;
            case MPMM_CSGIntersect:
                merger.CSGIntersect(owner_->GetMeshedParts()->GetMesh(0), owner_->GetMeshedParts()->GetMesh(0), child->GetMeshData()->GetMesh(0), child->GetWorldTransform(), true);
                anyMeshed_ = true;
                break;
            case MPMM_ClipIndependently: {
                MeshData* newMesh = new MeshData();
                merger.ClipTo(newMesh, owner_->GetMeshedParts()->GetMesh(0), child->GetMeshData()->GetMesh(0), child->GetWorldTransform(), false);
                owner_->GetMeshedParts()->GetMeshes().push_back(newMesh);
                anyMeshed_ = true;
                } break;
            case MPMM_ClipThenMerge: {
                MeshData* newMesh = new MeshData();
                merger.ClipTo(newMesh, owner_->GetMeshedParts()->GetMesh(0), child->GetMeshData()->GetMesh(0), child->GetWorldTransform(), false);
                merger.CombineMeshes(owner_->GetMeshedParts()->GetMesh(0), owner_->GetMeshedParts()->GetMesh(0), newMesh, child->GetWorldTransform());
                anyMeshed_ = true;
                } break;
            }
        }
        return true;
    }

    csgjs_model srcModel;
    SprueModel* owner_;
    bool useUVs_;
    bool anyMeshed_;
};

class SprueModel::FinVisitor : public TypeLimitedSceneObjectVisitor<FinPiece>
{
public:
    FinVisitor(SprueModel* owner, bool withUV) :
        owner_(owner),
        useUVs_(withUV),
        anyMeshed_(false)
    {
        //PrepareCSGModelData(owner_->meshedModelParts_[0].get(), srcModel, true);
    }

    ~FinVisitor()
    {
        //if (anyMeshed_)
        //    TransferCSGModelData(owner_->meshedModelParts_[0].get(), srcModel, useUVs_);
    }

    virtual bool Visit(FinPiece* child) override
    {
        if (child->IsClippedToMesh())
        {
            MeshMerger merger;
            auto childMesh = child->CreateMesh();
            MeshData* newMesh = new MeshData();
            merger.ClipTo(newMesh, owner_->GetMeshedParts()->GetMesh(0), childMesh, child->GetWorldTransform(), false);
            delete childMesh;
            newMesh->SetTwoSided(child->IsTwoSided());
            owner_->GetMeshedParts()->GetMeshes().push_back(newMesh);
        }
        else
        {
            auto mesh = child->CreateMesh();
            mesh->SetTwoSided(child->IsTwoSided());
            owner_->GetMeshedParts()->GetMeshes().push_back(mesh);
        }
        return anyMeshed_;
    }

    csgjs_model srcModel;
    SprueModel* owner_;
    bool useUVs_;
    bool anyMeshed_;
};

void SprueModel::Register(Context* context)
{
    context->RegisterFactory<SprueModel>("SprueModel", "The root of a meshable object");
    context->CopyBaseProperties("SceneObject", "SprueModel");
    REGISTER_PROPERTY(SprueModel, unsigned, GetGenerationDepth, SetGenerationDepth, 8, "Quality", "", PS_VisualConsequence);
    REGISTER_PROPERTY_MEMORY(SprueModel, float, offsetof(SprueModel, error_), 0.25f, "Error Tol.", "Error tolerance in QEF simplification step", PS_VisualConsequence);
}

SprueModel::SprueModel() :
    generatorDepth_(8),
    meshedModelParts_(new MeshResource())
{
}

SprueModel::~SprueModel()
{
}

bool SprueModel::LoadModel(Deserializer* src, const SerializationContext& context)
{
    bool success = false;
    try {
        
        std::string fileID = src->ReadFileID();
        if (fileID.compare("SKMD") != 0)
            return false;
        
        success = base::Deserialize(src, context);
    }
    catch (...)
    {
        return false;
    }
    return success;
}

bool SprueModel::SaveModel(Serializer* dest, const SerializationContext& context)
{
    bool success = dest->WriteFileID("SKMD");
    
    success &= base::Serialize(dest, context);

    return success;
}

#ifndef SPRUE_NO_XML
bool SprueModel::LoadModel(tinyxml2::XMLDocument* doc, const SerializationContext& context)
{
    if (tinyxml2::XMLElement* root = doc->FirstChildElement("SprueModel"))
        if (tinyxml2::XMLElement* myElement = root->FirstChildElement())
            return Deserialize(myElement, context);
    return false;
}

bool SprueModel::LoadModel(tinyxml2::XMLElement* fromElement, const SerializationContext& context)
{
    return Deserialize(fromElement, context);
}

bool SprueModel::SaveModel(tinyxml2::XMLDocument* intoDoc, const SerializationContext& context)
{
    tinyxml2::XMLElement* root = intoDoc->NewElement("SprueModel");
    intoDoc->InsertFirstChild(root);
    return Serialize(root, context);
}
#endif

void SprueModel::ProcessCSG()
{
    std::vector<ModelPiece*> modelPieces = this->GetChildrenOfType<ModelPiece>();

    // Scope for destructor
    {
        // Use CSG to combine meshes that aren't SDF meshed and don't retain UVs
        CSGMeshVisitor csgVisitor(this, false /*use UV coordinates*/);
        VisitParentFirst(&csgVisitor);

        FinVisitor finVisitor(this, false);
        VisitParentFirst(&finVisitor);
    }
}

void SprueModel::Regenerate()
{
    RegenerateMesh();
    std::vector<ModelPiece*> modelPieces = this->GetChildrenOfType<ModelPiece>();

    // Scope for destructor
    {
        // Use CSG to combine meshes that aren't SDF meshed and don't retain UVs
        CSGMeshVisitor csgVisitor(this, false /*use UV coordinates*/);
        VisitParentFirst(&csgVisitor);
    }

    //RegenerateTexture();

    // Scope for destructor
    //{
    //    // Same as above, but these meshes ARE UV MAPPED
    //    CSGMeshVisitor csgVisitor(this, true /* use UV coordinates*/);
    //    VisitParentFirst(&csgVisitor);
    //}
}

void SprueModel::Reset()
{

}

void SprueModel::RegenerateMesh()
{
    OctreeNode* octree = BuildOctree(this, Vec3(0, 0, 0), 8, 0.75f);
    VertexBuffer vbo;
    IndexBuffer ibo;
    GenerateMeshFromOctree(octree, vbo, ibo);

    MeshData* meshData = new MeshData();
    meshData->positionBuffer_.resize(vbo.size());
    meshData->normalBuffer_.resize(vbo.size());

    for (unsigned i = 0; i < vbo.size(); ++i)
    {
        meshData->positionBuffer_[i] = vbo[i].Position;
        meshData->normalBuffer_[i] = vbo[i].Normal;
    }
    meshData->indexBuffer_ = ibo;

    meshData->CalculateTangents();

    meshedModelParts_.reset(new MeshResource());
    meshedModelParts_->GetMeshes().push_back(meshData);
}

void SprueModel::RegenerateTexture()
{
    //TODO
}

BoundingBox SprueModel::ComputeBounds() const
{
    return bounds_ = BoundingBox();
}

std::shared_ptr<MeshResource> SprueModel::CloneMeshedParts()
{
    if (!meshedModelParts_)
        return std::shared_ptr<MeshResource>();

    std::shared_ptr<MeshResource> ret(new MeshResource());
    for (unsigned i = 0; i < meshedModelParts_->GetMeshCount(); ++i)
        ret->GetMeshes().push_back(meshedModelParts_->GetMesh(i)->CloneRaw());
    ret->SetSkeleton(meshedModelParts_->GetSkeleton()->Clone());
    return ret;
}

IEditable* SprueModel::Clone() const
{
    if (IEditable* clone = base::Clone())
    {
        if (SprueModel* cl = dynamic_cast<SprueModel*>(clone))
            cl->meshedModelParts_ = meshedModelParts_;
        return clone;
    }
    return 0x0;
}

}