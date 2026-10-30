using System;
using System.Collections.Generic;
using UnityEngine;



public class TreasureRoomSystem : MonoBehaviour
{
    [Serializable]
    private class ChestPrefabEntry
    {
        public TreasureObject prefab;

        // 가중치
        [Min(1)]
        public int weight = 1;
    }

    [SerializeField] private RecoveryObject recoveryObj;
    [SerializeField] private Transform chestSpawnPoint;
    [SerializeField] private List<ChestPrefabEntry> chestPrefabs = new();

    [Header("item drop")]
    [SerializeField] private int pickupPoolId;

    private TreasureObject currentChest;
    private bool hasSelected;

    private void OnEnable()
    {
        EventBus.Subscribe<RecoveryObjectUsedEvent>(HandleRecoveryObjectUsed);
        EventBus.Subscribe<TreasureSelectedEvent>(HandleTreasureSelected);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<RecoveryObjectUsedEvent>(HandleRecoveryObjectUsed);
        EventBus.UnSubscribe<TreasureSelectedEvent>(HandleTreasureSelected);

    }

    public void Initialized()
    {
        recoveryObj.SetAvailable(true);
        hasSelected = false;

        TreasureObject selectedTreasure = GetRandomChestPrefab();

        if (selectedTreasure == null)
        {
            Utils.LogWarning<TreasureRoomSystem>("상자 프리팹을 찾지 못함");
            return;
        }

        currentChest = Instantiate(selectedTreasure, chestSpawnPoint.position, chestSpawnPoint.rotation, chestSpawnPoint);
    }

    /// <summary>
    /// 회복을 선택했을 시 발생
    /// </summary>
    /// <param name="eventData"></param>
    private void HandleRecoveryObjectUsed(RecoveryObjectUsedEvent eventData)
    {
        if (hasSelected || eventData.Obj != recoveryObj)
            return;

        hasSelected = true;

        if (currentChest != null)
            currentChest.SetAvailable(false);

        CompleteChoice();
    }

    /// <summary>
    /// 보물을 선택했을 시 발생
    /// </summary>
    /// <param name="eventData"></param>
    private void HandleTreasureSelected(TreasureSelectedEvent eventData)
    {
        if (hasSelected || eventData.Obj != currentChest || currentChest == null)
            return;

        if (!TryDropArtifact())
            return;

        hasSelected = true;

        recoveryObj.SetAvailable(false);
        currentChest.SetAvailable(false);

        currentChest.Open();

        CompleteChoice();
    }

    /// <summary>
    /// 현재 상자 기준 등급을 뽑고 상자 위치에 아티팩트 드롭
    /// </summary>
    /// <returns></returns>
    private bool TryDropArtifact()
    {
        TreasureChestData chestData = currentChest.ChestData;

        if (chestData == null)
        {
            Utils.LogWarning<TreasureRoomSystem>("상자 프리팹에 data 연결이 안됨");
            return false;
        }

        // SO에 설정된 가중치로 등급 뽑기
        if (!chestData.TryRollRarity(out ItemRarity rarity))
        {
            Utils.LogWarning<TreasureRoomSystem>("상자별 등급 가중치 재확인 필요");
            return false;
        }

        GameManager gameManager = GameManager.Instance;
        ObjectPoolManager poolManager = ObjectPoolManager.Instance;

        if (gameManager == null || poolManager == null)
            return false;

        // 뽑힌 등급에 해당하는 무작위 아티팩트 하나를 data로 반환
        if (!gameManager.TryGetRandomArtifactData(rarity, out ArtifactData data))
        {
            Utils.LogWarning<TreasureRoomSystem>($"{rarity} 등급 아티팩트가 존재하지 않음");
            return false;
        }

        // 선택된 data로 아이템 객체 생성
        Item item = gameManager.CreateItem(data.Id);

        // Artifact 타입 검사
        if (item is not Artifact artifact)
        {
            Utils.LogWarning<TreasureRoomSystem>($"아티팩트 생성 실패 : {data.Id}");
            return false;
        }

        // 공용 픽업 프리팹 가져오기
        ItemPickupPool pickup = poolManager.Get<ItemPickupPool>(pickupPoolId, currentChest.DropPos, null);

        if (pickup == null)
        {
            Utils.LogWarning<TreasureRoomSystem>($"픽업 풀 발견 실패 : {pickupPoolId}");
            return false;
        }

        // 픽업 프리팹에 아이템을 넣고 초기화
        pickup.SetItem(item);
        return true;
    }

    /// <summary>
    /// 가중치에 따라 상자 랜덤 선택
    /// </summary>
    /// <returns></returns>
    private TreasureObject GetRandomChestPrefab()
    {
        int totalWeight = 0;

        // 모든 상자의 합산 가중치 계산
        foreach (ChestPrefabEntry entry in chestPrefabs)
        {
            if (entry != null && entry.prefab != null && entry.weight > 0)
                totalWeight += entry.weight;
        }

        if (totalWeight <= 0)
            return null;

        // 보물방의 가중치 선택
        int roll = UnityEngine.Random.Range(0, totalWeight);

        // 보물방 가중치에 상자의 가중치를 차감하여 먼저 0보다 작아진 된 상자를 선택
        foreach (ChestPrefabEntry entry in chestPrefabs)
        {
            if (entry == null || entry.prefab == null || entry.weight <= 0)
                continue;

            roll -= entry.weight;

            if (roll < 0)
                return entry.prefab;
        }

        return null;
    }

    private void CompleteChoice()
    {
        EventBus.Publish(new TreasureRoomEndEvent(this));        
    }
}
