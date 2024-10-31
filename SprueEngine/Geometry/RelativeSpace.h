#pragma once

#include <SprueEngine/ClassDef.h>
#include <SprueEngine/MathGeoLib/AllMath.h>

namespace SprueEngine
{

    enum RelativeSpaceMode
    {
        RSM_None = 0,
        RSM_Out = 1,
        RSM_In = 1 << 1,
        RSM_Synchronized = 1 << 2
    };

    SPRUE float RelativeSpaceAdjustValue(float value, unsigned char relativeSpaceMode, float maximumValue);
    SPRUE Vec3 RelativeSpaceAdjustVec3(const Vec3& value, unsigned char xSpaceMode, unsigned char ySpaceMode, unsigned char zSpaceMode, float xVal, float yVal, float zVal);
    /// Adjusts a list of values, all of which will be moved by identical amounts (in relation to themselves)
    SPRUE void RelativeSpaceAdjustVec3ListIdentical(Vec3* values, unsigned valueCt, unsigned char xSpaceMode, unsigned char ySpaceMode, unsigned char zSpaceMode, float xVal, float yVal, float zVal);
    /// Adjusts a list of values, all of which will be moved uniquely
    SPRUE void RelativeSpaceAdjustVec3ListUnique(Vec3* values, unsigned valueCt, unsigned char xSpaceMode, unsigned char ySpaceMode, unsigned char zSpaceMode, float xVal, float yVal, float zVal);
}