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
    Paused
}

public class GameManager : Singleton<GameManager>
{
    public PlayerController Player { get; private set; }
    public GameState CurrentState { get; private set; } = GameState.Play;

    private CameraTargetBinder cameraBinder;

    public event Action<PlayerController> OnPlayerChanged;

    private void OnEnable()
    {
        EventBus.Subscribe<GameStateRequestEvent>(HandleGameStateRequest);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<GameStateRequestEvent>(HandleGameStateRequest);
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
}
