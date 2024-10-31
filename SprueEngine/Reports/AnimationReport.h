#pragma once

#include <SprueEngine/ClassDef.h>

#include <string>
#include <vector>

namespace SprueEngine
{
    class HTMLReport;
    class SprueModel;

    class SPRUE AnimationReport
    {
    public:
        AnimationReport(const std::vector<std::string>& modelFiles, const std::vector<std::string>& animationFiles);
        AnimationReport(const std::string& modelFile, const std::vector<std::string>& animationFiles);

    protected:
    };

}