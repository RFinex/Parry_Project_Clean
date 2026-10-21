using UnityEngine;

public struct GameStateRequestEvent
{
    public GameState state;

    public GameStateRequestEvent(GameState state)
    {
        this.state = state;
    }
}
