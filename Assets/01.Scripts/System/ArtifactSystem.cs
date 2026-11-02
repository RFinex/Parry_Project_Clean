using System.Collections.Generic;
using UnityEngine;
using static TreasureChestData;

public class ArtifactSystem : IService
{
    public bool IsDdol { get; set; } = true;

    private ArtifactDataReader dataReader;
    private HashSet<int> droppedArtifactIds = new();
    private ItemFactory itemFactory;

    private List<ArtifactData> artifactList = new();
    private List<RarityWeight> availableRarityWeights = new();

    public void Initialized(ArtifactDataReader reader, ItemFactory factory)
    {
        dataReader = reader;
        itemFactory = factory;
    }

    public Artifact CreateArtifact(int itemId)
    {
        if (itemFactory == null || dataReader == null)
            return null;

        Item item = itemFactory.Create(itemId);

        if (item == null)
            return null;

        if (!dataReader.TryGetData(itemId, out ArtifactData data))
            return null;

        item.Initialized(data);

        if (item is not Artifact artifact)
            return null;

        return artifact;
    }

    /// <summary>
    /// 각 상자 가중치에 따른 드롭 아티팩트 등급 랜덤 반환
    /// </summary>
    /// <param name="result"> 반환된 랜덤 등급 </param>
    /// <returns></returns>
    public bool TryRollRarity(TreasureChestData data, out ItemRarity result)
    {
        result = default;

        if (data == null || dataReader == null)
            return false;

        availableRarityWeights.Clear();

        AddAvailableArtifact(data, false);

        if (availableRarityWeights.Count == 0)
            AddAvailableArtifact(data, true);

        if (availableRarityWeights.Count == 0)
            return false;

        // 전체 가중치 설정
        int totalWeight = 0;
                
        foreach (RarityWeight entry in availableRarityWeights)
            totalWeight += entry.weight;

        if (totalWeight <= 0)
            return false;

        int roll = UnityEngine.Random.Range(0, totalWeight);

        // 드롭될 등급 설정
        foreach (RarityWeight entry in availableRarityWeights)
        {
            roll -= entry.weight;

            if (roll < 0)
            {
                result = entry.rarity;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 해당 레어리티에 남은 아티팩트가 있는지 체크
    /// </summary>
    /// <param name="rarity"> 체크할 레어리티 </param>
    /// <returns></returns>
    private bool HasAvailableArtifact(ItemRarity rarity)
    {
        foreach (ArtifactData artifact in dataReader.Artifacts)
        {
            if (artifact != null && artifact.Rarity == rarity && !droppedArtifactIds.Contains(artifact.Id))
                return true;
        }

        return false;
    }

    /// <summary>
    /// availableRarityWeights 채워넣기
    /// </summary>
    /// <param name="data"> RarityWeight를 가져올 data </param>
    /// <param name="belowMinimum"> true : 보장 등급까지만 추가 / false : 보장 등급 아래도 추가 </param>
    private void AddAvailableArtifact(TreasureChestData data, bool belowMinimum)
    {
        foreach (RarityWeight entry in data.RarityWeights)
        {
            if (entry == null || entry.weight <= 0)
                continue;

            bool isBelowMinimum = entry.rarity < data.MinimumRarity;

            if (isBelowMinimum != belowMinimum)
                continue;

            if (!HasAvailableArtifact(entry.rarity))
                continue;

            availableRarityWeights.Add(entry);
        }
    }

    public bool TryGetRandomArtifactData(ItemRarity rarity, out ArtifactData selectedData)
    {
        selectedData = null;

        if (dataReader == null)
            return false;

        artifactList.Clear();

        // 해당 등급의 드롭 가능한 아티팩트 갯수 측정
        //int availableCount = 0;

        foreach (ArtifactData data in dataReader.Artifacts)
        {
            if (data != null && data.Rarity == rarity && !droppedArtifactIds.Contains(data.Id))
                artifactList.Add(data);
        }

        // 후보가 없으면 실패
        //if (availableCount == 0)
        //    return false;

        if (artifactList.Count == 0)
            return false;

        // 후보 중 무작위 하나 선택
        int index = UnityEngine.Random.Range(0, artifactList.Count);

        selectedData = artifactList[index];
        
        return true;

        // 선택된 후보의 데이터를 찾아서 selectedData에 주입
        //foreach (ArtifactData data in dataReader.Artifacts)
        //{
        //    if (data == null || data.Rarity != rarity || remainArtifactIds.Contains(data.Id))
        //        continue;

        //    if (index == 0)
        //    {
        //        selectedData = data;
        //        return true;
        //    }

        //    index--;
        //}
    }

    public void MarkArtifactDropped(int id)
    {
        droppedArtifactIds.Add(id);
    }
}
