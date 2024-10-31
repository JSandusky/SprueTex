#pragma once

#include <SprueEngine/Core/SceneObject.h>

namespace SprueEngine
{
    /// A component is a piece of attachable functionality to a sprue kit object.
    class SPRUE Component : public SceneObject
    {
        BASECLASSDEF(Component, SceneObject);
        NOCOPYDEF(Component);
    public:
        /// Construct.
        Component() : SceneObject() { }
        /// Destruct.
        virtual ~Component() { }

        /// Register properties, basically just a copy in the case of this object.
        static void Register(Context* context);
    };

}