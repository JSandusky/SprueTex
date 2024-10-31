#include "Component.h"

#include "Context.h"

namespace SprueEngine
{

    void Component::Register(Context* context)
    {
        context->CopyBaseProperties("SceneObject", "Component");
    }

}