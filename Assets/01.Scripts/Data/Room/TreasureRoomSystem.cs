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

        hasSelected = true;

        recoveryObj.SetAvailable(false);
        currentChest.SetAvailable(false);

        CompleteChoice();
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
