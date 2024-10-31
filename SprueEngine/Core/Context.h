#pragma once

#include <SprueEngine/Callbacks.h>
#include <SprueEngine/ClassDef.h>
#include <SprueEngine/Deserializer.h>
#include <SprueEngine/IEditableProperty.h>
#include <SprueEngine/Property.h>
#include <SprueEngine/SerializationContext.h>

#ifndef SPRUE_NO_XML
    #include <SprueEngine/Libs/tinyxml2.h>
#endif

namespace SprueEngine
{

class IContextService;
class MessageLog;
class ResourceLoader;
class SprueModel;

struct FactoryMethod
{
    virtual void* Create() = 0;
};

template<typename T>
struct TemplateFactoryMethod : public FactoryMethod
{
    virtual void* Create() override 
    {
        return (void*)new T();
    }
};

/// Manages the lifecycle of a model from construction -> final result
/// Also contains access to the backing threads for working in the background
class SPRUE Context
{
    static Context* instance_;
public:
    Context();
    ~Context();

    static Context* GetInstance();

    PropertyTable& GetPropertyTable() { return properties_; }

#ifndef CppSharp
    void Register(const StringHash& typeName, TypeProperty* prop);

    void CopyBaseProperties(const StringHash& fromType, const StringHash& intoType);

    void RemoveProperty(const StringHash& typeName, const char* propertyName);

    SprueEngine::TypeProperty* GetProperty(const StringHash& fromType, const StringHash& property);

    void RegisterHashName(const StringHash& hash, const std::string& name)
    {
        hashNames_[hash] = name;
    }

    template<typename T>
    void RegisterFactory(const std::string& hash, const std::string& description)
    {
        const StringHash hashValue(hash);
        auto found = factories_.find(hashValue);
        if (found != factories_.end())
            throw;
        factories_[hashValue] = new TemplateFactoryMethod<T>();
        hashNames_[hashValue] = hash;
        descriptions_[hashValue] = description;
    }

    template<typename T>
    T* Create(const StringHash& hash) const
    {
        auto found = factories_.find(hash);
        if (found != factories_.end())
            return static_cast<T*>(found->second->Create());
        return 0x0;
    }

    template<typename T>
    T* Deserialize(Deserializer* src, const SerializationContext& context) const
    {
        static_assert(std::is_base_of<IEditable, T>::value,"T must be derived from IEditable to use Context::Deserialize");

        StringHash hash = src->ReadStringHash();
        T* newObj = Create<T>(hash);
        SPRUE_ASSERT(newObj, "Failed to construct an object from hash");
        if (newObj)
        {
            if (!newObj->Deserialize(src, context))
            {
                delete newObj;
                return 0x0;
            }
            return newObj;
        }
        return 0x0;
    }

#ifndef SPRUE_NO_XML
    template<typename T>
    T* Deserialize(tinyxml2::XMLElement* element, const SerializationContext& context) const
    {
        static_assert(std::is_base_of<IEditable, T>::value, "T must be derived from IEditable to use Context::Deserialize");

        StringHash hash(element->Name());
        const unsigned version = element->UnsignedAttribute("version");
        T* newObj = Create<T>(hash);
        if (newObj)
        {
            if (!newObj->Deserialize(element, context))
            {
                delete newObj;
                return 0x0;
            }
            if (version != newObj->GetClassVersion())
                newObj->VersionUpdate(version);
            return newObj;
        }
        return 0x0;
    }
#endif

    /// Registers a resource loader with the context.
    bool RegisterResourceLoader(ResourceLoader* loader);
    /// Finds the resource loader associated with a specific resource type (hash of the mime type) and path, if possible.
    ResourceLoader* GetResourceLoader(const StringHash& resourceType, const char* path);

    void RegisterService(IContextService*);

    template<class T>
    T* GetService() const
    {
        for (auto a : attachedServices_)
            if (T* ret = dynamic_cast<T*>(a)) // Non-null check necessary because the first passing is returned
                return ret;
        return 0x0;
    }
#endif

    /// Retrieve the message log instance.
    MessageLog* GetLog() const { return log_; }

    /// Return the stored name of a StringHash.
    std::string GetHashName(const StringHash& hash);
    /// Return the registered type description for an object via it's type hash.
    std::string GetTypeDescription(const StringHash& hash);

    /// Intended for internal use only to access the utility functions for invoking callbacks.
    CallBacks& GetCallbacks() { return callbacks_; }

private:
    void RegisterProperties();

    CallBacks callbacks_;
    SprueModel* model_;
    MessageLog* log_;
    PropertyTable properties_;
    std::map<StringHash, FactoryMethod*> factories_;
    std::map<StringHash, std::string> hashNames_;
    std::map<StringHash, std::string> descriptions_;
    std::vector<ResourceLoader*> resourceLoaders_;
    std::vector<IContextService*> attachedServices_;
};

#define COPY_PROPERTIES(FROMTYPE, INTOTYPE) context->CopyBaseProperties(#FROMTYPE, #INTOTYPE)
#define REGISTER_PROPERTY(TYPENAME, PROPERTYTYPE, GETTER, SETTER, DEFVAL, NAME, DESC, FLAGS) context->Register(StringHash( #TYPENAME ), new TypePropertyImpl<TYPENAME, PROPERTYTYPE, PROPERTYTYPE>(&TYPENAME :: GETTER, &TYPENAME :: SETTER, DEFVAL, NAME, DESC, FLAGS))
#define REGISTER_PROPERTY_CONST_SET(TYPENAME, PROPERTYTYPE, GETTER, SETTER, DEFVAL, NAME, DESC, FLAGS) context->Register(StringHash( #TYPENAME ), new TypePropertyImplConstSet<TYPENAME, PROPERTYTYPE, PROPERTYTYPE>(&TYPENAME :: GETTER, &TYPENAME :: SETTER, DEFVAL, NAME, DESC, FLAGS))
#define REGISTER_PROPERTY_MEMORY(TYPENAME, PROPERTYTYPE, ADDR, DEFVAL, NAME, DESC, FLAGS) context->Register(StringHash( #TYPENAME ), new MemoryTypePropertyImpl<TYPENAME, PROPERTYTYPE, PROPERTYTYPE>(ADDR, DEFVAL, NAME, DESC, FLAGS))
#define REGISTER_ENUM(TYPENAME, PROPERTYTYPE, GETTER, SETTER, DEFVAL, NAME, DESC, FLAGS, NAMELIST) context->Register(StringHash( #TYPENAME ), new EnumPropertyImpl<TYPENAME, PROPERTYTYPE, PROPERTYTYPE>(&TYPENAME :: GETTER, &TYPENAME :: SETTER, DEFVAL, NAME, DESC, FLAGS, NAMELIST))
#define REGISTER_ENUM_MEMORY(TYPENAME, PROPERTYTYPE, ADDR, DEFVAL, NAME, DESC, FLAGS, NAMELIST) context->Register(StringHash( #TYPENAME ), new MemoryTypePropertyImpl<TYPENAME, PROPERTYTYPE, PROPERTYTYPE>(ADDR, DEFVAL, NAME, DESC, FLAGS, NAMELIST))
#define REGISTER_EDITABLE(TYPENAME, GETTER, SETTER, NAME, DESC, FLAGS, NAMELIST, HASHLIST) context->Register(StringHash(#TYPENAME), new IEditableProperty<TYPENAME>(&TYPENAME :: GETTER, &TYPENAME :: SETTER, NAME, DESC, FLAGS, NAMELIST, HASHLIST))
#define REGISTER_RESOURCE(TYPENAME, RESOURCETYPE, HGETTER, HSETTER, GETTER, SETTER, DEFVAL, NAME, DESC, FLAGS) context->Register(StringHash(#TYPENAME), new ResourcePropertyImpl<TYPENAME, RESOURCETYPE>(&TYPENAME :: HGETTER, &TYPENAME :: HSETTER, &TYPENAME :: GETTER, &TYPENAME :: SETTER, DEFVAL, NAME, DESC, FLAGS))
#define REGISTER_EDITABLE_LIST(TYPENAME, PROPERTYTYPE, GETTER, SETTER, NAME, DESC, FLAGS, NAMELIST, HASHLIST) context->Register(StringHash(#TYPENAME), new IEditableListProperty<TYPENAME, PROPERTYTYPE>(&TYPENAME :: GETTER, &TYPENAME :: SETTER, NAME, DESC, FLAGS, NAMELIST, HASHLIST))
#define REGISTER_LIST(TYPENAME, GETTER, SETTER, NAME, DESC, FLAGS, NAMELIST, HASHLIST) context->Register(StringHash(#TYPENAME), new IEditableProperty<TYPENAME, VariantVector>(&TYPENAME :: GETTER, &TYPENAME :: SETTER, NAME, DESC, FLAGS, NAMELIST, HASHLIST))
#define REGISTER_VECTOR_BUFFER(TYPENAME, ADDR, DEFVAL, NAME, DESC, FLAGS) context->Register(StringHash(#TYPENAME), new PointerBasedMemoryTypePropertyImpl<TYPENAME, VectorBuffer, VectorBuffer>(ADDR, DEFVAL, NAME, DESC, FLAGS))

}