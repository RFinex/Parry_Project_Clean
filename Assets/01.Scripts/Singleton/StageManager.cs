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

    private void OnEnable()
    {
        token?.Cancel();
        token?.Dispose();
        token = new CancellationTokenSource();
    }

    private void OnDisable()
    {
        token?.Cancel();
        token?.Dispose();
        token = null;

        RoomManager.Instance.OnRoomClear -= HandleRoomClear;
    }

    private void Start()
    {
        RoomManager.Instance.OnRoomClear += HandleRoomClear;

        foreach (StageMapNodeData node in nodes)
        {
            node.roomPrefab = RoomManager.Instance.GetRandomRoomPrefab(node.roomType);
        }

        map.SetMap(nodes, OnNodeSelected);

        StageMapNodeData startNode = GetNode(startNodeId);

        if (startNode == null || startNode.roomPrefab == null)
        {
            Utils.Log<StageManager>("시작 노드 또는 시작 방 프리팹이 없음");
            return;
        }

        currentNodeId = startNode.id;

        RoomManager.Instance.StartRoom(startNode.roomPrefab);
    }

    public void HandleRoomClear()
    {
        StageMapNodeData currentNode = GetNode(currentNodeId);

        if (currentNode == null || currentNode.nextNodeIds.Count == 0)
            return;

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

    private void OnNodeSelected(int nodeId)
    {
        StageMapNodeData currentNode = GetNode(currentNodeId);

        if (currentNode == null || !currentNode.nextNodeIds.Contains(nodeId))
            return;

        StageMapNodeData selectedNode = GetNode(nodeId);

        if (selectedNode == null || selectedNode.roomPrefab == null)
            return;

        map.HideMap();
        currentNodeId = selectedNode.id;

        RoomManager.Instance.StartRoom(selectedNode.roomPrefab);
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

    protected override void OnDestroy()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.OnRoomClear -= HandleRoomClear;
        }

        base.OnDestroy();
    }
}
