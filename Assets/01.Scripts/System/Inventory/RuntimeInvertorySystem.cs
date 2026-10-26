using System;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeInvertorySystem : MonoBehaviour, IService
{
    [SerializeField] private int maxCapacity = 20;
    [SerializeField] private int currentCapacity = 10;

    private readonly List<RelicData> relics = new();
    public IReadOnlyList<RelicData> Relics => relics;

    public bool IsDdol { get; set; } = true;
    public int Capacity => currentCapacity;

    public event Action OnChanged;

    private void Awake()
    {
        if (maxCapacity < 1)
            maxCapacity = 1;

        if (currentCapacity < 1)
            currentCapacity = 1;

        if(currentCapacity > maxCapacity)
            currentCapacity = maxCapacity;

        DontDestroyOnLoad(gameObject);
        ServiceLocator.Register<RuntimeInvertorySystem>(this);
    }

    /// <summary>
    /// 빈 슬롯에 유물 추가
    /// </summary>
    /// <param name="data"> 유물 데이터 </param>
    /// <returns></returns>
    public bool TryAdd(RelicData data)
    {
        if (data == null || relics.Count >= currentCapacity)
            return false;

        relics.Add(data);
        OnChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// 지정한 슬롯 유물 제거 및 반환
    /// </summary>
    /// <param name="index"> 슬롯 인덱스 </param>
    /// <returns></returns>
    public RelicData Remove(int index)
    {
        if (index < 0 || index >= relics.Count)
            return null;

        RelicData removeData = relics[index];
        relics.RemoveAt(index);
        OnChanged?.Invoke();

        return removeData;
    }

    /// <summary>
    /// 지정한 슬롯의 유물을 새 유물로 교체 및 반환
    /// </summary>
    /// <param name="index"> 슬롯 인덱스 </param>
    /// <param name="replaceData"> 교체할 유물 데이터 </param>
    /// <returns></returns>
    public RelicData Replace(int index, RelicData replaceData)
    {
        if (replaceData == null || index < 0 || index >= relics.Count)
            return null;

        RelicData previousData = relics[index];
        relics[index] = replaceData;
        OnChanged?.Invoke();

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
        OnChanged?.Invoke();
    }

    /// <summary>
    /// 보유 유물 초기화
    /// </summary>
    public void Clear()
    {
        if (relics.Count == 0)
            return;

        relics.Clear();
        OnChanged?.Invoke();
    }
}
