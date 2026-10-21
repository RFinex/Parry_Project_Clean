using System.Collections.Generic;
using UnityEngine;

public struct StageMapCreatedEvent
{
    public List<StageMapNodeData> nodes;

    public StageMapCreatedEvent(List<StageMapNodeData> nodes)
    {
        this.nodes = nodes;
    }
}

public struct StageMapShownEvent
{
    public List<int> selectableNodeIds;

    public StageMapShownEvent(List<int> selectableNodeIds)
    {
        this.selectableNodeIds = selectableNodeIds;
    }
}

public struct StageMapClosedEvent 
{

}
