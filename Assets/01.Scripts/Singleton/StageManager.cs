using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class StageManager : Singleton<StageManager>
{
    [SerializeField] private MapPanel map;
    [SerializeField] private List<StageMapNodeData> nodes;
    [SerializeField] private int startNodeId;
    [SerializeField] private float mapShowDelay = 2f;

    private int currentNodeId;

    private CancellationTokenSource token;

    private RoomManager roomManager;

    private void OnEnable()
    {
        token?.Cancel();
        token?.Dispose();
        token = new CancellationTokenSource();

        EventBus.Subscribe<RoomClearedEvent>(HandleRoomClear);
        EventBus.Subscribe<MapNodeSelectedEvent>(HandleMapNodeSelected);
    }

    private void OnDisable()
    {
        token?.Cancel();
        token?.Dispose();
        token = null;

        EventBus.UnSubscribe<RoomClearedEvent>(HandleRoomClear);
        EventBus.UnSubscribe<MapNodeSelectedEvent>(HandleMapNodeSelected);
    }

    private void Start()
    {
        ServiceLocator.Inject(out roomManager);

        foreach (StageMapNodeData node in nodes)
        {
            node.roomPrefab = roomManager.GetRandomRoomPrefab(node.roomType);
        }

        map.SetMap(nodes);

        StageMapNodeData startNode = GetNode(startNodeId);

        if (startNode == null || startNode.roomPrefab == null)
        {
            Utils.Log<StageManager>("시작 노드 또는 시작 방 프리팹이 없음");
            return;
        }

        currentNodeId = startNode.id;        

        roomManager.StartRoom(startNode.roomPrefab);
        //RoomManager.Instance.StartRoom(startNode.roomPrefab);
    }

    public void HandleRoomClear(RoomClearedEvent eventData)
    {
        StageMapNodeData currentNode = GetNode(currentNodeId);

        if (currentNode == null)
            return;

        if (currentNode.roomType == RoomType.Boss || currentNode.nextNodeIds.Count == 0)
        {
            EventBus.Publish(new StageClearedEvent());
            Utils.Log<StageManager>("스테이지 클리어");
            return;
        }

        ShowMapAfterDelay(currentNode.nextNodeIds, token.Token).Forget();
    }

    private async UniTaskVoid ShowMapAfterDelay(List<int> nextNoideIds, CancellationToken ctk)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(mapShowDelay), cancellationToken: ctk);

            map.ShowNextNodes(nextNoideIds);
        }
        catch (OperationCanceledException)
        {

        }
    }

    private void HandleMapNodeSelected(MapNodeSelectedEvent eventData)
    {
        int nodeId = eventData.nodeId;

        StageMapNodeData currentNode = GetNode(currentNodeId);

        if (currentNode == null || !currentNode.nextNodeIds.Contains(nodeId))
            return;

        StageMapNodeData selectedNode = GetNode(nodeId);

        if (selectedNode == null || selectedNode.roomPrefab == null)
            return;

        map.HideMap();
        currentNodeId = selectedNode.id;

        roomManager.StartRoom(selectedNode.roomPrefab);
    }

    private StageMapNodeData GetNode(int id)
    {
        foreach (StageMapNodeData node in nodes)
        {
            if (node.id == id)
                return node;
        }

        return null;
    }
}
