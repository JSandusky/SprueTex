// Original CSG.JS library by Evan Wallace (http://madebyevan.com), under the MIT license.
// GitHub: https://github.com/evanw/csg.js/
// 
// C++ port by Tomasz Dabrowski (http://28byteslater.com), under the MIT license.
// GitHub: https://github.com/dabroz/csgjs-cpp/
// 
// Constructive Solid Geometry (CSG) is a modeling technique that uses Boolean
// operations like union and intersection to combine 3D solids. This library
// implements CSG operations on meshes elegantly and concisely using BSP trees,
// and is meant to serve as an easily understandable implementation of the
// algorithm. All edge cases involving overlapping coplanar polygons in both
// solids are correctly handled.
//
// To use this as a header file, define CSGJS_HEADER_ONLY before including this file.
//

// 5/12/2016: Jonathan Sandusky - modifications:
//      Split into header/source instead of #ifdef check
//      convert to SprueEngine types (Vec3)
//      remove "this->" psychosis
//      Regularly "fixing" code style as encountered
//  TODO:
//      Replace csgjs_model with SprueEngine mesh types
//      Output a list of seam vertices (in order to smooth specific vertices)
//      Move functionality into TinyEditableMesh?

#pragma once

#include <SprueEngine/MathGeoLib/AllMath.h>
#include <vector>

namespace SprueEngine
{

struct csgjs_vertex
{
    Vec3 pos;
    Vec3 normal;
    Vec2 uv;
    unsigned sourceIndex = -1; // Vertex came from Vertex N in the source mesh
    bool isNew = false; // Vertex was created as a result of CSG
};

struct csgjs_model
{
    std::vector<csgjs_vertex> vertices;
    std::vector<int> indices;
};

// public interface - not super efficient, if you use multiple CSG operations you should
// use BSP trees and convert them into model only once. Another optimization trick is
// replacing csgjs_model with your own class.

class MeshData;

/// Fill the given csgjs_model based on the input mesh, optionally with UV
void PrepareCSGModelData(const MeshData* mesh, csgjs_model& model, bool includeUV = false);
/// Fill the given mesh based on the input csgjs_model, optionally with UV
void TransferCSGModelData(MeshData* mesh, csgjs_model& model, bool includeUV = false);

csgjs_model csgjs_union(const csgjs_model & a, const csgjs_model & b);
csgjs_model csgjs_intersection(const csgjs_model & a, const csgjs_model & b);
csgjs_model csgjs_difference(const csgjs_model & a, const csgjs_model & b);

}