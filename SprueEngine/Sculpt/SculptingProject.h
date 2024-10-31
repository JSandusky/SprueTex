#pragma once

#include <SprueEngine/Sculpt/SculptingMesh.h>
#include <SprueEngine/Sculpt/TexturePaintLayer.h>

namespace SprueEngine
{

    /// The root object in a sculpting/painting project.
    class SPRUE SculptingProject : public IEditable
    {
        NOCOPYDEF(SculptingProject);
        BASECLASSDEF(SculptingProject, IEditable);
        SPRUE_EDITABLE(SculptingProject);
    public:
        /// Construct.
        SculptingProject();
        /// Destruct.
        virtual ~SculptingProject();
        /// Register factory and properties.
        static void Register(Context*);

        /// Create a new instance with the groups already setup.
        static SculptingProject* Create(std::shared_ptr<MeshResource> resource);

        /// Returns the top level (non-deletable) mesh group.
        SculptingMeshGroup* GetMeshGroup() { return rootGroup_; }
        /// Returns the top level (non-deletable) layer group.
        TextureLayerGroup* GetLayerGroup() { return rootLayer_; }

    private:
        SculptingMeshGroup* rootGroup_;
        TextureLayerGroup* rootLayer_;
    };

    void RegisterSculptingTypes(Context*);
}