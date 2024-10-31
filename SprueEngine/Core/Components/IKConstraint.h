#pragma once

#include <SprueEngine/Core/Component.h>

namespace SprueEngine
{

    /// Sets up an IK constraint/damper for a bone.
    class SPRUE IKConstraint : public Component
    {
        BASECLASSDEF(IKConstraint, Component);
        NOCOPYDEF(IKConstraint);
        SPRUE_EDITABLE(IKConstraint);
    public:

    private:
        /// How intensely this bone should refuse to cooperate.
        float resistance_ = 0.0f;
    };

}