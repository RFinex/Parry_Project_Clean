using System;
using System.Collections.Generic;
using UnityEngine;



public class TreasureRoomSystem : MonoBehaviour
{
    [Serializable]
    private class ChestType
    {
        public Sprite closedSprite;

        [Header("Animation Name")]
        public string openAnimName;

        // 가중치
        [Min(1)] public int weight = 1;

        // 아이템 최소 등급
        [Min(0)] public int minItemGrade;
    }

    [SerializeField] private RecoveryObject recoveryObj;
    [SerializeField] private TreasureObject treasureObj;
    [SerializeField] private List<ChestType> chestTypes;

    private ChestType currentChestType;
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
        treasureObj.SetAvailable(true);

        currentChestType = GetRandomChestType();
        hasSelected = false;

        if (currentChestType == null)
        {
            Utils.LogError<TreasureRoomSystem>("사용 가능한 상자 종류가 없음");
            treasureObj.SetAvailable(false);
            return;
        }

        treasureObj.SetupChest(currentChestType.closedSprite, currentChestType.openAnimName);
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
        treasureObj.SetAvailable(false);

        CompleteChoice();
    }

    /// <summary>
    /// 보물을 선택했을 시 발생
    /// </summary>
    /// <param name="eventData"></param>
    private void HandleTreasureSelected(TreasureSelectedEvent eventData)
    {
        if (hasSelected || eventData.Obj != treasureObj)
            return;

        if (currentChestType == null)
            return;

        hasSelected = true;

        recoveryObj.SetAvailable(false);
        treasureObj.SetAvailable(false);

        treasureObj.Open();

        // 아이템, 골드 드랍

        CompleteChoice();
    }

    private void CompleteChoice()
    {
        EventBus.Publish(new TreasureRoomEndEvent(this));        
    }

    /// <summary>
    /// 가중치에 따라 상자 랜덤 선택
    /// </summary>
    /// <returns></returns>
    private ChestType GetRandomChestType()
    {
        int totalWeight = 0;

        // 모든 상자의 합산 가중치 계산
        foreach (ChestType chestType in chestTypes)
        {
            if (chestType != null && chestType.weight > 0)
                totalWeight += chestType.weight;
        }

        if (totalWeight == 0)
            return null;

        // 보물방의 가중치 선택
        int randomWight = UnityEngine.Random.Range(0, totalWeight);

        // 보물방 가중치에 상자의 가중치를 차감하여 먼저 0 이하가 된 상자를 선택
        foreach (ChestType chestType in chestTypes)
        {
            if (chestType == null || chestType.weight <= 0)
                continue;

            randomWight -= chestType.weight;

            if (randomWight < 0)
                return chestType;
        }

        return null;
    }
}
