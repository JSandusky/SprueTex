#include "ModelsReport.h"

#include <SprueEngine/FString.h>
#include <SprueEngine/Core/Bone.h>
#include <SprueEngine/Core/Component.h>
#include <SprueEngine/Reports/HTMLReport.h>
#include <SprueEngine/Core/SprueModel.h>
#include <SprueEngine/Core/SpruePieces.h>

namespace SprueEngine
{

    ModelsReport::ModelsReport(const std::vector<std::string>& files)
    {

    }

    ModelsReport::ModelsReport(const std::string& file)
    {

    }

    void ModelsReport::RecurseObjectTree(SceneObject* object, HTMLReport* report, bool printProperties)
    {
        std::string clazz = std::string();

        if (dynamic_cast<Component*>(object))
            clazz = "tree_component";
        else if (dynamic_cast<SpruePiece*>(object))
            clazz = "tree_piece";

        report->LI(clazz);
        report->Text(FString("%1 (%2)", object->GetName().empty() ? "<unnamed>" : object->GetName(), object->GetTypeName()).c_str());
        report->PopTag();

        for (auto child : object->GetChildren())
        {
            report->UL();
            RecurseObjectTree(object, report, printProperties);
            report->PopTag();
        }

    }

    void ModelsReport::MakeReport(const std::string& reportTitle, HTMLReport* report, SprueModel* model, bool detailed)
    {
        std::vector<SceneObject*> allObjects = model->GetFlatList();

        // Print the report title
        report->Header(1);
        report->Text(reportTitle);
        report->PopTag();

        report->Header(2);
        report->Text("Render");
        report->PopTag();

        report->Anchor("basic_info");
        report->Header(2);
        report->Text("Basic Information");
        report->PopTag();

        report->P();
        report->Bold("Number of Objects");
        report->Text(": ");
        report->Text(FString("%1", (unsigned)allObjects.size()).c_str());
        report->PopTag();

        report->P();
        report->Bold("Triangles");
        report->Text(": ");
        report->Text(FString("%1", (unsigned)allObjects.size()));
        report->PopTag();

        report->P();
        report->Bold("Vertices");
        report->Text(": ");
        report->Text(FString("%1", (unsigned)allObjects.size()));
        report->PopTag();

        if (!detailed)
            return;

        int boneCount = 0;
        int compCount = 0;
        int pieceCount = 0;

        for (auto item : allObjects)
        {
            if (dynamic_cast<Bone*>(item))
                ++boneCount;
            if (dynamic_cast<SpruePiece*>(item))
                ++pieceCount;
            if (dynamic_cast<Component*>(item))
                ++compCount;
        }

        // Print basic the "core" counts of object (bones, pieces, components)

        report->P();
        report->Bold("Number of Pieces");
        report->Text(": ");
        report->Text(FString("%1", (unsigned)pieceCount));
        report->PopTag();

        report->P();
        report->Bold("Number of Bones");
        report->Text(": ");
        report->Text(FString("%1", (unsigned)boneCount));
        report->PopTag();

        report->P();
        report->Bold("Number of Components");
        report->Text(": ");
        report->Text(FString("%1", (unsigned)compCount));
        report->PopTag();

        // Print the number of objects per type

        report->Anchor("details");
        report->Header(2);
        report->Text("Details");
        report->PopTag();

        std::map<std::string, int> typedCounts;
        for (auto item : allObjects)
        {
            auto found = typedCounts.find(item->GetTypeName());
            if (found != typedCounts.end())
                found->second += 1;
            else
                typedCounts.insert(std::make_pair(item->GetTypeName(), 1));
        }

        std::vector<std::pair<std::string, int> > sortedTypeCounts;
        for (auto item : typedCounts)
            sortedTypeCounts.push_back(item);

        std::sort(sortedTypeCounts.begin(), sortedTypeCounts.end(), [](std::pair<std::string, int>& lhs, std::pair<std::string, int>& rhs) { return lhs.second < rhs.second; });

        report->Table();
        report->Tr();
        report->Th(); report->Text("#"); report->PopTag();
        report->Th(); report->Text("Object Type"); report->PopTag();
        report->PopTag();
        for (auto it = sortedTypeCounts.rbegin(); it != sortedTypeCounts.rend(); ++it)
        {
            report->Tr();
            report->Td();
            report->Text(FString("%1", (unsigned)it->second));
            report->PopTag();
            report->Td();
            report->Text(it->first.c_str());
            report->PopTag();
            report->PopTag();
        }
        report->PopTag();

        // TODO print external references

        // Now print the entire tree off the model file
        report->Anchor("tree");
        report->Header(2);
        report->Text("Model Tree");
        report->PopTag();

        report->UL();
        RecurseObjectTree(model, report, false);
        report->PopTag();


    }


}