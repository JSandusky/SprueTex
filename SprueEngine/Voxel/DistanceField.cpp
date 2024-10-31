#ifdef USE_OPEN_VDB
#include "SprueEngine/Voxel/DistanceField.h"

#include "SprueEngine/Deserializer.h"
#include "SprueEngine/Geometry/MeshData.h"
#include "SprueEngine/Serializer.h"

#include <openvdb/openvdb.h>
#include <openvdb/tools/Interpolation.h>
#include <openvdb/tools/MeshToVolume.h>
#include <openvdb/tools/RayIntersector.h>
#include <openvdb/io/Stream.h>

#include <limits>
#include <stdint.h>
#include <iostream>
#include <streambuf>

namespace SprueEngine
{

    /*Iclass OStreamRouter : public std::basic_ostream<char>, public std::basic_streambuf<char>
    {
    public:
    OStreamRouter(Serializer* router) : output(router), std::basic_ostream<char>(this) { }

    template<class T>
    const OStreamRouter& operator<<(const T& v) const { output->Write(&v, sizeof(T)); return *this; }

    virtual std::streambuf* setbuf(char* s, std::streamsize n) override
    {
    output->Write(s, n);
    return this;
    }

    virtual int overflow(int c = std::char_traits<char>::eof()) override
    {
    if (c != std::char_traits<char>::eof())
    output->WriteInt(c);
    return std::char_traits<char>::eof();
    }

    Serializer* output;
    };

    class IStreamRouter : public std::basic_istream<char>, public std::basic_streambuf < char >
    {
    public:
    IStreamRouter(Deserializer* src) : src(src), std::basic_istream<char>(this) { }

    virtual std::streambuf* setbuf(char * s, std::streamsize n) override
    {
    src->Read(s, n);
    return this;
    }


    Deserializer* src;
    };*/

    class DistanceField::PrivateData
    {
    public:
        openvdb::FloatGrid::Ptr field_;

        typedef openvdb::FloatGrid GridType;
        static openvdb::Coord SharedSampleCoord;
        openvdb::FloatGrid::ConstAccessor* accessor_;

        typedef openvdb::tools::GridSampler<GridType, openvdb::tools::BoxSampler> SamplerType;
        SamplerType::Ptr gridSampler_;

        typedef openvdb::tools::VolumeRayIntersector<GridType> RayCasterType;
        RayCasterType* rayCaster_;

        PrivateData() : field_(0x0), rayCaster_(0x0), gridSampler_(0x0), accessor_(0x0)
        {

        }

        void ConstructMesh(const MeshData* meshData, float density)
        {
            // Prepare positions
            std::vector<openvdb::Vec3s> positions;
            const std::vector<Vec3>& posBuffer = meshData->GetPositionBuffer();
            positions.resize(posBuffer.size());
            for (int i = 0; i < posBuffer.size(); ++i)
                memcpy(&positions[i][0], &posBuffer[i].x, sizeof(float) * 3);


            // Prepare indices
            std::vector<openvdb::Vec3I> indices;
            const std::vector<uint32_t>& indexBuffer = meshData->GetIndexBuffer();
            indices.resize(indexBuffer.size());
            for (int i = 0; i < indexBuffer.size(); i += 3)
                memcpy(&indices[i][0], &indexBuffer[i], sizeof(uint32_t) * 3);

            // Junk
            std::vector<openvdb::Vec4I> quads;

            // Setup our transform, controls grid density
            openvdb::math::Transform::Ptr transform = openvdb::math::Transform::createLinearTransform(density);

            // Construct the grid
            field_ = openvdb::tools::meshToSignedDistanceField<GridType>(*transform, positions, indices, quads, 0.01f, 1.0f);

            PrepareData();
        }

        void PrepareData()
        {
            gridSampler_ = SamplerType::Ptr(new SamplerType(*field_));
            rayCaster_ = new RayCasterType(*field_);
        }

        ~PrivateData()
        {
            if (gridSampler_)
                gridSampler_.reset();
            if (rayCaster_)
                delete rayCaster_;
            if (field_)
                field_.reset();
        }
    };

    DistanceField::DistanceField() :
        privateData_(0x0)
    {

    }

    DistanceField::DistanceField(MeshData* meshData, float density)
        :
        density_(density)
    {
        privateData_ = new DistanceField::PrivateData();
        privateData_->ConstructMesh(meshData, density);
    }

    DistanceField::~DistanceField()
    {
        if (privateData_)
            delete privateData_;
    }

    float DistanceField::GetDistance(float x, float y, float z, bool normalizeBounds)
    {
        if (privateData_ != 0x0 && privateData_->field_.get() && privateData_->gridSampler_.get())
            return openvdb::tools::BoxSampler::sample(privateData_->field_->tree(), openvdb::Vec3R(x, y, z));
        //return privateData_->gridSampler_->wsSample(openvdb::Vec3f(x, y, z));
        return std::numeric_limits<float>::max();
    }

    int DistanceField::TraceSignChanges(const Vec3& start, const Vec3& end, bool normalizeBounds)
    {
        int changeCt = 0;
        if (privateData_ != 0x0 && privateData_->field_.get() && privateData_->rayCaster_)
        {
            if (privateData_->rayCaster_->setWorldRay(openvdb::math::Ray<double>(openvdb::Vec3R(start.x, start.y, start.z), openvdb::Vec3R(end.x, end.y, end.z))))
            {
                //openvdb::tools::VolumeRayIntersector
                std::vector< openvdb::math::Ray<double>::TimeSpan> hits;
                privateData_->rayCaster_->hits(hits);
                return (int)hits.size();
            }
        }
        return changeCt;
    }

    void DistanceField::Deserialize(Deserializer* src)
    {
        density_ = src->ReadFloat();
        // Do we have data?
        // if (src->ReadBool())
        // {
        //     if (privateData_ && privateData_->field_.get())
        //     {
        //         
        //     }
        // }
    }

    void DistanceField::Serialize(Serializer* dest) const
    {
        dest->WriteFloat(density_);
        // if (privateData_ && privateData_->field_.get())
        // {
        //     dest->WriteBool(true); // has data
        //     OStreamRouter router(dest);
        //     openvdb::GridPtrVecPtr vec(new openvdb::GridPtrVec());
        //     vec->push_back(privateData_->field_);
        //     openvdb::io::Stream(router).write(*vec);
        // }
        // else
        //     dest->WriteBool(false); // no data
    }

}
#endif // DEBUG
