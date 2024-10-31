#include "MatGraphBaseNodes.h"

#include <SprueEngine/Graph/GraphSocket.h>

namespace SprueEngine
{

    bool MatGraphNode::NodeIsValid() const
    {
        // If any required input sockets do not have connections then the node is invalid.
        for (auto inputSocket : inputSockets)
        {
            if (inputSocket->flags & MGF_Required)
            {
                if (!inputSocket->HasConnections())
                    return false;
            }
        }
        return true;
    }

}