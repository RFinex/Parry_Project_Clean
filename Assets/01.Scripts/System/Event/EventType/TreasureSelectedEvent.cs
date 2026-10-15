using UnityEngine;

public struct TreasureSelectedEvent
{
    public TreasureObject Obj { get; }

    public TreasureSelectedEvent(TreasureObject obj)
    {
        Obj = obj;
    }
}
