#include "ThermalSkinning.h"

#include <SprueEngine/Math/MathDef.h>
#include <SprueEngine/Geometry/MeshData.h>
#include <SprueEngine/Voxel/DistanceField.h>
#include <SprueEngine/Geometry/Skeleton.h>

namespace SprueEngine
{

void HeatDiffusionSkinning::CalculateWeights(MeshData* mesh, Skeleton* skeleton) const
{
    //DistanceField* distanceField = mesh->GetDistanceField();

    const unsigned jointCt = skeleton->GetAllJoints().size();
    const unsigned vertCt = mesh->GetPositionBuffer().size();
    const unsigned triCt = mesh->GetIndexBuffer().size() / 3;

    // Computed table of distances of each vert to each joint, rough guesstimate, the table will be around 1-3 mb in size for sane models
    float* distances = new float[vertCt * jointCt];
    float* triangleDistances = new float[jointCt * triCt];
    unsigned short* signChanges = new unsigned short[vertCt * jointCt];

    const std::vector<Vec3>& pos = mesh->GetPositionBuffer();
    const std::vector<Joint*>& joints = skeleton->GetAllJoints();

    // TODO, iterate on triangles instead of vertices?
    // Cast ray to triangle centroid, confirm currently processed triangle is the best hit
    // Diffuse weights progressively until everything is weighted
    // For each vertex the weights are the average for all of the triangles it is connected to
    //      Will require using mesh topology

    for (unsigned jointIdx = 0; jointIdx < jointCt; ++jointIdx)
    {
        for (unsigned vertIdx = 0; vertIdx < vertCt; ++vertIdx)
        {
            const int idx = ToArrayIndex(jointIdx, vertIdx, 1, jointCt, vertCt, 1);
            //TODO calculate as distance from line segment formed by the bone
            //TODO cast ray and count the intersections
            signChanges[idx] = 0;
            distances[idx] = (pos[vertIdx] - Vec3(0, 0, 0)).LengthSq();
        }
    }

    //TODO, deal with vertices that have aren't easily hit by the rays

}

}