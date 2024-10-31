#pragma once

#include <SprueEngine/ClassDef.h>
#include <SprueEngine/MathGeoLib/AllMath.h>

namespace SprueEngine
{

    class SPRUE LSMath
    {
    public:
        static float Determinant(float a, float b, float c,
            float d, float e, float f,
            float g, float h, float i);

        static Vec3 Solve3x3(const float* A, const float b[3]);

        static Vec3 leastSquares(size_t N, const Vec3* A, const float* b);
    };
}