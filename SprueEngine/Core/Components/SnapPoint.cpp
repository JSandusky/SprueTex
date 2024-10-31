#include "SnapPoint.h"

#include <SprueEngine/Core/Context.h>

namespace SprueEngine
{

    SnapPoint::SnapPoint() :
        Component()
    {

    }

    SnapPoint::~SnapPoint()
    {

    }

    void SnapPoint::Register(Context* context)
    {
        context->RegisterFactory<SnapPoint>("SnapPoint", "");
        context->CopyBaseProperties("SceneObject", "SnapPoint");
        // NO PROPERTIES
    }

    void SnapPoint::DrawDebug(IDebugRender* renderer, unsigned flags) const
    {
        RGBA color = flags & SPRUE_DEBUG_HOVER ? RGBA::Gold : RGBA::Green;
        color.a = 0.75f;
        if (flags & SPRUE_DEBUG_PASSIVE)
            color.a = 0.1f;
    }

}

