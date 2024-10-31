#pragma once

#include "Geometry.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace Microsoft::Xna::Framework;

namespace SprueBindings
{

    public enum class CSGTask
    {
        Independent,
        ClipIndependent,
        Merge,
        MergeSmoothNormals,
        CSG_Add,
        CSG_Subtract,
        CSG_Clip
    };

    public ref class CSGOperand
    {
    public:
        MeshData^ Geometry;
        Matrix Transform;
        CSGTask Task;
        float SmoothingPower = 1.0f;
        bool UseSmoothing = false;
    };

    public ref class CSGProcessor
    {
    public:
        MeshData^ ProcessUnions(List<CSGOperand^>^ operands, PluginLib::IErrorPublisher^ errors);
    };

}