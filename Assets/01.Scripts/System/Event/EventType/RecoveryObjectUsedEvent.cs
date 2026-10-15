using UnityEngine;

public struct RecoveryObjectUsedEvent
{
    public RecoveryObject Obj { get; }

    public RecoveryObjectUsedEvent(RecoveryObject obj)
    {
        Obj = obj;
    }
}
