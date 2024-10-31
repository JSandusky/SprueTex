#pragma once

#include <SprueEngine/ClassDef.h>

namespace SprueEngine
{

    /// A PolyObject is a base for any object that contains a collection of nodes.
    class SPRUE PolyObject
    {
    public:
        /// Returns true if this object forms a closed loop.
        virtual bool IsCircular() const = 0;
    };

}