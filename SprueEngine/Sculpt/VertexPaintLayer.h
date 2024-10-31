#pragma once

#include <SprueEngine/IEditable.h>

namespace SprueEngine
{

    /// A painting layer for per-vertex colors.
    class SPRUE VertexPaintLayer : public IEditable
    {
        NOCOPYDEF(VertexPaintLayer);
        BASECLASSDEF(VertexPaintLayer, IEditable);
        SPRUE_EDITABLE(VertexPaintLayer);
    public:
        /// Construct.
        VertexPaintLayer();
        /// Destruct.
        virtual ~VertexPaintLayer();
        /// Register factory and properties.
        static void Register(Context*);



    private:
        std::vector<RGBA> vertexColors_;
    };

}