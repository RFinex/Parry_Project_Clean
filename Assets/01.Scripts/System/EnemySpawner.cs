using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    private readonly List<EnemyController> activeEnemies = new List<EnemyController>();

    // 스폰 시도 체크
    private bool hasSpawned;

    private bool isSpawnFailed;

    public event Action OnRoomClear;

    public bool IsCleared { get; private set; }
    public int ActiveEnemyCount => activeEnemies.Count;

    private void Start()
    {
        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        if (hasSpawned)
            return;

        hasSpawned = true;

        EnemySpawnPoint[] spawnPoints = GetComponentsInChildren<EnemySpawnPoint>();

        foreach (EnemySpawnPoint spawnPoint in spawnPoints)
        {
            EnemyController enemy = ObjectPoolManager.Instance.Get<EnemyController>(spawnPoint.EnemyId, spawnPoint.transform.position);

            if (enemy == null)
            {
                isSpawnFailed = true;
                Utils.LogError<EnemySpawner>($"적 {spawnPoint.EnemyId} 를 풀에서 가져오지 못했습니다.");
                continue;
            }

            activeEnemies.Add(enemy);
            enemy.OnDeathFinished += OnEnemyDeathFinished;
        }

        if (spawnPoints.Length == 0)
            ClearRoom();
    }

    private void OnEnemyDeathFinished(EnemyController enemy)
    {
        enemy.OnDeathFinished -= OnEnemyDeathFinished;
        activeEnemies.Remove(enemy);

        Utils.Log<EnemySpawner>($"남은 적 수 : {activeEnemies.Count}");

        if (activeEnemies.Count == 0 && !isSpawnFailed)
            ClearRoom();        
    }

    private void ClearRoom()
    {
        if (IsCleared)
            return;

        Utils.Log<EnemySpawner>("방 클리어");

        IsCleared = true;
        OnRoomClear?.Invoke();
    }

    private void OnDestroy()
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
}
