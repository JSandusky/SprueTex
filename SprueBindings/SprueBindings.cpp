// This is the main DLL file.

#include "stdafx.h"

#include "SprueBindings.h"

#include <SprueEngine/Logging.h>
#include <SprueEngine/MessageLog.h>
#include <SprueEngine/Core/Context.h>

using namespace System::Runtime::InteropServices;

namespace SprueBindings
{
    
    void System_LogcallBack(const char* msg, int level)
    {
        switch (level)
        {
        case 0:
            level = 2;
            break;
        case 2:
            level = 0;
            break;
        }
        System::GetPublsiher()->PublishError(::System::String::Format("SprueEngine: {0}", gcnew ::System::String(msg)), level);
    }

    void System::SetLogCallback(PluginLib::IErrorPublisher^ publisher)
    {
        if (publisher_ == nullptr)
        {
            publisher_ = publisher;
            auto log = SprueEngine::Context::GetInstance()->GetLog();
            log->SetLogCallback(System_LogcallBack);
        }
    }

    void System::Logcallback(const char* msg, int level)
    {

    }
}