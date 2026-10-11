using System.Collections.Generic;
using UnityEngine;

public class StageManager : Singleton<StageManager>
{
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private PlayerController player;

    public void StartRooom(Transform room)
    {
        if (room == null)
        {
            Utils.Log<StageManager>("존재하지 않는 방입니다.");
            return;
        }

        if (spawner == null || player == null)
        {
            Utils.Log<StageManager>("EnemySpawner or PlayerController가 연결되지 않음");
            return;
        }

        PlayerSpawnPoint startPoint = room.GetComponentInChildren<PlayerSpawnPoint>();

        if (startPoint == null)
        {
            Utils.Log<StageManager>("PlayerSpawnPoint를 찾지 못했습니다.");
            return;
        }

        player.MoveToRoomStart(startPoint.transform.position);

        List<EnemyController> enemies = spawner.Spawn(room);

        // 룸 매니저에 적 리스트와 스폰 실패 여부 전달
        RoomManager.Instance.StartRoom(enemies);
    }
}
