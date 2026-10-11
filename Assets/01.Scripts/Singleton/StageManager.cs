using System.Collections.Generic;
using UnityEngine;

public class StageManager : Singleton<StageManager>
{
    [SerializeField] private EnemySpawner spawner;

    public void StartRooom(Transform room)
    {
        if (spawner == null)
        {
            Utils.Log<StageManager>("EnemySpawner가 연결되지 않음");
            return;
        }

        List<EnemyController> enemies = spawner.Spawn(room);

        // 룸 매니저에 적 리스트와 스폰 실패 여부 전달
        RoomManager.Instance.StartRoom(enemies);
    }
}
