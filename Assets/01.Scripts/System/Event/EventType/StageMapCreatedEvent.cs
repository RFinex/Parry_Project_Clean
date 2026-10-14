using System.Collections.Generic;
using UnityEngine;

public struct StageMapCreatedEvent
{
    public List<StageMapNodeData> Nodes;
}

public struct StageMapShownEvent
{
    public List<int> SelectableNodeIds;
}

public struct StageMapClosedEvent 
{

}
