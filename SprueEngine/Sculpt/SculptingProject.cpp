#include "SculptingProject.h"

#include <SprueEngine/Core/Context.h>

namespace SprueEngine
{

    void RegisterSculptingTypes(Context* context)
    {
        SculptingProject::Register(context);
        SculptingMesh::Register(context);
        TextureLayerGroup::Register(context);
        TexturePaintLayer::Register(context);
    }

    SculptingProject::SculptingProject()
    {

    }

    SculptingProject::~SculptingProject()
    {

    }

    void SculptingProject::Register(Context* context)
    {
        context->RegisterFactory<SculptingProject>("SculptingProject", "");
    }

    SculptingProject* SculptingProject::Create(std::shared_ptr<MeshResource> resource)
    {
        SculptingProject* ret = new SculptingProject();
        ret->rootGroup_ = new SculptingMeshGroup();
        ret->rootLayer_ = new TextureLayerGroup();
        ret->rootLayer_->SetName("Layers");

        auto meshRes = std::dynamic_pointer_cast<MeshResource>(resource->Clone());

        for (unsigned i = 0; i < resource->GetMeshCount(); ++i)
        {
            SculptingMesh* meshObject = new SculptingMesh();
            meshObject->SetMeshResource(meshRes->GetHandle());
            meshObject->SetMesh(meshRes);
            meshObject->SetMeshSubIndex(i);
            ret->rootGroup_->GetMeshes().push_back(meshObject);
        }

        return ret;
    }
}