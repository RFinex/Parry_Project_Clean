using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StageMapGenerator
{
    [SerializeField] private int floorCount = 8;
    [SerializeField] private int laneCount = 7;
    [SerializeField] private int pathCount = 5;
    [SerializeField] private int eliteFloor = 4;

    [SerializeField] private float horizontalSpacing = 190f;
    [SerializeField] private float verticalSpacing = 125f;

    [SerializeField] private int treasureRoomCount = 2;
    [SerializeField] private int shopRoomCount = 2;

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
            // 첫 경로는 엘리트 노드를 반드시 통과시켜 엘리트 노드가 고립되지 않게 함
            bool passElite = pathIndex == 0;

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

    private StageMapNodeData GetOrCreateNode(
        Vector2Int cell,
        int lastFloor,
        int middleLane,
        int fixedEliteFloor,
        Dictionary<Vector2Int, StageMapNodeData> nodeByCell,
        List<StageMapNodeData> nodes)
    {
        if (nodeByCell.TryGetValue(cell, out StageMapNodeData existingNode))
            return existingNode;

        RoomType roomType = RoomType.Normal;

        if (cell.x == lastFloor)
            roomType = RoomType.Boss;
        else if (cell.x == fixedEliteFloor && cell.y == middleLane)
            roomType = RoomType.Elite;

        float x = (cell.x - (lastFloor * 0.5f)) * horizontalSpacing;
        float y = (cell.y - middleLane) * verticalSpacing;

        StageMapNodeData node = new StageMapNodeData
        {
            id = nodes.Count,
            roomType = roomType,
            mapPosition = new Vector2(x, y)
        };

        nodeByCell.Add(cell, node);
        nodes.Add(node);

        return node;
    }

    private void AssignSpecialRooms(List<StageMapNodeData> nodes)
    {
        List<StageMapNodeData> candidates = new List<StageMapNodeData>();

        foreach (StageMapNodeData node in nodes)
        {
            // 시작 노드와 엘리트·보스 노드는 제외
            if (node.id != 0 && node.roomType == RoomType.Normal)
                candidates.Add(node);
        }

        AssignRoomType(candidates, RoomType.Treasure, treasureRoomCount);
        AssignRoomType(candidates, RoomType.Shop, shopRoomCount);

        if (candidates.Count < treasureRoomCount + shopRoomCount)
        {
            Utils.LogWarning<StageMapGenerator>("생성된 일반 노드가 부족해 보물방 또는 상점방 수를 채우지 못했습니다.");
        }
    }

    private void AssignRoomType(
        List<StageMapNodeData> candidates,
        RoomType roomType,
        int count)
    {
        for (int i = 0; i < count && candidates.Count > 0; i++)
        {
            int index = UnityEngine.Random.Range(0, candidates.Count);
            candidates[index].roomType = roomType;
            candidates.RemoveAt(index);
        }
    }
}