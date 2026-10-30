using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public enum GameState
{
    Play,
    Cutscene,
    Map,
    SceneTransition,
    Paused,
    Popup
}

public class GameManager : Singleton<GameManager>
{    
    public PlayerController Player { get; private set; }
    public GameState CurrentState { get; private set; } = GameState.Play;

    private CameraTargetBinder cameraBinder;

    public RuntimeInventorySystem Inventory { get; private set; }

    public event Action<PlayerController> OnPlayerChanged;

    [SerializeField] private ArtifactDataReader artifactReader;

    public ItemFactory ItemFactory { get; private set; }

    protected override void AwakeSingleton()
    {
        Initialized();
    }

    private void OnEnable()
    {
        if (Inventory == null)
            return;

        EventBus.Subscribe<GameStateRequestEvent>(HandleGameStateRequest);
        EventBus.Subscribe<InventoryToggleEvent>(HandleInventoryToggle);
        EventBus.Subscribe<PlayerDiedEvent>(HandlePlayerDied);
        EventBus.Subscribe<StageClearedEvent>(HandleStageCleared);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<GameStateRequestEvent>(HandleGameStateRequest);
        EventBus.UnSubscribe<InventoryToggleEvent>(HandleInventoryToggle);
        EventBus.UnSubscribe<PlayerDiedEvent>(HandlePlayerDied);
        EventBus.UnSubscribe<StageClearedEvent>(HandleStageCleared);
    }

    private void Initialized()
    {
        Inventory = new RuntimeInventorySystem();

        ItemFactory = new ItemFactory();
        ItemFactory.Initialized();
    }

    public void RegisterPlayer(PlayerController player)
    {
        if (player == null)
            return;


        if (Inventory == null)
        {
            Debug.Log("Inventory가 초기화되지 않음");
            return;
        }

        Player = player;
        Inventory.SetPlayerBoard(Player.PlayerBoard);
        SetCameraTarget(Player.transform);
        OnPlayerChanged?.Invoke(Player);
    }

    public void UnregisterPlayer(PlayerController player)
    {
        if (Player != player)
            return;

        Inventory.SetPlayerBoard(null);

        Player = null;
        OnPlayerChanged?.Invoke(null);
    }

    public void RegisterCameraBinder(CameraTargetBinder binder)
    {
        if(binder != null)
            cameraBinder = binder;
    }

    public void UnRegisterCameraBinder(CameraTargetBinder binder)
    {
        if(cameraBinder == binder)
            cameraBinder = null;
    }

    public void SetGameState(GameState state)
    {
        if (CurrentState == state)
            return;

        CurrentState = state;
        EventBus.Publish(new GameStateChangedEvent(CurrentState));
    }

    #region Camera
    public void SetCameraTarget(Transform target)
    {
        if (cameraBinder != null)
            cameraBinder.SetFollowTarget(target);
    }

    public UniTask MoveCameraTargetAsync(Transform target, float duration, CancellationToken ctk)
    {
        if (cameraBinder == null)
            return UniTask.CompletedTask;

        return cameraBinder.MoveToTargetAsync(target, duration, ctk);
    }
    #endregion

    private void HandleGameStateRequest(GameStateRequestEvent eventData)
    {
        SetGameState(eventData.state);
    }    

    /// <summary>
    /// 인벤토리 팝업 토글 이벤트 실행
    /// </summary>
    /// <param name="eventData"></param>
    private void HandleInventoryToggle(InventoryToggleEvent eventData)
    {
        EventBus.Publish(new InventoryTogglePopupEvent(Inventory));
    }

    /// <summary>
    /// 플레이어 사망 시 인벤토리 초기화
    /// </summary>
    /// <param name="eventData"></param>
    private void HandlePlayerDied(PlayerDiedEvent eventData)
    {
        Inventory.Clear();
    }

    /// <summary>
    /// 현재 1스테이지 클리어 후 로비로 돌아오므로 임시로 바로 초기화
    /// </summary>
    /// <param name="eventData"></param>
    private void HandleStageCleared(StageClearedEvent eventData)
    {
        Inventory.Clear();
    }

    public Item CreateItem(int itemId)
    {
        if (ItemFactory == null || artifactReader == null)
            return null;

        Item item = ItemFactory.Create(itemId);

        if (item == null)
            return null;

        if (!artifactReader.TryGetData(itemId, out ArtifactData data))
            return null;

        item.Initialized(data);
        return item;
    }

    public bool TryGiveArtifact(int itemId)
    {
        if (Inventory == null)
            return false;

        Item item = CreateItem(itemId);

        if (item is not Artifact artifact)
            return false;

        return Inventory.TryAdd(artifact);
    }

    public bool TryGetRandomArtifactData(ItemRarity rarity, out ArtifactData selectedData)
    {
        selectedData = null;

        if (artifactReader == null)
            return false;

        // 해당 등급의 아티팩트 갯수 측정
        int matchingCount = 0;

        foreach (ArtifactData data in artifactReader.Artifacts)
        {
            if (data != null && data.Rarity == rarity)
                matchingCount++;
        }

        // 후보가 없으면 실패
        if (matchingCount == 0)
            return false;

        // 후보 중 무작위 하나 선택
        int index = UnityEngine.Random.Range(0, matchingCount);

        // 선택된 후보의 데이터를 찾아서 selectedData에 주입
        foreach (ArtifactData data in artifactReader.Artifacts)
        {
            if (data == null || data.Rarity != rarity)
                continue;

            if (index == 0)
            {
                selectedData = data;
                return true;
            }

            index--;
        }

        return false;
    }
}
