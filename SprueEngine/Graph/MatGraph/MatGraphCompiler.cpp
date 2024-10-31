#include "MatGraphCompiler.h"

#include <SprueEngine/FString.h>
#include <SprueEngine/Graph/Graph.h>
#include <SprueEngine/Graph/GraphSocket.h>

#include <set>
#include <unordered_map>
#include <vector>

namespace SprueEngine
{
    struct MatGraphPrecompileData
    {
        std::unordered_map<const GraphSocket*, std::string> socketNames_;

        /// Will assign unique names to each output socket
        void AssignSocketNames(std::set<std::string>& usedNames)
        {

        }
    };

    struct MatGraphNodeDepthRecord
    {
        const MatGraphNode* node_;
        unsigned depthLevel_;

        // we actually want a depth ordered sort
        bool operator<(const MatGraphNodeDepthRecord& rhs)
        {
            return depthLevel_ > rhs.depthLevel_;
        }
    };

    /// This visitor's job is to collect all nodes
    struct MatGraphCollectorVisitor : public GraphNodeVisitor
    {
        std::vector<MatGraphNodeDepthRecord>& records_;
        unsigned currentDepth_ = 0;

        MatGraphCollectorVisitor(std::vector<MatGraphNodeDepthRecord>& records) :
            records_(records)
        {

        }

        virtual void DepthPush() override { ++currentDepth_; }
        virtual void DepthPop() override { --currentDepth_; }

        virtual void Visit(const GraphNode* node) override {
            // Check if we already have a record for this, if not 
            auto found = std::find_if(records_.begin(), records_.end(),
                    [node](const MatGraphNodeDepthRecord& rec) {
                    return rec.node_ == node;
                });

            // Wasn't found so create a records
            if (found == records_.end())
                records_.push_back({ (MatGraphNode*)node, currentDepth_ });
            else // was found, make sure we use the deepest depth encountered
                found->depthLevel_ = std::max(found->depthLevel_, currentDepth_);
        }
    };

    /// Prepass visitor visits first
    struct MatGraphCompilerPrepassVisitor : public GraphNodeVisitor
    {
        virtual void Visit(const GraphNode* node) override {

        }
    };

    /// Visits the graph in upstream execution
    struct MatGraphCompilerCodeGenVisitor : public GraphNodeVisitor
    {
        virtual void Visit(const GraphNode* node) override {
            // Write code if necessary

            /* When evaluating sockets:
                if (connected socket was written)
                {
                    if (socket connected to constant node)
                    {
                        no nothing, write the node value directly
                    }
                    else if (socket connected to a node that created a variable for the socket)
                    {
                        write the variable in place of that socket
                    }
                    else if (socket connected connected to the only output in a node)
                    {
                        if (the node we're connected to wrote to a variable)
                        {
                            use that variable
                        }
                        else
                        {
                            write the node inline where the socket belongs
                        }
                    }
                }
                if socket is connected to a non-constant 
            */
        }
    };

    MatGraphCompiler::MatGraphCompiler(const MatGraphNode* node, MatGraphTargetLanguage lang) :
        node_(node),
        language_(lang)
    {

    }

    void MatGraphCompiler::Compile()
    {
        if (!node_)
            return; // TODO add error
        if (!node_->graph)
            return; // TODO add error

        // table of precompiled data, names and basic trait information for the compiler, acceleration structure to reduce redundant calls
        std::unordered_map<const MatGraphNode*, MatGraphPrecompileData> precompilerData;
        // depth sorted list of node records.
        std::vector<MatGraphNodeDepthRecord> depthSorted;

        // Collect upstream nodes
        MatGraphCollectorVisitor nodeCollector(depthSorted);
        const_cast<MatGraphNode*>(node_)->VisitUpstream(&nodeCollector);

        // Step 1: Verify that node is valid (ie. all required inputs are set)
        for (auto record : depthSorted)
        {
            
            if (!record.node_->NodeIsValid())
            {
                // TODO, cease compilation and return error
                return;
            }
        }

        // Step 2: Sort by node depth, sorting is done after validation as large graphs may take considerable time to sort
        std::sort(depthSorted.begin(), depthSorted.end());

        // Step 3: generate unique names as required
        for (auto record : depthSorted)
        {
            MatGraphPrecompileData data;
            // Check all output sockets
            for (auto outputSocket : record.node_->outputSockets)
            {
                // More than one connection, need to create a storage variable for it, generate a unique name for the storage variable
                if (outputSocket->GetConnections().size() > 1)
                    data.socketNames_[outputSocket] = FString("n_%1_s_%2", record.node_->id, outputSocket->name);
                else
                {
                    // Only one connection, this value will go inline so do nothing
                }
            }
            precompilerData[record.node_] = data;
        }

        // Step 4: generate header code
        std::set<StringHash> headerWrittenTypeIDs; // don't repeatedly write the same header data
        for (auto record : depthSorted)
        {
            if (headerWrittenTypeIDs.find(record.node_->GetTypeHash()) != headerWrittenTypeIDs.end())
                continue;

            // Write the header and then record the node type we wrote a header for
            record.node_->WriteHeader(language_);
            headerWrittenTypeIDs.insert(record.node_->GetTypeHash());
        }

        // Step 5: generate body code
        for (auto record : depthSorted)
        {
            if (record.node_->NeedsToWriteCode(language_))
            {
                record.node_->WriteCode(language_);
            }
        }
    }

}