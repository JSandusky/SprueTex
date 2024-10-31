#pragma once

#include <SprueEngine/ClassDef.h>
#include <SprueEngine/IEditable.h>
#include <SprueEngine/Core/SceneObject.h>
#include <SprueEngine/Resource.h>
#include <SprueEngine/Geometry/MeshData.h>

#include <vector>

namespace SprueEngine
{
    class Context;
    class Serializer;
    class Deserializer;

    premiere class SPRUE SprueModel : public SceneObject
    {
        NOCOPYDEF(SprueModel);
        SPRUE_EDITABLE(SprueModel);
    public:
        SprueModel();
        virtual ~SprueModel();

        static void Register(Context*);

        bool LoadModel(Deserializer* src, const SerializationContext& context);
        bool SaveModel(Serializer* dest, const SerializationContext& context);

#ifndef SPRUE_NO_XML
        bool LoadModel(tinyxml2::XMLDocument* doc, const SerializationContext& context);
        bool LoadModel(tinyxml2::XMLElement* fromElement, const SerializationContext& context);
        bool SaveModel(tinyxml2::XMLDocument* intoDoc, const SerializationContext& context);
#endif

        void ProcessCSG();
        void ProcessTexture();
        void Regenerate();
        void Reset();

        unsigned GetGenerationDepth() const { return generatorDepth_; }
        void SetGenerationDepth(unsigned depth) { generatorDepth_ = depth; }

        float GetError() const { return error_; }
        void SetError(float val) { error_ = val; }

        virtual BoundingBox ComputeBounds() const override;

        std::shared_ptr<MeshResource> CloneMeshedParts();
        std::shared_ptr<MeshResource>& GetMeshedParts() { return meshedModelParts_; }
        const std::shared_ptr<MeshResource>& GetMeshedParts() const { return meshedModelParts_; }
        void SetMeshedParts(std::shared_ptr<MeshResource> parts) { meshedModelParts_ = parts; }

        virtual IEditable* Clone() const override;

    protected:
        void RegenerateMesh();
        void RegenerateTexture();

    private:
        class FinVisitor;
        class CSGMeshVisitor;
        class TextureVisitor;

        unsigned generatorDepth_;
        float error_ = 0.25f;
#ifndef CppSharp        
        /// Non-meshed child models may result in multiple pieces if they are marked to stay independent
        std::shared_ptr<MeshResource> meshedModelParts_;
#endif
    };

}