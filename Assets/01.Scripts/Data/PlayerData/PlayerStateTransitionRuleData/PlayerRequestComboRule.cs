using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Transition/Rules/Request Combo")]
public class PlayerRequestComboRule : PlayerTransitionRule
{
    private readonly PlayerRequestComboAction action = new();

    public override IPlayerInputAction Action => action;

    public override Type GetNextState(PlayerBlackBoard board)
    {
        return null;
    }
}
