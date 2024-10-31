#pragma once

#include <SprueEngine/MathGeoLib/AllMath.h>

namespace SprueEngine
{
    union SPRUE VoxelProperties {
        uint16_t value_;
        // Undefined, but functions as expected on all platforms
        struct {
            // If it is not solid, air, or fluid then it must be a surface voxel, and the position will be valid
            unsigned validPos_ : 1;     // Voxel is an edge voxel and neither underground nor air, position will be valid
            unsigned twoSided_ : 1;     // Voxel requires double faces for recreating a tiny strip, faces will be emitted for both directions
            unsigned written_ : 1;      // Voxel has written data
            unsigned id_ : 13;          // Allows for 2^13 - 1 materials, (material 0 is always air)
        };
    };

    struct SPRUE FullVoxel
    {
        Vec3 cellPosition_;
    };

    // AND with this to "fullness" (either fully air or fully solid, fully air will have 0 for material id)
    #define IS_FILLED_VOXEL 0x8000
    // AND with this to test for two sided
    #define IS_TWO_SIDED 0x4000
    #define IS_WRITTEN 0x2000
    // AND with this to filter to material ID only
    #define ID_MASK 0x1FFF
    #define DEFAULT_SOLID_VOXEL 0x2001
    #define VOID_VOXEL 0x0
    #define DEFAULT_AIR_VOXEL 0x2000

    // 8 bytes, twice that of an RGBA8
    struct SPRUE PackedFullVoxel
    {
        // 2 bytes
        VoxelProperties properties_;

        // 3 bytes, relative to the voxel volume
        uint8_t x_, y_, z_;
        // 3 bytes, w_ is actually a the 1D form of a 2D index (64px chunks, giving each u/v value 1/4 pixel accuracy), texture comes from material
        uint8_t u_, v_, w_;

        void stuff()
        {
        }
    };
}