using UnityEngine;

public struct TreasureRoomEndEvent
{
    public TreasureRoomSystem Obj { get; }

    public TreasureRoomEndEvent(TreasureRoomSystem obj)
    {
        this.Obj = obj;
    }
}
