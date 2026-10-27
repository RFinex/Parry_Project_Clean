using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private GameObject lockedSlot;

    private InventoryPanel inventory;
    private ArtifactData artifact;

    public void Initialized(InventoryPanel inventory)
    {
        this.inventory = inventory;
    }

    public void SetLocked()
    {
        artifact = null;

        icon.sprite = null;
        icon.enabled = false;

        lockedSlot.SetActive(true);
    }

    public void SetEmpty()
    {
        artifact = null;

        icon.sprite = null;
        icon.enabled = false;

        lockedSlot.SetActive(false);
    }

    public void SetArtifact(ArtifactData data)
    {
        artifact = data;

        icon.sprite = data.Icon;
        icon.enabled = data.Icon != null;

        lockedSlot.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (inventory == null)
            return;

        inventory.ShowDescription(artifact);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (inventory == null)
            return;

        inventory.ShowDescription(null);
    }
}
