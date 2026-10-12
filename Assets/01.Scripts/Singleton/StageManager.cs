using System.Collections.Generic;
using UnityEngine;

public class StageManager : Singleton<StageManager>
{
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private PlayerController player;

    [SerializeField] private RoomType testRoomType = RoomType.Normal;

    private void Start()
    {
        StartRoom(testRoomType);
    }

    public void StartRoom(RoomType roomType)
    {
        RoomManager.Instance.StartRoom(roomType);
    }
}
