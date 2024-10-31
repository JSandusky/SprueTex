#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace Microsoft::Xna::Framework;

#include "Geometry.h"

namespace SprueBindings
{

    public delegate bool BoneWeightCancel();

    public ref class RCurve {
    public:
        int CurveShape;
        float x, y, slope, exp;
        bool flipX, flipY;
    };

    public ref class LaplaceHandle
    {
    public:
        Vector3 Position;
        int Index;
    };

    public ref class LaplaceProcessing
    {
    public:

        static MeshData^ Deform(MeshData^ target, List<LaplaceHandle^>^ controlPoints);
        static bool CalculateBoneWeights(MeshData^ target, RCurve^ respCurve, BoneWeightCancel^ cancelFunc);

    };

}