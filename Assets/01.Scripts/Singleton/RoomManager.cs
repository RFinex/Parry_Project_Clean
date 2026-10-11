using UnityEngine;
using System.Collections.Generic;
using System;

public class RoomManager : Singleton<RoomManager>
{
    // 스폰된 적 리스트
    private readonly List<EnemyController> activeEnemies = new List<EnemyController>();

    public event Action OnRoomClear;

    public bool IsCleared {  get; private set; }
    public int ActiveEnemyCount => activeEnemies.Count;

    public void StartRoom(List<EnemyController> enemies)
    {
        EndRoom();

        IsCleared = false;

        // 적 리스트를 활성화 적 리스트에 등록
        foreach (EnemyController enemy in enemies)
        {
            if (enemy == null || activeEnemies.Contains(enemy))
                continue;

            activeEnemies.Add(enemy);
            enemy.OnDeathFinished += OnEnemyDeathFinished;
        }

        if (activeEnemies.Count == 0)
            ClearRoom();
    }

    public void EndRoom()
    {
        foreach (EnemyController enemy in activeEnemies)
        {
            if (enemy == null)
                continue;

            enemy.OnDeathFinished -= OnEnemyDeathFinished;
            enemy.ReturnPool();
        }

        activeEnemies.Clear();
    }

    /// <summary>
    /// 적 사망 시 실행
    /// </summary>
    /// <param name="enemy"> 활성화 적 </param>
    private void OnEnemyDeathFinished(EnemyController enemy)
    {
        enemy.OnDeathFinished -= OnEnemyDeathFinished;
        activeEnemies.Remove(enemy);

        Utils.Log<EnemySpawner>($"남은 적 수 : {activeEnemies.Count}");

        if (activeEnemies.Count == 0)
            ClearRoom();
    }

    private void ClearRoom()
    {
        if (IsCleared)
            return;

        IsCleared = true;
        Utils.Log<RoomManager>("방 클리어");

        OnRoomClear?.Invoke();
    }

    protected override void OnDestroy()
    {
        EndRoom();
        base.OnDestroy();
    }
}
