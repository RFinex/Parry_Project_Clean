using UnityEngine;

public class ItemPickup : InteractableBase
{
    [SerializeField] private SpriteRenderer itemRenderer;
    [SerializeField] private ItemPickupPool poolObject;

    private Item item;

    public override bool CanInteract => item is Artifact artifact && artifact.ArtifactData != null;

    /// <summary>
    /// 아이템 스프라이트를 데이터의 스프라이트로 교체
    /// </summary>
    /// <param name="item"> 스프라이트 교체할 아이템 </param>
    public void SetItem(Item item)
    {
        this.item = item;

        if (itemRenderer != null)
        {
            itemRenderer.sprite = item != null && item.Data != null ? item.Data.Icon : null;
        }

        RefreshIcon();
    }

    public override void Interact(PlayerController interactor)
    {
        if (interactor == null || !CanInteract || poolObject == null)
            return;

        GameManager manager = GameManager.Instance;

        if (manager == null || manager.Inventory == null)
            return;

        Artifact artifact = item as Artifact;

        if (!manager.Inventory.TryAdd(artifact))
        {
            Utils.Log<ItemPickup>("인벤토리 공간 부족");
            return;
        }

        poolObject.ReturnPool();
    }

    public void ClearItem()
    {
        item = null;

        if (itemRenderer != null)
            itemRenderer.sprite = null;

        SetTrigger(false);
        RefreshIcon();
    }
}
