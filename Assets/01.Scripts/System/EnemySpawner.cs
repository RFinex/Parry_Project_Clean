using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform statusBarParent;

    /// <summary>
    /// 적 스폰
    /// </summary>
    /// <param name="room"> 방 정보 </param>
    /// <param name="spawnFailed"> 적 스폰 실패 여부 반환 </param>
    /// <returns></returns>
    public List<EnemyController> Spawn(Transform room)
    {
        List<EnemyController> spawnEnemies = new List<EnemyController>();

        if (room == null)
        {
            Utils.LogError<EnemySpawner>("방이 지정되지 않았습니다.");
            return spawnEnemies;
        }

        // 방에 존재하는 적 스폰 포인트 확인
        EnemySpawnPoint[] spawnPoints = room.GetComponentsInChildren<EnemySpawnPoint>();

        foreach (EnemySpawnPoint spawnPoint in spawnPoints)
        {
            EnemyController enemy = ObjectPoolManager.Instance.Get<EnemyController>(spawnPoint.EnemyId, spawnPoint.transform.position);

            if (enemy == null)
            {
                Utils.LogError<EnemySpawner>($"{spawnPoint.EnemyId} 적을 풀에서 가져오지 못했습니다.");
                continue;
            }

            EnemyStatusBar statusBar = UIManager.Instance.GetUI<EnemyStatusBar>(UIName.Enemy_Status_Bar, enemy.transform.position, statusBarParent);

            if (statusBar != null)
            {
                enemy.SetStatusBar(statusBar);
                statusBar.Bind(enemy);
            }

            spawnEnemies.Add(enemy);
        }

        return spawnEnemies;
    }    
}
