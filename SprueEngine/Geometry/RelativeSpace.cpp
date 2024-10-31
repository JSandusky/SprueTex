#include "RelativeSpace.h"

#include <SprueEngine/MathGeoLib/Algorithm/Random/LCG.h>

namespace SprueEngine
{
    float RelativeSpaceAdjustValue(float value, unsigned char relativeSpaceMode, float maximumValue)
    {
        LCG lcg;
        if (relativeSpaceMode & (RSM_In & RSM_Out))
            return value + lcg.Float(-maximumValue, maximumValue);
        else if (relativeSpaceMode & RSM_In)
            return value + lcg.Float(0.0f, maximumValue);
        else if (relativeSpaceMode = RSM_Out)
            return value - lcg.Float(0.0f, maximumValue);
    }

    Vec3 RelativeSpaceAdjustVec3(const Vec3& value, unsigned char xSpaceMode, unsigned char ySpaceMode, unsigned char zSpaceMode, float xVal, float yVal, float zVal)
    {
        return Vec3(RelativeSpaceAdjustValue(value.x, xSpaceMode, xVal), RelativeSpaceAdjustValue(value.y, ySpaceMode, yVal), RelativeSpaceAdjustValue(value.z, zSpaceMode, zVal));
    }

    void RelativeSpaceAdjustVec3List(Vec3* values, unsigned valueCt, unsigned char xSpaceMode, unsigned char ySpaceMode, unsigned char zSpaceMode, float xVal, float yVal, float zVal)
    {
        Vec3 adjustment(RelativeSpaceAdjustValue(0.0f, xSpaceMode, xVal), RelativeSpaceAdjustValue(0.0f, ySpaceMode, yVal), RelativeSpaceAdjustValue(0.0f, zSpaceMode, zVal));
        for (int i = 0; i < valueCt; ++i)
            values[i] += values[i].Normalized() * adjustment;
    }

    void RelativeSpaceAdjustVec3ListUnique(Vec3* values, unsigned valueCt, unsigned char xSpaceMode, unsigned char ySpaceMode, unsigned char zSpaceMode, float xVal, float yVal, float zVal)
    {
        for (int i = 0; i < valueCt; ++i)
            values[i] += values[i].Normalized() * Vec3(RelativeSpaceAdjustValue(0.0f, xSpaceMode, xVal), RelativeSpaceAdjustValue(0.0f, ySpaceMode, yVal), RelativeSpaceAdjustValue(0.0f, zSpaceMode, zVal));
    }
}