#pragma once

#include <SprueBindings/Geometry.h>

using namespace System;
using namespace System::Collections::Generic;
using namespace Microsoft::Xna::Framework;

namespace SprueBindings
{

    public ref class BMeshBall
    {
    public:
        Vector3 position_;
        int ballType_;
        float radius_;
        float radiusX_;
        float radiusY_;
        Vector3 axisX_;
        Vector3 axisY_;
        Vector3 axisZ_;
    };

    public ref class BMeshConnection
    {
    public:
        int from_;
        int to_;
    };

    public ref class BMeshBuilder {
    public:
        MeshData^ Build(array<BMeshBall^>^ balls, array<BMeshConnection^>^ links, int subdivs);
    };
}