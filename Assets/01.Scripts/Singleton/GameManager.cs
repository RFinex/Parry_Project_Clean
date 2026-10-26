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


    protected override void AwakeSingleton()
    {
        Inventory = new RuntimeInventorySystem();
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

    public void RegisterPlayer(PlayerController player)
    {
        if (player == null)
            return;

        Player = player;
        OnPlayerChanged?.Invoke(Player);
    }

    public void UnregisterPlayer(PlayerController player)
    {
        if (Player != player)
            return;

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
}
