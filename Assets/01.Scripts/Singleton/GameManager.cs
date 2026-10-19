using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public PlayerController Player { get; private set; }

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
}
