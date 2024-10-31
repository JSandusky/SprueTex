#pragma once

#include <SprueEngine/Voxel/VoxelData.h>

#include <stdint.h>
#include <set>

namespace SprueEngine
{
    // Cells (Y dimension)
    // Voxels (X dimension)
    // Buffer CellData
    /*
    struct {
        unsigned nodeMortonCode;
        unsigned dataOffset;        // only non-zero if it's a real cell
        unsigned fillMaterialID;    // only used if it is a zero cell
    }
    */
    // Buffer VoxelData
    /* 
    struct {
        ushort materialID;
        half x;
        half y;
        half z;
    }

    // nVidia OCL marching cubes uses: 6 kernels for scan and 4 reads
    // Histogram Pyramids method uses more than 8-12 kernels

Baseline is 
    2 scans, 3 working kernels 

    // Writing an SVO into a chunk (user edits, l-system results, etc)
        Kernel #1 - Cell Blit
            in CellData
            inout FinalVoxelData
            out DataToScan
        SCAN Kernel #2 - Scan
            in DataToScan
            out ScanResults
        Kernel #2 - Voxel Blit
            in CellData
            in VoxelData
            inout FinalVoxelData

    // Procedural step (terrain, caves, dungeons)
        Kernel - Generation of voxel data

    // Primitives step (
        Kernel - Render shape description and write into voxel-data

    // Raw buffer step
        Kernel - write data into voxel data

    Voxelization #1 - Build boundary
        in voxeldata
        out DataToScan
    SCAN Voxelization #2 - Scan boundary data, sum of this scan says how many vertices exist
        in DataToScan
        out DataToScanResults
    Voxelization #3 - Compute normals & Calculate triangle counts
        in DataToScanResults
        out VertexBufferData (position + normal + material)
        out TriangleCounts
    SCAN Voxelizatin #4 - Scan triangle counts
        in TriangleCounts
        out TriangleScan
    Voxelization #5 - Output
        in voxelData
        in TriangleScanResults
        in DataToScanResults
        out TriangleIndices

    33 samples
    */

    struct SPRUE SparseVoxelOctree
    {
        unsigned char childMask_;       // Mask for child containment
        // Palette size only needs to be as big as the maximum number of non-leaf nodes
        unsigned char paletteIndex_;    // Index into the chunk palette, probably reduced to a bitfield based on max tree depth
    };

    template<int CELLS> // cells may be 2 (2^3, 8 children) or 4 (2^6, 64 children), 8 (2^9, 512 children)  would be absurd and will not fit into 1 byte of storage
    struct SPRUE OctreeBitMask
    {
        /// Number of bits for a 111 or 111111 mask
        static const int Bits = CELLS + (CELLS / 2);
        /// How many masks we could fit into the mask, in practice this would not infact be used except as a guessing mechanism. (3-4 levels of depth)
        static const int MaxDepth = 32 / (CELLS + (CELLS / 2));
        /// A value that is >= means the LOCAL (to this cell) mask ID has exceeded the valid range (CELLS == 2 == b1000 and CELLS == 4 == b10000
        static const int OverrunValue = 1 << (CELLS + (CELLS / 2));

        /// Shift off the correct number of bits.
        inline uint32_t GetParent(uint32_t currentPath) { return currentPath >> Bits; }
        /// Shift forward the corrent number of bits and then && with the new value.
        inline uint32_t GetChild(uint32_t currentPath, uint8_t childID) { return (currentPath << Bits) & childID; }
        /// If the popped value of two cells are equal then they have the same parent
        inline bool AreSiblings(uint32_t lhs, uint32_t rhs) { return Pop(lhs) == Pop(rhs); }

        inline bool Contains(uint32_t parent, uint32_t fullChildID) { return false; }
    };

    template<2>
    struct SPRUE OctreeBitMask
    {
        static const uint32_t Bits = 3;
        static const uint32_t Size = 2;
        static const uint32_t MaxDepth = 32 / 3;
        static const uint32_t OverrunValue = 1 << 3;
        static const uint32_t WipeParentMask = 0xFFFFFFFF << 3;
        static const uint8_t YBit = 1 << 2;
        static const uint8_t ZBit = 1 << 1;
        static const uint8_t XBit = 1;

        /// Shift off the correct number of bits.
        inline uint32_t GetParent(uint32_t currentPath) { return currentPath >> 3; }
        /// Shift forward the corrent number of bits and then && with the new value.
        inline uint32_t GetChild(uint32_t currentPath, uint8_t childID) { return (currentPath << 3) & childID; }
        /// If the popped value of two cells are equal then they have the same parent
        inline bool AreSiblings(uint32_t lhs, uint32_t rhs) { return (lhs >> 3) == (rhs >> 3); }
        inline uint8_t LocalID(uint32_t id) { return ~WipeParentMask & id; }

        static Vec3 LocalPos[] = { 
            Vec3(0, 0, 0), Vec3(0, 0, 1), Vec3(1, 0, 0), Vec3(1, 0, 1),  //bottom
            Vec3(0, 1, 0), Vec3(0, 1, 1), Vec3(1, 1, 0), Vec3(1, 1, 1) };

        inline Vec3 LocalPosition(uint32_t index) { return Vec3(index & XBit, index & YBit, index & zBit); }

        uint32_t GetDepth(uint32_t code) { return log2(code) / 3; }

        Vec3 ToPosition(uint32_t lhs)
        {
            Vec3 localPos = LocalPosition(lhs);
            uint32_t xPos = 0;
            uint32_t yPos = 0;
            uint32_t zPos = 0;
            uint32_t current = lhs;
            while (current)
            {
                localPos = (localPos * (Size * depth) + LocalPosition(lhs);
                xPos |= 
                lhs = GetParent(lhs);
                current << Bits;
            }
            return localPos;
        }
    };

    template<4>
    struct SPRUE OctreeBitMask
    {
        static const uint32_t Bits = 6;
        static const uint32_t MaxDepth = 32 / 6;
        static const uint32_t OverrunValue = 1 << 6;

        /// Shift off the correct number of bits.
        inline uint32_t GetParent(uint32_t currentPath) { return currentPath >> 6; }
        /// Shift forward the corrent number of bits and then && with the new value.
        inline uint32_t GetChild(uint32_t currentPath, uint8_t childID) { return (currentPath << 6) & childID; }
        /// If the popped value of two cells are equal then they have the same parent
        inline bool AreSiblings(uint32_t lhs, uint32_t rhs) { return (lhs >> 6) == (rhs >> 6); }
    };

    union SPRUE SVONodeID
    {
        uint32_t value_;
    };

    struct SPRUE SVOChunk
    {
        VoxelProperties properties_[255];
        SparseVoxelOctree root_;

        // Mapped by packed octree cell ID
        std::set<SparseVoxelOctree> octreeCells_;
        std::set<PackedFullVoxel> voxelData_;
    };
}