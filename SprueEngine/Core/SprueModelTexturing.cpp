#include "SprueModel.h"

#include <SprueEngine/BlockMap.h>
#include <SprueEngine/Core/Components/TexturingComponent.h>
#include <SprueEngine/Texturing/RasterizerData.h>
#include <SprueEngine/Texturing/SprueTextureBaker.h>

#include <memory>

namespace SprueEngine
{

    class SprueModel::TextureVisitor : public TypeLimitedSceneObjectVisitor<TexturingComponent>
    {
    public:
        std::vector<TexturingComponent*> components_;


        virtual ~TextureVisitor()
        {
        }

        virtual bool Visit(TexturingComponent* child) override
        {
            if (!child->IsDisabled())
                components_.push_back(child);
            return true;
        }
    };

    void SprueModel::ProcessTexture()
    {
        TextureVisitor visitor;
        VisitParentFirst(&visitor);

        if (visitor.components_.size())
        {
            SprueTextureBaker baker(this, visitor.components_);
            FilterableBlockMap<RGBA>* image = baker.Bake();
            if (image)
            {
                BasicImageLoader::SavePNG(image, "TestRasterize.png");
                if (GetMeshedParts() && GetMeshedParts()->GetMeshCount())
                    GetMeshedParts()->GetMesh(0)->SetImage(image);
            }
        }
    }
}