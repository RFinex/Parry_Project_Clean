using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class InventoryPanel : PopupBase
{
    [SerializeField] private List<SlotUI> slots;

    [Header("Description")]
    [SerializeField] private TextMeshProUGUI artifactNameText;
    [SerializeField] private TextMeshProUGUI artifactDescription;
    [SerializeField] private TextMeshProUGUI artifactRarity;
    [SerializeField] private Image artifactIcon;

    private RuntimeInventorySystem inventorySystem;

    private void Awake()
    {
        for (int i = 0; i < slots.Count; i++)
            slots[i].Initialized(this);

        ShowDescription(null);
    }

    protected override void Initialized()
    {
        base.Initialized();

        if (inventorySystem == null)
        {
            Utils.Log<InventoryPanel>("인벤토리 서비스 등록 실패");
            return;
        }

        inventorySystem.OnSlotChanged -= Refresh;
        inventorySystem.OnSlotChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (inventorySystem != null)
            inventorySystem.OnSlotChanged -= Refresh;
    }

    public void Refresh()
    {
        if (inventorySystem == null)
            return;

        for (int i = 0; i < slots.Count; i++)
        {
            if (i >= inventorySystem.Capacity)
                slots[i].SetLocked();
            else if (i >= inventorySystem.Artifacts.Count)
                slots[i].SetEmpty();
            else
                slots[i].SetArtifact(inventorySystem.Artifacts[i]);
        }

        ShowDescription(null);
    }

    public void ShowDescription(ArtifactData data)
    {
        if (artifactIcon != null)
        {
            artifactIcon.sprite = data != null ? data.Icon : null;
            artifactIcon.enabled = data != null && data.Icon != null;
        }

        if (data == null)
        {
            artifactNameText.text = "";
            artifactDescription.text = "";
            artifactRarity.text = "";
            return;
        }

        artifactNameText.text = data.ItemName;
        artifactDescription.text = data.Description;
        artifactRarity.text = data.Rarity.ToString();
        artifactIcon.sprite = data.Icon;
    }

    public void SetInventory(RuntimeInventorySystem inventory)
    {
        if (inventorySystem == inventory)
            return;

        if (inventorySystem != null)
            inventorySystem.OnSlotChanged -= Refresh;

        inventorySystem = inventory;
    }
}
