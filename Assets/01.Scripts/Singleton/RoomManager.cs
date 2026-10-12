using UnityEngine;
using System.Collections.Generic;
using System;

public class RoomManager : Singleton<RoomManager>
{
    [SerializeField] private List<RoomPrefabGroup> roomPrefabGroups;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private PlayerController player;

    private RoomFactory roomFactory;

    private GameObject currentRoom;
    // 스폰된 적 리스트
    private readonly List<EnemyController> activeEnemies = new List<EnemyController>();

    public event Action OnRoomClear;

    public bool IsCleared { get; private set; }
    public int ActiveEnemyCount => activeEnemies.Count;

    private CameraTargetBinder cameraTargetBinder;

    protected override void Awake()
    {
        base.Awake();

        roomFactory = new RoomFactory();

        if (roomPrefabGroups == null)
        {
            Utils.Log<RoomManager>("RoomPrefabGroup 목록이 설정되지 않음");
            return;
        }

        foreach (RoomPrefabGroup group in roomPrefabGroups)
        {
            roomFactory.Register(group);
        }
    }

    public void RegisterCameraBinder(CameraTargetBinder binder)
    {
        if (binder == null)
            return;

        cameraTargetBinder = binder;

        if (currentRoom != null)
            ApplyCameraBounds();
    }

    public void UnRegisterCameraBinder(CameraTargetBinder binder)
    {
        if (cameraTargetBinder == binder)
            cameraTargetBinder = null;
    }

    private void ApplyCameraBounds()
    {
        if (cameraTargetBinder == null || currentRoom == null)
            return;

        PolygonCollider2D bounds = currentRoom.GetComponentInChildren<PolygonCollider2D>(true);

        if (bounds == null)
        {
            Utils.Log<RoomManager>($"{currentRoom.name}에 카메라 경계 PolygonCollider2D가 없음");
            return;
        }

        if (!cameraTargetBinder.SetBounds(bounds))
        {
            Utils.Log<RoomManager>("Virtual Camera에 CinemachineConfiner2D가 없음");
        }
    }

    public void StartRoom(GameObject roomPrefab)
    {
        if (player == null || enemySpawner == null)
        {
            Utils.LogError<RoomManager>("Player 또는 EnemySpawner가 연결되지 않음");
            return;
        }

        if (roomPrefab == null)
        {
            Utils.LogError<RoomManager>("생성할 방 프리팹이 저장되지 않음");
            return;
        }

        // 방 초기화
        EndRoom();
        IsCleared = false;

        // 방 생성
        currentRoom = roomFactory.Create(roomPrefab);

        if (currentRoom == null)
        {
            Utils.Log<RoomManager>("방 생성 실패");
            return;
        }

        ApplyCameraBounds();

        // 플레이어를 시작 위치로 이동
        PlayerSpawnPoint spawnPoint = currentRoom.GetComponentInChildren<PlayerSpawnPoint>(true);

        if (spawnPoint == null)
        {
            Utils.Log<RoomManager>($"{currentRoom.name}에 PlayerSpawnPoint가 존재하지 않음");
            return;
        }

        player.MoveToRoomStart(spawnPoint.transform.position);

        // 적을 스폰 위치에 소환
        List<EnemyController> enemies = enemySpawner.Spawn(currentRoom.transform);

        foreach (EnemyController enemy in enemies)
        {
            if (enemy == null || activeEnemies.Contains(enemy))
                continue;

            activeEnemies.Add(enemy);
            enemy.OnDeathFinished += OnEnemyDeathFinished;
        }

        // 적이 소환되지 않았으면 클리어 처리
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

        if (currentRoom != null)
        {
            Destroy(currentRoom);
            currentRoom = null;
        }
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

    public GameObject GetRandomRoomPrefab(RoomType roomType)
    {
        return roomFactory.GetRandomPrefab(roomType);
    }

    protected override void OnDestroy()
    {
        EndRoom();
        base.OnDestroy();
    }
}
