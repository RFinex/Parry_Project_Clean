public struct GameStateChangedEvent
{
    public GameState state;

    public GameStateChangedEvent(GameState state)
    {
        this.state = state;
    }
}
