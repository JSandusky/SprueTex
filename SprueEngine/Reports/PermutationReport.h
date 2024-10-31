#pragma once

#include <SprueEngine/ClassDef.h>

#include <string>
#include <vector>

namespace SprueEngine
{
    class HTMLReport;
    class SprueModel;

    class SPRUE PermutationReport
    {
    public:
        PermutationReport(const std::vector<std::string>& files);
        PermutationReport(const std::string& file);
    };

}