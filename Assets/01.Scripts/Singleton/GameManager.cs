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

    public void RegisterPlayer(PlayerController player)
    {
        if (player != null)
            Player = player;
    }

    public void UnregisterPlayer(PlayerController player)
    {
        if(Player == player)
            Player = null;
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

    public void SetCameraTarget(Transform target)
    {
        if (cameraBinder != null)
            cameraBinder.SetFollowTarget(target);
    }
}
