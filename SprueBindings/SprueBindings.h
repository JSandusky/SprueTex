// SprueBindings.h

#pragma once

using namespace System;

namespace SprueBindings {

    public ref class System
    {
    public:
        static void SetLogCallback(PluginLib::IErrorPublisher^ publisher);

        static PluginLib::IErrorPublisher^ GetPublsiher() { return publisher_; }

    private:
        static void Logcallback(const char* msg, int level);

        static PluginLib::IErrorPublisher^ publisher_ = nullptr;
    };
	
}
