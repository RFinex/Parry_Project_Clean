using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TreasureChestData", menuName = "Data/Room/Treasure Chest")]
public class TreasureChestData : ScriptableObject
{
    [Serializable]
    public class RarityWeight
    {
        public ItemRarity rarity;

        [Min(0)]
        public int weight = 1;
    }

    [Header("최소 보장 등급")]
    [SerializeField] private ItemRarity minimumRarity;

    [Header("각 등급별 가중치")]
    [SerializeField] private List<RarityWeight> rarityWeights = new();

    /// <summary>
    /// 각 상자 가중치에 따른 드롭 아티팩트 등급 랜덤 반환
    /// </summary>
    /// <param name="result"> 반환된 랜덤 등급 </param>
    /// <returns></returns>
    public bool TryRollRarity(out ItemRarity result)
    {
        result = minimumRarity;

        // 전체 가중치 설정
        int totalWeight = 0;

        foreach (RarityWeight entry in rarityWeights)
        {
            if (entry != null && entry.weight > 0 && entry.rarity >= minimumRarity)
                totalWeight += entry.weight;
        }

        if (totalWeight <= 0)
            return false;

        int roll = UnityEngine.Random.Range(0, totalWeight);

        // 드롭될 등급 설정
        foreach (RarityWeight entry in rarityWeights)
        {
            // 최소 보장 등급보다 낮으면 체크하지 않음
            if (entry == null || entry.weight <= 0 || entry.rarity < minimumRarity)
                continue;

            roll -= entry.weight;

            if (roll < 0)
            {
                result = entry.rarity;
                return true;
            }
        }

        return false;
    }
}
