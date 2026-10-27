using UnityEngine;

public class ItemPickupPool : PoolableGameObject
{
    [SerializeField] private ItemPickup pickup;
    
    public void SetItem(Item item)
    {
        if (pickup != null)
            pickup.SetItem(item);
    }

    public override void InitPool()
    {
        base.InitPool();

        if (pickup != null)
            pickup.ClearItem();
    }

    public override void InitPoolReturn()
    {
        if (pickup != null)
            pickup.ClearItem();

        base.InitPoolReturn();
    }
}
