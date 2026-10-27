using System;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeInventorySystem
{
    private int maxCapacity = 40;
    private int currentCapacity = 20;

    private readonly List<ArtifactData> artifacts = new();
    public IReadOnlyList<ArtifactData> Artifacts => artifacts;

    public int Capacity => currentCapacity;

    public event Action OnSlotChanged;

    /// <summary>
    /// 빈 슬롯에 유물 추가
    /// </summary>
    /// <param name="data"> 유물 데이터 </param>
    /// <returns></returns>
    public bool TryAdd(ArtifactData data)
    {
        if (data == null || artifacts.Count >= currentCapacity)
            return false;

        artifacts.Add(data);
        OnSlotChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// 지정한 슬롯 유물 제거 및 반환
    /// </summary>
    /// <param name="index"> 슬롯 인덱스 </param>
    /// <returns></returns>
    public ArtifactData Remove(int index)
    {
        if (index < 0 || index >= artifacts.Count)
            return null;

        ArtifactData removeData = artifacts[index];
        artifacts.RemoveAt(index);
        OnSlotChanged?.Invoke();

        return removeData;
    }

    /// <summary>
    /// 지정한 슬롯의 유물을 새 유물로 교체 및 반환
    /// </summary>
    /// <param name="index"> 슬롯 인덱스 </param>
    /// <param name="replaceData"> 교체할 유물 데이터 </param>
    /// <returns></returns>
    public ArtifactData Replace(int index, ArtifactData replaceData)
    {
        if (replaceData == null || index < 0 || index >= artifacts.Count)
            return null;

        ArtifactData previousData = artifacts[index];
        artifacts[index] = replaceData;
        OnSlotChanged?.Invoke();

        return previousData;
    }

    /// <summary>
    /// 슬롯 용량 변경
    /// </summary>
    /// <param name="newCapacity"> 변경할 용량 </param>
    public void SetCapacity(int newCapacity)
    {
        if (newCapacity <= 0)
            newCapacity = 1;
        else if(newCapacity > maxCapacity)
            newCapacity = maxCapacity;

        if (currentCapacity == newCapacity)
            return;

        currentCapacity = newCapacity;
        OnSlotChanged?.Invoke();
    }

    /// <summary>
    /// 보유 유물 초기화
    /// </summary>
    public void Clear()
    {
        if (artifacts.Count == 0)
            return;

        artifacts.Clear();
        OnSlotChanged?.Invoke();
    }
}
