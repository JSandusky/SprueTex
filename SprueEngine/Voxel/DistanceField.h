#pragma once
#ifdef USE_OPENVDB

#include <SprueEngine/ClassDef.h>
#include <SprueEngine/MathGeoLib/AllMath.h>

namespace SprueEngine
{
    class Deserializer;
    class MeshData;
    class Serializer;

    class DistanceField
    {
        NOCOPYDEF(DistanceField);
    public:
        DistanceField();
        DistanceField(MeshData* meshData, float density);
        virtual ~DistanceField();

        float GetDistance(float x, float y, float z, bool normalizeBounds);
        int TraceSignChanges(const Vec3& start, const Vec3& end, bool normalizeBounds);

        void Deserialize(Deserializer* src);
        void Serialize(Serializer* dest) const;

    private:
        class PrivateData;
        PrivateData* privateData_;
        float density_;
    };

}
#endif // DEBUG
