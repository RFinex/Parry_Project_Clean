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

    [Header("Item Drop")]
    [Tooltip("공용 ItemPickup 프리팹 등록 풀 ID")]
    [SerializeField] private int pickupPoolId = 1000000;

    [Tooltip("드랍 위치 오프셋")]
    [SerializeField] private Vector3 dropOffset = new(0f, 0.3f, 0f);

    private RuntimeInventorySystem inventorySystem;

    private void Awake()
    {
        for (int i = 0; i < slots.Count; i++)
            slots[i].Initialized(this, i);

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
            {
                Artifact artifact = inventorySystem.Artifacts[i];

                if (artifact == null || artifact.ArtifactItemData == null)
                    slots[i].SetEmpty();
                else
                    slots[i].SetArtifact(artifact.ArtifactItemData);

            }
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

        artifactNameText.text = ItemRarityColor.Apply(data.Rarity, data.ItemName);
        artifactDescription.text = data.Description;
        artifactRarity.text = ItemRarityColor.Apply(data.Rarity, data.Rarity.ToString());
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

    /// <summary>
    /// 아이템 버리기
    /// </summary>
    /// <param name="index"> 버릴 아이템 슬롯 인덱스 </param>
    public void DropArtifact(int index)
    {
        if (inventorySystem == null || index < 0 || index >= inventorySystem.Artifacts.Count)
            return;

        Artifact artifact = inventorySystem.Artifacts[index];

        if (artifact == null || artifact.ArtifactItemData == null)
            return;

        GameManager gameManager = GameManager.Instance;
        ObjectPoolManager poolManager = ObjectPoolManager.Instance;

        if (gameManager == null || gameManager.Player == null || poolManager == null)
            return;

        Vector3 dropPos = gameManager.Player.transform.position + dropOffset;

        ItemPickupPool pickup = poolManager.Get<ItemPickupPool>(pickupPoolId, dropPos, null);

        if (pickup == null)
            return;

        pickup.SetItem(artifact);

        Artifact removedArtifact = inventorySystem.Remove(index);

        if (removedArtifact == null)
        {
            pickup.ReturnPool();
            return;
        }

        Utils.Log<InventoryPanel>($"아이템 버리기 : {removedArtifact.Data.ItemName}");
    }
}
