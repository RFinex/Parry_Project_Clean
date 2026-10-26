using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class InventoryPanel : PopupBase
{
    [SerializeField] private List<SlotUI> slots;

    [Header("Description")]
    [SerializeField] private TextMeshProUGUI relicNameText;
    [SerializeField] private TextMeshProUGUI relicDescription;
    [SerializeField] private TextMeshProUGUI relicRarity;

    private RuntimeInvertorySystem inventorySystem;

    private void Awake()
    {
        for (int i = 0; i < slots.Count; i++)
            slots[i].Initialized(this);
    }

    protected override void Initialized()
    {
        base.Initialized();

        if (inventorySystem == null)
            ServiceLocator.Inject(out inventorySystem);

        if (inventorySystem == null)
        {
            Utils.Log<InventoryPanel>("인벤토리 서비스 등록 실패");
            return;
        }

        inventorySystem.OnChanged -= Refresh;
        inventorySystem.OnChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (inventorySystem != null)
            inventorySystem.OnChanged -= Refresh;
    }

    public void Refresh()
    {
        if (inventorySystem == null)
            return;

        for (int i = 0; i < slots.Count; i++)
        {
            if (i >= inventorySystem.Capacity)
                slots[i].SetLocked();
            else if (i >= inventorySystem.Relics.Count)
                slots[i].SetEmpty();
            else
                slots[i].SetRelic(inventorySystem.Relics[i]);
        }

        ShowDescription(null);
    }

    public void ShowDescription(RelicData data)
    {
        if (data == null)
        {
            relicNameText.text = "";
            relicDescription.text = "";
            return;
        }

        relicNameText.text = data.ItemName;
        relicDescription.text = data.Description;
        relicRarity.text = data.Rarity.ToString();
    }
}
