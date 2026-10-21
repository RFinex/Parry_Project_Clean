using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : Singleton<StageManager>
{
    [SerializeField] private int startNodeId;
    [SerializeField] private float mapShowDelay = 2f;

    [SerializeField] private StageMapGenerator mapGenerator = new StageMapGenerator();

    private List<StageMapNodeData> nodes;
    private int currentNodeId;
    
    private CancellationTokenSource token;

    private RoomSystem roomSystem;

    private bool isRoomTransition;

    private void OnEnable()
    {
        token?.Cancel();
        token?.Dispose();
        token = new CancellationTokenSource();

        EventBus.Subscribe<RoomClearedEvent>(HandleRoomClear);
        EventBus.Subscribe<MapNodeSelectedEvent>(HandleMapNodeSelected);
        EventBus.Subscribe<RoomExitInteractedEvent>(HandleRoomExitInteracted);
        EventBus.Subscribe<PlayerDiedEvent>(HandlePlayerDied);
    }

    private void OnDisable()
    {
        token?.Cancel();
        token?.Dispose();
        token = null;

        EventBus.UnSubscribe<RoomClearedEvent>(HandleRoomClear);
        EventBus.UnSubscribe<MapNodeSelectedEvent>(HandleMapNodeSelected);
        EventBus.UnSubscribe<RoomExitInteractedEvent>(HandleRoomExitInteracted);
        EventBus.UnSubscribe<PlayerDiedEvent>(HandlePlayerDied);
    }

    private void Start()
    {
        Debug.Log($"StageManager Start | id={GetInstanceID()} | scene={gameObject.scene.name} | frame={Time.frameCount}");

        if (ServiceLocator.IsRegister<RoomSystem>())
        {
            ServiceLocator.Inject(out roomSystem);
        }

        nodes = mapGenerator.Generate();
        startNodeId = nodes[0].id;

        foreach (StageMapNodeData node in nodes)
        {
            node.roomPrefab = roomSystem.GetRandomRoomPrefab(node.roomType);
            node.icon = roomSystem.GetRoomIcon(node.roomType);
        }

        EventBus.Publish(new StageMapCreatedEvent(nodes));

        StageMapNodeData startNode = GetNode(startNodeId);

        if (startNode == null || startNode.roomPrefab == null)
        {
            Utils.Log<StageManager>("시작 노드 또는 시작 방 프리팹이 없음");
            return;
        }

        currentNodeId = startNode.id;

        if (!roomSystem.StartRoom(startNode.roomPrefab))
            return;

        SceneLoadManager.Instance.SceneReady();
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
            SceneLoadManager.Instance.LoadSceneAsync(SceneName.LobbyScene).Forget();
            return;
        }
    }

    private void HandleRoomExitInteracted(RoomExitInteractedEvent eventData)
    {
        StageMapNodeData currentNode = GetNode(currentNodeId);

        if (currentNode == null)
            return;

        if (currentNode.roomType == RoomType.Boss || currentNode.nextNodeIds.Count == 0)
            return;

        EventBus.Publish(new GameStateRequestEvent(GameState.Map));
        EventBus.Publish(new StageMapShownEvent(currentNode.nextNodeIds));
    }

    private void HandleMapNodeSelected(MapNodeSelectedEvent eventData)
    {
        // 중복 전환 방지
        if (isRoomTransition)
            return;

        int nodeId = eventData.nodeId;

        StageMapNodeData currentNode = GetNode(currentNodeId);

        if (currentNode == null || !currentNode.nextNodeIds.Contains(nodeId))
            return;

        StageMapNodeData selectedNode = GetNode(nodeId);

        if (selectedNode == null || selectedNode.roomPrefab == null)
            return;

        isRoomTransition = true;

        ChangeRoomAsync(selectedNode, currentNodeId, token.Token).Forget();
    }

    private async UniTaskVoid ChangeRoomAsync(StageMapNodeData node, int previousNodeId, CancellationToken ctk)
    {
        bool isRoomChanged = false;

        try
        {
            // 화면 전환 상태
            EventBus.Publish(new GameStateRequestEvent(GameState.SceneTransition));

            // 페이드 아웃
            await roomSystem.FadeOutAsync(ctk);
            ctk.ThrowIfCancellationRequested();

            // 방 불러오기
            currentNodeId = node.id;
            isRoomChanged = roomSystem.StartRoom(node.roomPrefab);

            if (!isRoomChanged)
            {
                currentNodeId = previousNodeId;
                return;
            }

            // 지도 닫기
            EventBus.Publish(new StageMapClosedEvent());
        }
        catch (OperationCanceledException)
        {
            currentNodeId = previousNodeId;
        }
        finally
        {
            // 반드시 페이드 인
            await roomSystem.FadeInAsync(ctk);

            if (!ctk.IsCancellationRequested)
            {
                // 방이 성공적으로 바뀌었으면 Play, 그렇지 않으면 Map 상태
                GameState state = isRoomChanged ? GameState.Play : GameState.Map;
                EventBus.Publish(new GameStateRequestEvent(state));
            }

            isRoomTransition = false;
        }
    }

    private void HandlePlayerDied(PlayerDiedEvent eventData)
    {
        SceneLoadManager.Instance.LoadSceneAsync(SceneName.LobbyScene).Forget();
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
