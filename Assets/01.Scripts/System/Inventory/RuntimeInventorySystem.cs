using System;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeInventorySystem
{
    private int maxCapacity = 40;
    private int currentCapacity = 20;

    private readonly List<Artifact> artifacts = new();
    public IReadOnlyList<Artifact> Artifacts => artifacts;

    public int Capacity => currentCapacity;

    public event Action OnSlotChanged;

    private PlayerBlackBoard playerBoard;

    /// <summary>
    /// 사용 가능한 슬롯을 빈칸으로 초기화
    /// </summary>
    public RuntimeInventorySystem()
    {
        for (int i = 0; i < currentCapacity; i++)
            artifacts.Add(null);
    }

    public void SetPlayerBoard(PlayerBlackBoard board)
    {
        if (playerBoard == board)
            return;

        Debug.Log("\ud14c\uc2a4\ud2b8 \uc2dc\uc791");

        // 보드 초기화 전 아티펙트를 해제
        if (playerBoard != null)
        {
            foreach (Artifact artifact in artifacts)
                artifact?.OnUnequip(playerBoard);

            Debug.Log("테스트 1");
        }

        playerBoard = board;

        if (playerBoard != null)
        {
            foreach (Artifact artifact in artifacts)
                artifact?.OnEquip(playerBoard);

            Debug.Log("테스트 2");
        }
    }

    /// <summary>
    /// 빈 슬롯에 아티팩트 추가 및 효과 적용
    /// </summary>
    /// <param name="artifact"> 아티팩트 데이터 </param>
    /// <returns></returns>
    public bool TryAdd(Artifact artifact)
    {
        if (artifact == null || artifact.ArtifactItemData == null)
            return false;

        for (int i = 0; i < currentCapacity; i++)
        {
            if (artifacts[i] != null)
                continue;

            artifacts[i] = artifact;
            artifact.OnEquip(playerBoard);

            OnSlotChanged?.Invoke();

            return true;
        }        

        return false;
    }

    /// <summary>
    /// 지정한 슬롯 아티팩트 제거 및 반환
    /// </summary>
    /// <param name="index"> 슬롯 인덱스 </param>
    /// <returns></returns>
    public Artifact Remove(int index)
    {
        if (index < 0 || index >= artifacts.Count)
            return null;

        Artifact removedArtifact = artifacts[index];

        if (removedArtifact == null)
            return null;

        removedArtifact.OnUnequip(playerBoard);
        artifacts[index] = null;

        OnSlotChanged?.Invoke();

        return removedArtifact;
    }

    /// <summary>
    /// 지정한 슬롯의 유물을 새 유물로 교체 및 반환
    /// </summary>
    /// <param name="index"> 슬롯 인덱스 </param>
    /// <param name="replaceArtifact"> 교체할 유물 데이터 </param>
    /// <returns></returns>
    public Artifact Replace(int index, Artifact replaceArtifact)
    {
        if (replaceArtifact == null || replaceArtifact.ArtifactItemData == null || index < 0 || index >= artifacts.Count)
            return null;

        Artifact previousArtifact = artifacts[index];

        if (previousArtifact == null)
            return null;

        previousArtifact.OnUnequip(playerBoard);
        artifacts[index] = replaceArtifact;
        replaceArtifact.OnEquip(playerBoard);

        OnSlotChanged?.Invoke();

        return previousArtifact;
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

        if (newCapacity < currentCapacity)
        {
            for (int i = newCapacity; i < currentCapacity; i++)
            {
                if (artifacts[i] == null)
                    continue;

                Utils.LogWarning<RuntimeInventorySystem>("아이템이 있는 슬롯은 잠글 수 없음");
                return;
            }

            artifacts.RemoveRange(newCapacity, currentCapacity - newCapacity);
        }
        else
        {
            for (int i = currentCapacity; i < newCapacity; i++)
                artifacts.Add(null);
        }

        currentCapacity = newCapacity;
        OnSlotChanged?.Invoke();
    }

    /// <summary>
    /// 보유 유물 초기화
    /// </summary>
    public void Clear()
    {
        bool isChanged = false;

        for (int i = 0; i < artifacts.Count; i++)
        {
            Artifact artifact = Artifacts[i];

            if (artifact == null)
                continue;

            artifact.OnUnequip(playerBoard);
            artifacts[i] = null;
            isChanged = true;
        }

        if(isChanged)
            OnSlotChanged?.Invoke();
    }
}
