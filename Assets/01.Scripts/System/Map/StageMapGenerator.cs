using System;
using System.Collections.Generic;
using UnityEngine;

// ==========================================================
// 1. StageManager.Start()에서 RoomSystem Inject -> StageMapGenerator.Generator() 호출 -> 첫 노드를 StartNodeId에 지정 -> RoomSystem을 통해 방 프리팹과 아이콘 연결
// -> StageMapCreatedEvent 이벤트 발행해 지도UI에 노드 전달 -> 시작 노드 방 프리팹을 RoomSystem.StartRoom()으로 시작
// 2. StageMapGenerator.Generator() 에서 노드와 길 생성
// 3. UI 매니저에서 StageMapCreatedEvent 이벤트를 받아와 MapPanel.SetMap(nodes) 호출
// 4. 방 출구 상호작용 시 RoomExitInteractedEvent 이벤트 발생.
// 5. StageManager에서 현재 노드의 nextNodeIds를 가져와 지도에 표시 및 노드 활성화
// ==========================================================


[Serializable]
public class StageMapGenerator
{
    [Header("Floor Setting")]
    [SerializeField] private int floorCount = 8;
    [SerializeField] private int laneCount = 7;
    [SerializeField] private int pathCount = 5;
    [SerializeField] private int eliteFloor = 4;

    [Header("Map Spacing")]
    [SerializeField] private float horizontalSpacing = 190f;
    [SerializeField] private float verticalSpacing = 125f;

    [Header("Special Room Setting")]
    [SerializeField] private int treasureRoomCount = 2;
    [SerializeField] private int shopRoomCount = 2;

    /// <summary>
    /// 맵 생성 총괄 함수
    /// </summary>
    /// <returns></returns>
    public List<StageMapNodeData> Generate()
    {
        floorCount = Mathf.Max(3, floorCount);
        laneCount = Mathf.Max(3, laneCount);

        // 중앙 위치를 갖도록 레인 수는 홀수로 유지
        if (laneCount % 2 == 0)
            laneCount++;

        pathCount = Mathf.Max(1, pathCount);

        int lastFloor = floorCount - 1;
        int middleLane = laneCount / 2;
        int fixedEliteFloor = Mathf.Clamp(eliteFloor, 1, lastFloor - 1);

        Dictionary<Vector2Int, StageMapNodeData> nodeByCell = new Dictionary<Vector2Int, StageMapNodeData>();
        List<StageMapNodeData> nodes = new List<StageMapNodeData>();

        for (int pathIndex = 0; pathIndex < pathCount; pathIndex++)
        {
            List<Vector2Int> path = CreatePath(lastFloor, middleLane, fixedEliteFloor);

            StageMapNodeData previousNode = null;

            foreach (Vector2Int cell in path)
            {
                StageMapNodeData node = GetOrCreateNode(cell, lastFloor, middleLane, fixedEliteFloor, nodeByCell, nodes);

                if (previousNode != null &&
                    !previousNode.nextNodeIds.Contains(node.id))
                {
                    previousNode.nextNodeIds.Add(node.id);
                }

                previousNode = node;
            }
        }

        AssignSpecialRooms(nodes);

        return nodes;
    }

    /// <summary>
    /// 시작 - 보스 까지의 하나의 경로 생성
    /// </summary>
    /// <param name="lastFloor"> 마지막 층 </param>
    /// <param name="middleLane"> 중앙 레인 </param>
    /// <param name="fixedEliteFloor"> 엘리트 배치될 층 </param>
    /// <returns></returns>
    private List<Vector2Int> CreatePath(int lastFloor, int middleLane, int fixedEliteFloor)
    {
        List<Vector2Int> path = new List<Vector2Int>()
        {
            new Vector2Int(0, middleLane)
        };
        int currentLane = middleLane;

        for (int floor = 1; floor < lastFloor; floor++)
        {
            if (floor == fixedEliteFloor)
            {
                // 모든 경로가 엘리트 노드 하나로 합류
                currentLane = middleLane;
            }
            else
            {
                // 엘리트 층 전에는 엘리트 노드로 도달할 수 있게,
                // 이후에는 보스 노드로 도달할 수 있게 레인을 선택
                int targetFloor = floor < fixedEliteFloor ? fixedEliteFloor : lastFloor;

                currentLane = GetNextLane(currentLane, floor, targetFloor, middleLane);
            }

            path.Add(new Vector2Int(floor, currentLane));
        }

        path.Add(new Vector2Int(lastFloor, middleLane));

        return path;
    }

    /// <summary>
    /// 현재 레인에서 다음 층으로 갈 수 있는 레인을 반환
    /// </summary>
    /// <param name="currentLane"> 현재 레인 </param>
    /// <param name="currentFloor"> 현재 층 </param>
    /// <param name="targetFloor"> 목표 층 </param>
    /// <param name="targetLane"> 목표 레인 </param>
    /// <returns></returns>
    private int GetNextLane(int currentLane, int currentFloor, int targetFloor, int targetLane)
    {
        List<int> candidates = new List<int>();

        int minLane = Mathf.Max(0, currentLane - 1);
        int maxLane = Mathf.Min(laneCount - 1, currentLane + 1);
        int remainingMoves = targetFloor - currentFloor;

        for (int lane = minLane; lane <= maxLane; lane++)
        {
            // 남은 층 수 안에 목표 레인에 도달할 수 있는 선택지만 허용
            if (Mathf.Abs(targetLane - lane) <= remainingMoves)
                candidates.Add(lane);
        }

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    /// <summary>
    /// 해당 층, 레인 위치의 노드를 가져오거나 새로 생성
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="lastFloor"></param>
    /// <param name="middleLane"></param>
    /// <param name="fixedEliteFloor"></param>
    /// <param name="nodeByCell"></param>
    /// <param name="nodes"></param>
    /// <returns></returns>
    private StageMapNodeData GetOrCreateNode(Vector2Int cell, int lastFloor, int middleLane, int fixedEliteFloor, Dictionary<Vector2Int, StageMapNodeData> nodeByCell, List<StageMapNodeData> nodes)
    {
        // 같은 층/레인에 노드가 이미 존재하면 기존 노드를 반환해 경로 결합
        if (nodeByCell.TryGetValue(cell, out StageMapNodeData existingNode))
            return existingNode;

        // 마지막 층이면 Boss, 지정된 층이면 Elite, 나머지는 Normal로 지정
        RoomType roomType = RoomType.Normal;

        if (cell.x == lastFloor)
            roomType = RoomType.Boss;
        else if (cell.x == fixedEliteFloor && cell.y == middleLane)
            roomType = RoomType.Elite;

        // 노드 표시 위치 좌표 계산
        float x = (cell.x - (lastFloor * 0.5f)) * horizontalSpacing;
        float y = (cell.y - middleLane) * verticalSpacing;

        StageMapNodeData node = new StageMapNodeData
        {
            id = nodes.Count,
            roomType = roomType,
            mapPosition = new Vector2(x, y),
            floorIndex = cell.x
        };

        nodeByCell.Add(cell, node);
        nodes.Add(node);

        return node;
    }

    /// <summary>
    /// 특수방을 배정할 후보 수집
    /// </summary>
    /// <param name="nodes"></param>
    private void AssignSpecialRooms(List<StageMapNodeData> nodes)
    {
        List<StageMapNodeData> candidates = new List<StageMapNodeData>();

        foreach (StageMapNodeData node in nodes)
        {
            // 시작, 엘리트, 보스 노드는 특수 방 배정 대상에서 제외
            if (node.id != 0 && node.roomType == RoomType.Normal)
                candidates.Add(node);
        }

        int requiredCount = treasureRoomCount + shopRoomCount;
        bool hasEnoughCandidates = candidates.Count >= requiredCount;

        AssignRoomTypeOnUniqueFloors(candidates, RoomType.Treasure, treasureRoomCount);
        //AssignRoomType(candidates, RoomType.Treasure, treasureRoomCount);
        AssignRoomType(candidates, RoomType.Shop, shopRoomCount);

        if (!hasEnoughCandidates)
        {
            Utils.LogWarning<StageMapGenerator>("보물방 또는 상점방 개수를 모두 배치할 일반 노드가 부족합니다.");
        }
    }

    /// <summary>
    /// 후보 목록 중 무작위 노드를 골라 지정한 방으로 변경
    /// </summary>
    /// <param name="candidates"> 배치된 모든 Normal 타입 방 </param>
    /// <param name="roomType"> 변경할 방 타입 </param>
    /// <param name="count"> 반복 횟수 </param>
    private void AssignRoomType(List<StageMapNodeData> candidates, RoomType roomType, int count)
    {
        for (int i = 0; i < count && candidates.Count > 0; i++)
        {
            int index = UnityEngine.Random.Range(0, candidates.Count);
            candidates[index].roomType = roomType;
            candidates.RemoveAt(index);
        }
    }

    /// <summary>
    /// AssignRoomType과 비슷한 기능. 같은 층에는 같은 특수방을 배치하지 않는 조건을 추가
    /// </summary>
    /// <param name="candidates"> 배치된 모든 Normal 타입 방 </param>
    /// <param name="roomType"> 변경할 방 타입 </param>
    /// <param name="count"> 반복 횟수 </param>
    private void AssignRoomTypeOnUniqueFloors(List<StageMapNodeData> candidates, RoomType roomType, int count)
    {
        // 이미 배정된 층에는 다시 배치하지 않음
        HashSet<int> usedFloors = new();
        List<StageMapNodeData> floorCandidates = new();

        for (int i = 0; i < count && candidates.Count > 0; i++)
        {
            floorCandidates.Clear();

            foreach (StageMapNodeData candidate in candidates)
            {
                if (!usedFloors.Contains(candidate.floorIndex))
                    floorCandidates.Add(candidate);
            }

            if (floorCandidates.Count == 0)
                break;

            StageMapNodeData selected = floorCandidates[UnityEngine.Random.Range(0, floorCandidates.Count)];

            selected.roomType = roomType;
            usedFloors.Add(selected.floorIndex);
            candidates.Remove(selected);
        }
    }
}