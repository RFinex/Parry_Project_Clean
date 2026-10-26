using UnityEngine;

public struct PopupCountChangedEvent
{
    public int count;

    public PopupCountChangedEvent(int count)
    {
        this.count = count;
    }
}
