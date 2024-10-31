#pragma once

#include <SprueEngine/ClassDef.h>

#include <string>
#include <vector>

namespace SprueEngine
{
    class HTMLReport;
    class SceneObject;
    class SprueModel;

    class SPRUE ModelsReport
    {
    public:
        ModelsReport(const std::string& file);
        ModelsReport(const std::vector<std::string>& files);

    private:
        void MakeReport(const std::string& reportTitle, HTMLReport* report, SprueModel* model, bool detailed);
        void RecurseObjectTree(SceneObject* object, HTMLReport* report, bool printProperties);
    };

}