using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotUI : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private GameObject lockedSlot;

    private InventoryPanel inventory;
    private RelicData relic;

    public void Initialized(InventoryPanel inventory)
    {
        this.inventory = inventory;
    }

    public void SetLocked()
    {
        relic = null;

        icon.sprite = null;
        icon.enabled = false;

        lockedSlot.SetActive(true);
    }

    public void SetEmpty()
    {
        relic = null;

        icon.sprite = null;
        icon.enabled = false;

        lockedSlot.SetActive(false);
    }

    public void SetRelic(RelicData data)
    {
        relic = data;

        icon.sprite = data.Icon;
        icon.enabled = data.Icon != null;

        lockedSlot.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (inventory == null)
            return;

        inventory.ShowDescription(relic);
    }
}
