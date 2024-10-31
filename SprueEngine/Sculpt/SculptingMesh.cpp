#include "SculptingMesh.h"

#include <SprueEngine/Core/Context.h>>

namespace SprueEngine
{

    SculptingMesh::SculptingMesh()
    {

    }

    SculptingMesh::~SculptingMesh()
    {

    }

    void SculptingMesh::Register(Context* context)
    {
        context->RegisterFactory<SculptingMesh>("SculptingMesh", "A singular mesh object from a model file");
        REGISTER_PROPERTY_MEMORY(SculptingMesh, unsigned, offsetof(SculptingMesh, subMeshIndex_), 0, "Submesh Index", "", PS_Secret | PS_VisualConsequence);
        REGISTER_RESOURCE(SculptingMesh, MeshResource, GetMeshResource, SetMeshResource, GetMesh, SetMesh, ResourceHandle("Mesh"), "Mesh", "", PS_Secret);
    }

    nv::ProximityGrid* SculptingMesh::GetProximityGrid()
    {
        if (proximityGrid_ == 0x0)
            RebuildMeshMeta();
        return proximityGrid_;
    }
    
    void SculptingMesh::RebuildProximityGrid()
    {
        if (proximityGrid_)
            proximityGrid_->reset();
        else
            proximityGrid_ = new nv::ProximityGrid();
        
        if (resource_)
            proximityGrid_->init(resource_->GetMesh(subMeshIndex_)->GetPositionBuffer());
    }

    nv::HalfEdge::Mesh* SculptingMesh::GetHalfEdgeMesh()
    {
        if (halfEdgeMesh_ == 0x0)
            RebuildMeshMeta();
        return halfEdgeMesh_;
    }

    void SculptingMesh::RebuildHalfEdgeMesh()
    {
        if (halfEdgeMesh_)
            delete halfEdgeMesh_;
        halfEdgeMesh_ = 0x0;

        if (resource_)
            halfEdgeMesh_ = resource_->GetMesh(subMeshIndex_)->BuildHalfEdgeMesh();
    }

    void SculptingMesh::RebuildMeshMeta()
    {
        RebuildProximityGrid();
        RebuildHalfEdgeMesh();
    }

    MeshData* SculptingMesh::GetMeshData()
    {
        if (resource_)
            return resource_->GetMesh(subMeshIndex_);
        return 0x0;
    }

    SculptingMeshGroup::SculptingMeshGroup()
    {

    }

    SculptingMeshGroup::~SculptingMeshGroup()
    {

    }

    void SculptingMeshGroup::Register(Context* context)
    {
        context->RegisterFactory<SculptingMesh>("SculptingMeshGroup", "");
    }

}