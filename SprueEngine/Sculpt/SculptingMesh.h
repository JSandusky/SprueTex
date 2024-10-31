#pragma once

#include <SprueEngine/IEditable.h>
#include <SprueEngine/Geometry/MeshData.h>
#include <SprueEngine/Resource.h>

#include <SprueEngine/Libs/nvmath/ProximityGrid.h>
#include <SprueEngine/Libs/nvmesh/halfedge/Mesh.h>

namespace SprueEngine
{

    class SPRUE SculptingMesh : public IEditable
    {
        NOCOPYDEF(SculptingMesh);
        BASECLASSDEF(SculptingMesh, IEditable);
        SPRUE_EDITABLE(SculptingMesh);
    public:
        SculptingMesh();
        virtual ~SculptingMesh();
        static void Register(Context*);

        ResourceHandle GetMeshResource() const { return meshResource_; }
        void SetMeshResource(const ResourceHandle& handle) { meshResource_ = handle; }
        std::shared_ptr<MeshResource> GetMesh() const { return resource_; }
        void SetMesh(const std::shared_ptr<MeshResource>& res) { resource_ = res; }

        nv::ProximityGrid* GetProximityGrid();
        void RebuildProximityGrid();
        nv::HalfEdge::Mesh* GetHalfEdgeMesh();
        void RebuildHalfEdgeMesh();
        void RebuildMeshMeta();

        unsigned GetSubMeshIndex() const { return subMeshIndex_; }
        void SetMeshSubIndex(unsigned val) { subMeshIndex_ = val; }
        MeshData* GetMeshData();

    private:
        nv::ProximityGrid* proximityGrid_ = 0x0;
        nv::HalfEdge::Mesh* halfEdgeMesh_ = 0x0;
        ResourceHandle meshResource_;
        std::shared_ptr<MeshResource> resource_;
        unsigned subMeshIndex_ = 0;
    };

    class SPRUE SculptingMeshGroup : public IEditable
    {
        NOCOPYDEF(SculptingMeshGroup);
        BASECLASSDEF(SculptingMeshGroup, IEditable);
        SPRUE_EDITABLE(SculptingMeshGroup);
    public:
        SculptingMeshGroup();
        virtual ~SculptingMeshGroup();
        static void Register(Context*);

        std::vector<SculptingMesh*>& GetMeshes() { return meshes_; }

    private:
        std::vector<SculptingMesh*> meshes_;
    };

}