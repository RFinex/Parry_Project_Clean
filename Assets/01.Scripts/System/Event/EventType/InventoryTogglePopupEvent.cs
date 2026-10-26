using UnityEngine;

public struct InventoryTogglePopupEvent
{
    public RuntimeInventorySystem inventory;

    public InventoryTogglePopupEvent(RuntimeInventorySystem inventory)
    {
        this.inventory = inventory;
    }
}
