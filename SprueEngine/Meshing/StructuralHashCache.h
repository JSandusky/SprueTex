#pragma once

#include <SprueEngine/ClassDef.h>
#include <SprueEngine/Core/SprueModel.h>
#include <SprueEngine/StringHash.h>

#include <map>

namespace SprueEngine
{
    /// Maintains a cache of arbitrary objects mapped by the 'structural' hash of a SprueModel objects. 
    /// A use-case would be storing precompiled shaders based on the structure of the SprueModel.
    /// The assumption is made that any given SprueModel instance only needs one corresponding cache object, as it is for reuse and reconstruction.
    template<class T> 
    class SPRUE StructuralHashCache
    {
    public:
        /// Construct.
        StructuralHashCache() 
        { 
        }

        /// Destruct.
        ~StructuralHashCache() 
        {
            Clear();
        }

        /// Gets the shader for a particular model.
        T* GetDataFor(SprueModel* model)
        {
            if (model == 0x0)
                return 0x0;

            unsigned modelID = model->GetInstanceID();
            if (model->IsClone())
                modelID = model->GetSourceID();

            const unsigned currentHash = model->StructuralHash();

            // If we find an existing record and the hashes match then return the existing ptr.
            auto foundRecord = data_.find(modelID);
            if (foundRecord != data_.end() && foundRecord->second.first == currentHash)
                return foundRecord->second.second;
            else
            {
                T* newData = CreateNewData(model);
                if (newData)
                {
                    // Replace or insert the new shader
                    if (foundRecord != data_.end())
                    {
                        foundRecord->second.first = currentHash;
                        DestroyData(foundRecord->second.second);
                        foundRecord->second.second = newData;
                    }
                    else
                        data_.insert(std::make_pair(modelID, std::make_pair(currentHash, newData)));

                    return newData;
                }
            }
            return 0x0;
        }

        /// Erases all items in the cache.
        void Clear()
        {
            for (auto item : data_)
            {
                if (item.second.second != 0x0)
                    DestroyData(item.second.second);
            }
            data_.clear();
        }

    protected:
        virtual T* CreateNewData(SprueModel* forModel) = 0;
        virtual void DestroyData(T* data) = 0;

        std::map<unsigned, std::pair<unsigned, T*> > data_;
    };

}